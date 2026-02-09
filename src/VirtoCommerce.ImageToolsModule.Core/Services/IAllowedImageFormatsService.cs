using System.Threading.Tasks;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// Service for checking if image formats are allowed for processing.
    /// </summary>
    public interface IAllowedImageFormatsService
    {
        /// <summary>
        /// Checks if the file at the given URL has an allowed image format.
        /// </summary>
        /// <param name="url">The file URL or path to check.</param>
        /// <returns>True if the file extension is in the allowed formats list.</returns>
        Task<bool> IsAllowedAsync(string url);
    }
}
