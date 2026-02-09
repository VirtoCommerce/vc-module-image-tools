using System;
using System.Globalization;

namespace VirtoCommerce.ImageToolsModule.Core.Models
{
    /// <summary>
    /// Represents the dimensions of an SVG image
    /// </summary>
    public class SvgDimensions
    {
        /// <summary>
        /// Width attribute value (may be null if not specified)
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Height attribute value (may be null if not specified)
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// ViewBox attribute value (e.g., "0 0 100 100")
        /// </summary>
        public string ViewBox { get; set; }

        /// <summary>
        /// Gets the effective width from viewBox if Width is not set
        /// </summary>
        public int EffectiveWidth => Width ?? ParseViewBoxWidth() ?? 100;

        /// <summary>
        /// Gets the effective height from viewBox if Height is not set
        /// </summary>
        public int EffectiveHeight => Height ?? ParseViewBoxHeight() ?? 100;

        private int? ParseViewBoxWidth()
        {
            if (string.IsNullOrEmpty(ViewBox))
            {
                return null;
            }

            var parts = ViewBox.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
            {
                return (int)Math.Round(width);
            }

            return null;
        }

        private int? ParseViewBoxHeight()
        {
            if (string.IsNullOrEmpty(ViewBox))
            {
                return null;
            }

            var parts = ViewBox.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4 && double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var height))
            {
                return (int)Math.Round(height);
            }

            return null;
        }
    }
}
