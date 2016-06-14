using System.Drawing;

namespace VirtoCommerce.ImageToolsModule.Web.Services
{
    /// <summary>
    /// Image resize library interface
    /// </summary>
    public interface IImageResize
    {
        /// <summary>
        /// Resize image.
        /// Original image will be resized proportionally to fit given Width, Height .
        /// Original image will not be cropped.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right). 
        /// The empty spaces will be filled with given color.
        /// </summary>
        Image FixedSize(Image image, int Width, int Height, Color color);
    }
}