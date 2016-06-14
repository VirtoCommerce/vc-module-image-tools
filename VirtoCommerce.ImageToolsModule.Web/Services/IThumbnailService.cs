
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Web.Models;

namespace VirtoCommerce.ImageToolsModule.Web.Services
{
    /// <summary>
    /// Thumbnail of an image is an image with another size(resolution). 
    /// The thumbnails size is less of original size.
    /// Thumbnails are using in an interface, where it doesn't need high resolution.
    /// For example, in listings, short views.
    /// The service allows to generate different thumbnails, get list of existed thumbnails.
    /// </summary>
    public interface IThumbnailService
    {
        /// <summary>
        /// Generate different thumbnails by given image url.
        /// </summary>
        /// <param name="imageUrl">Original image.</param>
        /// <param name="thumbnailsParameters">ImageTools.ImageSizes settings.</param>
        /// <param name="isRegenerateAll">True to replace all existed thumbnails with a new ones.</param>
        Task<bool> GenerateAsync(string imageUrl, string[] thumbnailsParameters, bool isRegenerateAll);

        /// <summary>
        /// Get all existed thumbnails urls of given image.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="aliases">Thumbnails aliases (suffixes).</param>
        /// <returns>List of existed thumbnails.</returns>
        string[] GetThumbnails(string imageUrl, string[] aliases);
    }
}