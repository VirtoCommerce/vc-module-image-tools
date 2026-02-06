using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// SVG resizer using XML manipulation.
    /// SVG resizing works differently from raster - we modify width/height attributes
    /// while preserving the viewBox for proper scaling.
    /// </summary>
    public partial class SvgResizer : ISvgResizer
    {
        private readonly ILogger<SvgResizer> _logger;

        public SvgResizer(ILogger<SvgResizer> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string Resize(string svgContent, int? width, int? height, ResizeMethod method, AnchorPosition anchorPosition = AnchorPosition.Center)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                return svgContent;
            }

            try
            {
                // Ensure viewBox exists for proper scaling
                svgContent = EnsureViewBox(svgContent);
                var doc = XDocument.Parse(svgContent);
                var svg = doc.Root;

                if (svg == null)
                {
                    return svgContent;
                }

                // Get current dimensions from viewBox
                var dimensions = ParseDimensions(svgContent);
                var currentWidth = dimensions.EffectiveWidth;
                var currentHeight = dimensions.EffectiveHeight;

                if (method == ResizeMethod.Crop)
                {
                    // For crop, modify the viewBox to show only the cropped portion
                    return ApplyCrop(svg, doc, currentWidth, currentHeight, width ?? currentWidth, height ?? currentHeight, anchorPosition);
                }

                // Calculate new dimensions based on resize method
                var (newWidth, newHeight) = CalculateNewDimensions(
                    currentWidth, currentHeight, width, height, method);

                // Apply new dimensions
                if (newWidth.HasValue)
                {
                    svg.SetAttributeValue("width", newWidth.Value);
                }

                if (newHeight.HasValue)
                {
                    svg.SetAttributeValue("height", newHeight.Value);
                }

                return doc.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resizing SVG");
                return svgContent;
            }
        }

        private static string ApplyCrop(XElement svg, XDocument doc, int currentWidth, int currentHeight, int targetWidth, int targetHeight, AnchorPosition anchor)
        {
            // Calculate the viewBox crop region based on anchor position
            var (viewBoxX, viewBoxY) = CalculateCropOffset(currentWidth, currentHeight, targetWidth, targetHeight, anchor);

            // Clamp the crop region to valid bounds
            var cropWidth = Math.Min(targetWidth, currentWidth);
            var cropHeight = Math.Min(targetHeight, currentHeight);
            viewBoxX = Math.Max(0, Math.Min(viewBoxX, currentWidth - cropWidth));
            viewBoxY = Math.Max(0, Math.Min(viewBoxY, currentHeight - cropHeight));

            // Set new viewBox to show only the cropped portion
            svg.SetAttributeValue("viewBox", $"{viewBoxX} {viewBoxY} {cropWidth} {cropHeight}");

            // Set output dimensions to target size
            svg.SetAttributeValue("width", targetWidth);
            svg.SetAttributeValue("height", targetHeight);

            return doc.ToString();
        }

        private static (int X, int Y) CalculateCropOffset(int currentWidth, int currentHeight, int targetWidth, int targetHeight, AnchorPosition anchor)
        {
            var excessWidth = Math.Max(0, currentWidth - targetWidth);
            var excessHeight = Math.Max(0, currentHeight - targetHeight);

            return anchor switch
            {
                AnchorPosition.TopLeft => (0, 0),
                AnchorPosition.TopCenter => (excessWidth / 2, 0),
                AnchorPosition.TopRight => (excessWidth, 0),
                AnchorPosition.CenterLeft => (0, excessHeight / 2),
                AnchorPosition.Center => (excessWidth / 2, excessHeight / 2),
                AnchorPosition.CenterRight => (excessWidth, excessHeight / 2),
                AnchorPosition.BottomLeft => (0, excessHeight),
                AnchorPosition.BottomCenter => (excessWidth / 2, excessHeight),
                AnchorPosition.BottomRight => (excessWidth, excessHeight),
                _ => (excessWidth / 2, excessHeight / 2)
            };
        }

        /// <inheritdoc />
        public string SetDimensions(string svgContent, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                return svgContent;
            }

            try
            {
                var doc = XDocument.Parse(svgContent);
                var svg = doc.Root;

                if (svg == null)
                {
                    return svgContent;
                }

                svg.SetAttributeValue("width", width);
                svg.SetAttributeValue("height", height);

                return doc.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting SVG dimensions");
                return svgContent;
            }
        }

        /// <inheritdoc />
        public string EnsureViewBox(string svgContent)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                return svgContent;
            }

            try
            {
                var doc = XDocument.Parse(svgContent);
                var svg = doc.Root;

                if (svg == null)
                {
                    return svgContent;
                }

                // Check if viewBox already exists
                if (svg.Attribute("viewBox") != null)
                {
                    return svgContent;
                }

                // Try to get dimensions from width/height attributes
                var widthAttr = svg.Attribute("width")?.Value;
                var heightAttr = svg.Attribute("height")?.Value;

                var width = ParseDimensionValue(widthAttr) ?? 100;
                var height = ParseDimensionValue(heightAttr) ?? 100;

                svg.SetAttributeValue("viewBox", $"0 0 {width} {height}");

                return doc.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring SVG viewBox");
                return svgContent;
            }
        }

        /// <inheritdoc />
        public SvgDimensions ParseDimensions(string svgContent)
        {
            var dimensions = new SvgDimensions();

            if (string.IsNullOrEmpty(svgContent))
            {
                return dimensions;
            }

            try
            {
                var doc = XDocument.Parse(svgContent);
                var svg = doc.Root;

                if (svg == null)
                {
                    return dimensions;
                }

                // Parse viewBox
                dimensions.ViewBox = svg.Attribute("viewBox")?.Value;

                // Parse width
                var widthValue = svg.Attribute("width")?.Value;
                dimensions.Width = ParseDimensionValue(widthValue);

                // Parse height
                var heightValue = svg.Attribute("height")?.Value;
                dimensions.Height = ParseDimensionValue(heightValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SVG dimensions");
            }

            return dimensions;
        }

        private static int? ParseDimensionValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            // Remove units (px, em, pt, etc.)
            var numericPart = NonNumericRegex().Replace(value, "");

            if (double.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return (int)Math.Round(result);
            }

            return null;
        }

        [GeneratedRegex(@"[^\d.]")]
        private static partial Regex NonNumericRegex();

        private static (int? Width, int? Height) CalculateNewDimensions(
            int currentWidth, int currentHeight,
            int? targetWidth, int? targetHeight,
            ResizeMethod method)
        {
            return method switch
            {
                ResizeMethod.FixedSize => CalculateFixedSize(currentWidth, currentHeight, targetWidth, targetHeight),
                ResizeMethod.FixedWidth => CalculateFixedWidth(currentWidth, currentHeight, targetWidth),
                ResizeMethod.FixedHeight => CalculateFixedHeight(currentWidth, currentHeight, targetHeight),
                //ResizeMethod.Crop => (targetWidth ?? currentWidth, targetHeight ?? currentHeight),
                _ => (targetWidth, targetHeight)
            };
        }

        private static (int? Width, int? Height) CalculateFixedSize(
            int currentWidth, int currentHeight,
            int? targetWidth, int? targetHeight)
        {
            if (!targetWidth.HasValue || !targetHeight.HasValue)
            {
                return (targetWidth ?? currentWidth, targetHeight ?? currentHeight);
            }

            // Calculate scale to fit within target dimensions while maintaining aspect ratio
            var scaleX = (double)targetWidth.Value / currentWidth;
            var scaleY = (double)targetHeight.Value / currentHeight;
            var scale = Math.Min(scaleX, scaleY);

            return ((int)Math.Round(currentWidth * scale), (int)Math.Round(currentHeight * scale));
        }

        private static (int? Width, int? Height) CalculateFixedWidth(
            int currentWidth, int currentHeight,
            int? targetWidth)
        {
            if (!targetWidth.HasValue || currentWidth == 0)
            {
                return (targetWidth, null);
            }

            var scale = (double)targetWidth.Value / currentWidth;
            var newHeight = (int)Math.Round(currentHeight * scale);

            return (targetWidth, newHeight);
        }

        private static (int? Width, int? Height) CalculateFixedHeight(
            int currentWidth, int currentHeight,
            int? targetHeight)
        {
            if (!targetHeight.HasValue || currentHeight == 0)
            {
                return (null, targetHeight);
            }

            var scale = (double)targetHeight.Value / currentHeight;
            var newWidth = (int)Math.Round(currentWidth * scale);

            return (newWidth, targetHeight);
        }
    }
}
