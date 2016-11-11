using System.Drawing;
using VirtoCommerce.ImageToolsModule.Data.Models;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Image resize library interface
    /// </summary>
    public interface IImageResizer
    {
        /// <summary>
        /// Resize the image to the desired size while maintaining the aspect ratio without cropping.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right). 
        /// The empty spaces will be filled with given color.
        /// </summary>
        Image FixedSize(Image image, int width, int height, Color backgroung);

        /// <summary>
        /// Resize the image to the desired height while maintaining the aspect ratio.
        /// </summary>
        Image FixedHeight(Image image, int height, Color backgroung);

        /// <summary>
        /// Resize the image to the desired width while maintaining the aspect ratio.
        /// </summary>
        Image FixedWidth(Image image, int width, Color backgroung);

        /// <summary>
        /// Get a part of image without change aspect ratio or resize.
        /// Anchor defines, which part of original image will be used
        /// </summary>
        Image Crop(Image image, int width, int height, AnchorPosition anchor);

    }
}
