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
        /// <param name="codecInfo">Image codec info.</param>
        /// <param name="encoderParams">Image encoder parameters.</param>
        Task SaveImage(string imageUrl, Image image, ImageCodecInfo codecInfo, EncoderParameters encoderParams = null);

        /// <summary>
        /// Get codec info by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        ImageCodecInfo GetImageCodecInfo(Image image);

        /// <summary>
        /// Get encoder parameters by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        EncoderParameters GetEncoderParameters(Image image, ThumbnailOption option);

        /// <summary>
        /// Get Jpeg quality parameter by JpegQuality object.
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        long GetJpegQualityParameter(JpegQuality quality);
    }
}
