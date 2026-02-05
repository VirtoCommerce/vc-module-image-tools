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
    /// Default implementation of SVG resizer using XML manipulation.
    /// SVG resizing works differently from raster - we modify width/height attributes
    /// while preserving the viewBox for proper scaling.
    /// </summary>
    public class DefaultSvgResizer : ISvgResizer
    {
        private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
        private readonly ILogger<DefaultSvgResizer> _logger;

        public DefaultSvgResizer(ILogger<DefaultSvgResizer> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string Resize(string svgContent, int? width, int? height, ResizeMethod method)
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

                // Ensure viewBox exists for proper scaling
                svgContent = EnsureViewBox(svgContent);
                doc = XDocument.Parse(svgContent);
                svg = doc.Root;

                // Get current dimensions
                var dimensions = ParseDimensions(svgContent);
                var currentWidth = dimensions.EffectiveWidth;
                var currentHeight = dimensions.EffectiveHeight;

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
            var numericPart = Regex.Replace(value, @"[^\d.]", "");

            if (double.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return (int)Math.Round(result);
            }

            return null;
        }

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
                ResizeMethod.Crop => (targetWidth ?? currentWidth, targetHeight ?? currentHeight),
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
