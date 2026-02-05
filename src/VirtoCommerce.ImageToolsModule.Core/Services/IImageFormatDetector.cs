using System.IO;
using System.Threading.Tasks;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// Detects the format type of an image from its URL or content
    /// </summary>
    public interface IImageFormatDetector
    {
        /// <summary>
        /// Detect format type from file extension
        /// </summary>
        /// <param name="imageUrl">The image URL or path</param>
        /// <returns>The detected image format type</returns>
        ImageFormatType DetectFormatType(string imageUrl);

        /// <summary>
        /// Detect format type from stream content
        /// </summary>
        /// <param name="stream">The image stream</param>
        /// <returns>The detected image format type</returns>
        Task<ImageFormatType> DetectFormatTypeAsync(Stream stream);

        /// <summary>
        /// Check if the format is supported for processing
        /// </summary>
        /// <param name="imageUrl">The image URL or path</param>
        /// <returns>True if the format is supported</returns>
        Task<bool> IsFormatSupportedAsync(string imageUrl);
    }
}
