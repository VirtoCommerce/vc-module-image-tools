using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IImageResizer
    {
        /// <summary>
        /// Scale image by given percent
        /// </summary>
        RasterImage ScaleByPercent(RasterImage image, int percent);

        /// <summary>
        /// Resize image vertically with keeping it aspect rate.
        /// </summary>
        RasterImage FixedHeight(RasterImage image, int height, ImageColor background);

        /// <summary>
        /// Resize image horizontally with keeping it aspect rate
        /// </summary>
        RasterImage FixedWidth(RasterImage image, int width, ImageColor background);

        /// <summary>
        /// Resize image.
        /// Original image will be resized proportionally to fit given Width, Height.
        /// Original image will not be cropped.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right).
        /// The empty spaces will be filled with given color.
        /// </summary>
        RasterImage FixedSize(RasterImage image, int width, int height, ImageColor background);

        /// <summary>
        /// Resize and trim excess.
        /// The image will have given size
        /// </summary>
        RasterImage Crop(RasterImage image, int width, int height, AnchorPosition anchor);
    }
}
