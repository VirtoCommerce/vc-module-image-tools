using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// SVG resizing operations. Unlike raster resizing, SVG resizing modifies
    /// the width/height attributes while preserving the viewBox for scalability.
    /// </summary>
    public interface ISvgResizer
    {
        /// <summary>
        /// Resize SVG by modifying width/height attributes according to the resize method
        /// </summary>
        /// <param name="svgContent">The original SVG content</param>
        /// <param name="width">Target width (may be null depending on method)</param>
        /// <param name="height">Target height (may be null depending on method)</param>
        /// <param name="method">The resize method to apply</param>
        /// <returns>The resized SVG content</returns>
        string Resize(string svgContent, int? width, int? height, ResizeMethod method);

        /// <summary>
        /// Set explicit dimensions on SVG
        /// </summary>
        /// <param name="svgContent">The original SVG content</param>
        /// <param name="width">The width to set</param>
        /// <param name="height">The height to set</param>
        /// <returns>The SVG content with updated dimensions</returns>
        string SetDimensions(string svgContent, int width, int height);

        /// <summary>
        /// Ensure SVG has a viewBox attribute. If missing, adds one based on width/height.
        /// </summary>
        /// <param name="svgContent">The original SVG content</param>
        /// <returns>The SVG content with viewBox ensured</returns>
        string EnsureViewBox(string svgContent);

        /// <summary>
        /// Parse dimensions from SVG content without loading from storage
        /// </summary>
        /// <param name="svgContent">The SVG content</param>
        /// <returns>The parsed dimensions</returns>
        SvgDimensions ParseDimensions(string svgContent);
    }
}
