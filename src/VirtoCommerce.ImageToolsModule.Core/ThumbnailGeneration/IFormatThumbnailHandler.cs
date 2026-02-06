using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    /// <summary>
    /// Format-specific thumbnail generation handler.
    /// Implement this interface to add support for new image formats.
    /// </summary>
    public interface IFormatThumbnailHandler
    {
        /// <summary>
        /// Priority for handler selection. Higher values are preferred.
        /// Use 0 for default handlers, positive values for specialized handlers.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this handler can process the given image
        /// </summary>
        /// <param name="imageUrl">The image URL or path</param>
        /// <returns>True if this handler can process the image</returns>
        Task<bool> CanHandleAsync(string imageUrl);

        /// <summary>
        /// Generate thumbnails for the image
        /// </summary>
        /// <param name="source">Source image URL</param>
        /// <param name="destination">Destination path for thumbnails</param>
        /// <param name="options">Thumbnail options defining sizes and methods</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Result containing generated thumbnail URLs and any errors</returns>
        Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
            string source,
            string destination,
            IList<ThumbnailOption> options,
            ICancellationToken token);
    }
}
