using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// Service for SVG-specific operations including loading and saving SVG content
    /// </summary>
    public interface ISvgService
    {
        /// <summary>
        /// Load SVG content from storage
        /// </summary>
        /// <param name="svgUrl">The SVG file URL or path</param>
        /// <returns>The SVG content as string, or null if not found</returns>
        Task<string> LoadSvgAsync(string svgUrl);

        /// <summary>
        /// Save SVG content to storage
        /// </summary>
        /// <param name="svgUrl">The destination URL or path</param>
        /// <param name="svgContent">The SVG content to save</param>
        Task SaveSvgAsync(string svgUrl, string svgContent);

        /// <summary>
        /// Get the dimensions of an SVG (width, height, and viewBox attributes)
        /// </summary>
        /// <param name="svgUrl">The SVG file URL or path</param>
        /// <returns>The SVG dimensions</returns>
        Task<SvgDimensions> GetDimensionsAsync(string svgUrl);

        /// <summary>
        /// Check if the given URL points to an SVG file
        /// </summary>
        /// <param name="url">The file URL or path</param>
        /// <returns>True if the file is an SVG</returns>
        bool IsSvgFile(string url);
    }
}
