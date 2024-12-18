using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
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
        /// <returns>Image object.</returns>
        Task<Image<Rgba32>> LoadImageAsync(string imageUrl);

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        /// <param name="jpegQuality">Target image quality.</param>
        Task SaveImageAsync(string imageUrl, Image<Rgba32> image, IImageFormat format, JpegQuality jpegQuality);

        /// <summary>
        /// Defines if given file extension is allowed for image processing
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> IsFileExtensionAllowedAsync(string path);

        /// <summary>
        /// Defines if given image format is allowed for image processing
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        Task<bool> IsImageFormatAllowedAsync(IImageFormat format);
    }
}
