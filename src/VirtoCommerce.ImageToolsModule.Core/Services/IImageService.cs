using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// Work with image
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Loads Image from blob storage
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object, or null when the blob is missing, corrupt or not in a supported format.</returns>
        Task<RasterImage> LoadImageAsync(string imageUrl);

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        /// <param name="jpegQuality">Target image quality.</param>
        Task SaveImageAsync(string imageUrl, RasterImage image, RasterImageFormat format, JpegQuality jpegQuality);
    }
}
