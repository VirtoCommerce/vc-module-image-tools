using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    /// <summary>
    /// Work with image
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        Task<Image> LoadImageAsync(string imageUrl);

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="imageFormat">ImageFormat object.</param>
        /// <param name="jpegQuality">JpegQuality object.</param>
        Task SaveImage(string imageUrl, Image image, ImageFormat imageFormat, JpegQuality jpegQuality);
    }
}
