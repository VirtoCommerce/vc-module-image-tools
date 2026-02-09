using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class SvgResizerTests
    {
        private readonly SvgResizer _svgResizer;

        public SvgResizerTests()
        {
            _svgResizer = new SvgResizer(Mock.Of<ILogger<SvgResizer>>());
        }

        #region ParseDimensions Tests

        [Fact]
        public void ParseDimensions_WithWidthAndHeight_ReturnsCorrectValues()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var dimensions = _svgResizer.ParseDimensions(svg);

            // Assert
            Assert.Equal(200, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        [Fact]
        public void ParseDimensions_WithViewBox_ReturnsViewBox()
        {
            // Arrange
            var svg = @"<svg viewBox=""0 0 300 150"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var dimensions = _svgResizer.ParseDimensions(svg);

            // Assert
            Assert.Equal("0 0 300 150", dimensions.ViewBox);
            Assert.Equal(300, dimensions.EffectiveWidth);
            Assert.Equal(150, dimensions.EffectiveHeight);
        }

        [Fact]
        public void ParseDimensions_WithPixelUnits_ParsesCorrectly()
        {
            // Arrange
            var svg = @"<svg width=""200px"" height=""100px"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var dimensions = _svgResizer.ParseDimensions(svg);

            // Assert
            Assert.Equal(200, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        [Fact]
        public void ParseDimensions_WithNullContent_ReturnsEmptyDimensions()
        {
            // Act
            var dimensions = _svgResizer.ParseDimensions(null);

            // Assert
            Assert.Null(dimensions.Width);
            Assert.Null(dimensions.Height);
            Assert.Null(dimensions.ViewBox);
        }

        [Fact]
        public void ParseDimensions_WithEmptyContent_ReturnsEmptyDimensions()
        {
            // Act
            var dimensions = _svgResizer.ParseDimensions(string.Empty);

            // Assert
            Assert.Null(dimensions.Width);
            Assert.Null(dimensions.Height);
        }

        [Fact]
        public void ParseDimensions_WithDecimalValues_RoundsCorrectly()
        {
            // Arrange
            var svg = @"<svg width=""200.7"" height=""100.3"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var dimensions = _svgResizer.ParseDimensions(svg);

            // Assert
            Assert.Equal(201, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        #endregion

        #region EnsureViewBox Tests

        [Fact]
        public void EnsureViewBox_WithoutViewBox_AddsViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.EnsureViewBox(svg);

            // Assert
            Assert.Contains("viewBox=\"0 0 200 100\"", result);
        }

        [Fact]
        public void EnsureViewBox_WithExistingViewBox_DoesNotModify()
        {
            // Arrange
            var svg = @"<svg viewBox=""10 10 200 100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.EnsureViewBox(svg);

            // Assert
            Assert.Contains("viewBox=\"10 10 200 100\"", result);
        }

        [Fact]
        public void EnsureViewBox_WithoutDimensions_UsesDefault()
        {
            // Arrange
            var svg = @"<svg xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.EnsureViewBox(svg);

            // Assert
            Assert.Contains("viewBox=\"0 0 100 100\"", result);
        }

        [Fact]
        public void EnsureViewBox_WithNullContent_ReturnsNull()
        {
            // Act
            var result = _svgResizer.EnsureViewBox(null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region SetDimensions Tests

        [Fact]
        public void SetDimensions_SetsWidthAndHeight()
        {
            // Arrange
            var svg = @"<svg width=""100"" height=""50"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.SetDimensions(svg, 300, 200);

            // Assert
            Assert.Contains("width=\"300\"", result);
            Assert.Contains("height=\"200\"", result);
        }

        [Fact]
        public void SetDimensions_WithNullContent_ReturnsNull()
        {
            // Act
            var result = _svgResizer.SetDimensions(null, 100, 100);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SetDimensions_AddsAttributesIfMissing()
        {
            // Arrange
            var svg = @"<svg xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.SetDimensions(svg, 150, 75);

            // Assert
            Assert.Contains("width=\"150\"", result);
            Assert.Contains("height=\"75\"", result);
        }

        #endregion

        #region Resize Tests - FixedSize

        [Fact]
        public void Resize_FixedSize_MaintainsAspectRatio()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" viewBox=""0 0 200 100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.FixedSize);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert - should fit within 100x100 while maintaining 2:1 aspect ratio
            Assert.Equal(100, dimensions.Width);
            Assert.Equal(50, dimensions.Height);
        }

        [Fact]
        public void Resize_FixedSize_TallImage_FitsInBounds()
        {
            // Arrange
            var svg = @"<svg width=""100"" height=""200"" viewBox=""0 0 100 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.FixedSize);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert - should fit within 100x100 while maintaining 1:2 aspect ratio
            Assert.Equal(50, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        #endregion

        #region Resize Tests - FixedWidth

        [Fact]
        public void Resize_FixedWidth_ScalesProportionally()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" viewBox=""0 0 200 100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, null, ResizeMethod.FixedWidth);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert
            Assert.Equal(100, dimensions.Width);
            Assert.Equal(50, dimensions.Height);
        }

        [Fact]
        public void Resize_FixedWidth_ScalesUp()
        {
            // Arrange
            var svg = @"<svg width=""100"" height=""50"" viewBox=""0 0 100 50"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 200, null, ResizeMethod.FixedWidth);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert
            Assert.Equal(200, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        #endregion

        #region Resize Tests - FixedHeight

        [Fact]
        public void Resize_FixedHeight_ScalesProportionally()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" viewBox=""0 0 200 100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, null, 50, ResizeMethod.FixedHeight);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert
            Assert.Equal(100, dimensions.Width);
            Assert.Equal(50, dimensions.Height);
        }

        [Fact]
        public void Resize_FixedHeight_ScalesUp()
        {
            // Arrange
            var svg = @"<svg width=""100"" height=""50"" viewBox=""0 0 100 50"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, null, 100, ResizeMethod.FixedHeight);
            var dimensions = _svgResizer.ParseDimensions(result);

            // Assert
            Assert.Equal(200, dimensions.Width);
            Assert.Equal(100, dimensions.Height);
        }

        #endregion

        #region Resize Tests - Crop

        [Fact]
        public void Resize_Crop_Center_ModifiesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""200"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.Crop, AnchorPosition.Center);

            // Assert
            Assert.Contains("viewBox=\"50 50 100 100\"", result);
            Assert.Contains("width=\"100\"", result);
            Assert.Contains("height=\"100\"", result);
        }

        [Fact]
        public void Resize_Crop_TopLeft_ModifiesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""200"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.Crop, AnchorPosition.TopLeft);

            // Assert
            Assert.Contains("viewBox=\"0 0 100 100\"", result);
        }

        [Fact]
        public void Resize_Crop_BottomRight_ModifiesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""200"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.Crop, AnchorPosition.BottomRight);

            // Assert
            Assert.Contains("viewBox=\"100 100 100 100\"", result);
        }

        [Fact]
        public void Resize_Crop_TopCenter_ModifiesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""200"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.Crop, AnchorPosition.TopCenter);

            // Assert
            Assert.Contains("viewBox=\"50 0 100 100\"", result);
        }

        [Fact]
        public void Resize_Crop_BottomCenter_ModifiesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""200"" viewBox=""0 0 200 200"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, 100, ResizeMethod.Crop, AnchorPosition.BottomCenter);

            // Assert
            Assert.Contains("viewBox=\"50 100 100 100\"", result);
        }

        [Fact]
        public void Resize_Crop_LargerThanOriginal_ClampsToOriginalSize()
        {
            // Arrange
            var svg = @"<svg width=""100"" height=""100"" viewBox=""0 0 100 100"" xmlns=""http://www.w3.org/2000/svg""></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 200, 200, ResizeMethod.Crop, AnchorPosition.Center);

            // Assert - viewBox width/height should be clamped to original size (100)
            Assert.Contains("viewBox=\"0 0 100 100\"", result);
            Assert.Contains("width=\"200\"", result);
            Assert.Contains("height=\"200\"", result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Resize_WithNullContent_ReturnsNull()
        {
            // Act
            var result = _svgResizer.Resize(null, 100, 100, ResizeMethod.FixedSize);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resize_WithEmptyContent_ReturnsEmpty()
        {
            // Act
            var result = _svgResizer.Resize(string.Empty, 100, 100, ResizeMethod.FixedSize);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Resize_WithInvalidXml_ReturnsOriginal()
        {
            // Arrange
            var invalidSvg = "not valid xml <svg";

            // Act
            var result = _svgResizer.Resize(invalidSvg, 100, 100, ResizeMethod.FixedSize);

            // Assert - should return original on error
            Assert.Equal(invalidSvg, result);
        }

        [Fact]
        public void Resize_PreservesViewBox()
        {
            // Arrange
            var svg = @"<svg width=""200"" height=""100"" viewBox=""0 0 200 100"" xmlns=""http://www.w3.org/2000/svg""><rect x=""0"" y=""0"" width=""200"" height=""100""/></svg>";

            // Act
            var result = _svgResizer.Resize(svg, 100, null, ResizeMethod.FixedWidth);

            // Assert - viewBox should be preserved, only width/height attributes change
            Assert.Contains("viewBox=\"0 0 200 100\"", result);
        }

        #endregion
    }
}
