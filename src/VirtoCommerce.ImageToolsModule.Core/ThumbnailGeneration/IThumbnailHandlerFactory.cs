using System.Threading.Tasks;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    /// <summary>
    /// Factory for selecting appropriate thumbnail handler based on image format.
    /// Routes image processing to format-specific handlers.
    /// </summary>
    public interface IThumbnailHandlerFactory
    {
        /// <summary>
        /// Get the appropriate handler for the given image URL
        /// </summary>
        /// <param name="imageUrl">The image URL or path</param>
        /// <returns>The handler that can process this image, or null if no handler found</returns>
        Task<IFormatThumbnailHandler> GetHandlerAsync(string imageUrl);

        /// <summary>
        /// Register a new format handler at runtime
        /// </summary>
        /// <param name="handler">The handler to register</param>
        void RegisterHandler(IFormatThumbnailHandler handler);
    }
}
