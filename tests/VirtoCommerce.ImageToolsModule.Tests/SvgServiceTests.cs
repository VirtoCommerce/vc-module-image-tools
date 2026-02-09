using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class SvgServiceTests
    {
        private readonly Mock<IBlobStorageProvider> _storageProviderMock;
        private readonly Mock<ISvgResizer> _svgResizerMock;
        private readonly SvgService _svgService;

        public SvgServiceTests()
        {
            _storageProviderMock = new Mock<IBlobStorageProvider>();
            _svgResizerMock = new Mock<ISvgResizer>();
            _svgService = new SvgService(
                _storageProviderMock.Object,
                _svgResizerMock.Object,
                Mock.Of<ILogger<SvgService>>());
        }

        #region IsSvgFile Tests

        [Theory]
        [InlineData("image.svg", true)]
        [InlineData("image.SVG", true)]
        [InlineData("image.Svg", true)]
        [InlineData("path/to/image.svg", true)]
        [InlineData("image.png", false)]
        [InlineData("image.jpg", false)]
        [InlineData("image.jpeg", false)]
        [InlineData("image.webp", false)]
        [InlineData("image.gif", false)]
        [InlineData("svg.png", false)]
        [InlineData("mysvg", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsSvgFile_VariousInputs_ReturnsExpected(string url, bool expected)
        {
            // Act
            var result = _svgService.IsSvgFile(url);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsSvgFile_WithQueryString_ReturnsTrue()
        {
            // Arrange
            var url = "image.svg?v=123";

            var result = _svgService.IsSvgFile(url);

            Assert.True(result);
        }

        #endregion

        #region LoadSvgAsync Tests

        [Fact]
        public async Task LoadSvgAsync_Success_ReturnsSvgContent()
        {
            // Arrange
            var svgUrl = "test/image.svg";
            var expectedContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100""></svg>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ReturnsAsync(stream);

            // Act
            var result = await _svgService.LoadSvgAsync(svgUrl);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task LoadSvgAsync_FileNotFound_ReturnsNull()
        {
            // Arrange
            var svgUrl = "nonexistent.svg";
            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ThrowsAsync(new FileNotFoundException());

            // Act
            var result = await _svgService.LoadSvgAsync(svgUrl);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoadSvgAsync_WithUtf8Content_HandlesCorrectly()
        {
            // Arrange
            var svgUrl = "test/unicode.svg";
            var expectedContent = @"<svg xmlns=""http://www.w3.org/2000/svg""><text>Unicode: \u00e9\u00e8\u00ea</text></svg>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ReturnsAsync(stream);

            // Act
            var result = await _svgService.LoadSvgAsync(svgUrl);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        #endregion

        #region SaveSvgAsync Tests

        [Fact]
        public async Task SaveSvgAsync_Success_WritesContent()
        {
            // Arrange
            var svgUrl = "test/output.svg";
            var svgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100""></svg>";
            var outputStream = new MemoryStream();

            _storageProviderMock
                .Setup(x => x.OpenWriteAsync(svgUrl))
                .ReturnsAsync(outputStream);

            // Act
            await _svgService.SaveSvgAsync(svgUrl, svgContent);

            // Assert
            _storageProviderMock.Verify(x => x.OpenWriteAsync(svgUrl), Times.Once);
        }

        [Fact]
        public async Task SaveSvgAsync_StorageError_ThrowsException()
        {
            // Arrange
            var svgUrl = "test/output.svg";
            var svgContent = @"<svg></svg>";

            _storageProviderMock
                .Setup(x => x.OpenWriteAsync(svgUrl))
                .ThrowsAsync(new IOException("Storage error"));

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => _svgService.SaveSvgAsync(svgUrl, svgContent));
        }

        #endregion

        #region GetDimensionsAsync Tests

        [Fact]
        public async Task GetDimensionsAsync_ValidSvg_ReturnsDimensions()
        {
            // Arrange
            var svgUrl = "test/image.svg";
            var svgContent = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""200"" height=""100""></svg>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent));
            var expectedDimensions = new SvgDimensions { Width = 200, Height = 100 };

            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ReturnsAsync(stream);

            _svgResizerMock
                .Setup(x => x.ParseDimensions(svgContent))
                .Returns(expectedDimensions);

            // Act
            var result = await _svgService.GetDimensionsAsync(svgUrl);

            // Assert
            Assert.Equal(200, result.Width);
            Assert.Equal(100, result.Height);
        }

        [Fact]
        public async Task GetDimensionsAsync_FileNotFound_ReturnsEmptyDimensions()
        {
            // Arrange
            var svgUrl = "nonexistent.svg";
            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ThrowsAsync(new FileNotFoundException());

            // Act
            var result = await _svgService.GetDimensionsAsync(svgUrl);

            // Assert
            Assert.Null(result.Width);
            Assert.Null(result.Height);
        }

        [Fact]
        public async Task GetDimensionsAsync_CallsResizerParseDimensions()
        {
            // Arrange
            var svgUrl = "test/image.svg";
            var svgContent = @"<svg></svg>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent));

            _storageProviderMock
                .Setup(x => x.OpenReadAsync(svgUrl))
                .ReturnsAsync(stream);

            _svgResizerMock
                .Setup(x => x.ParseDimensions(svgContent))
                .Returns(new SvgDimensions());

            // Act
            await _svgService.GetDimensionsAsync(svgUrl);

            // Assert
            _svgResizerMock.Verify(x => x.ParseDimensions(svgContent), Times.Once);
        }

        #endregion
    }
}
