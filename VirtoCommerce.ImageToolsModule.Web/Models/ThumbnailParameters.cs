using System.Drawing;

namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    /// <summary>
    /// Specification to generate a thumbnail.
    /// </summary>
    public class ThumbnailParameters
    {
    /// <summary>
    /// Thumbnail width.
    /// </summary>
    public int Width { get; set; }
        /// <summary>
        /// Thumbnail height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Background layer color of image.
        /// If the original image has an aspect ratio different from thumbnail, 
        /// not covered part the thumbnail will be filled with that color.
        /// </summary>
        public string Background { get; set; }
        /// <summary>
        /// Background layer color of image.
        /// If the original image has an aspect ratio different from thumbnail, 
        /// not covered part the thumbnail will be filled with that color.
        /// </summary>
        public Color Color
        {
            get
            {
                return !string.IsNullOrEmpty(Background) ? ColorTranslator.FromHtml(Background) : Color.White;
            }
        }

        /// <summary>
        /// Thumbnail alias (using to generate a thumbnail url as a suffix)  
        /// </summary>
        public string Alias { get; set; }

    }
}