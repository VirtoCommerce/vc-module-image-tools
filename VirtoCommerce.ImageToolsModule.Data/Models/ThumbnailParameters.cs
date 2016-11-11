using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    /// <summary>
    /// Specification to generate a thumbnail.
    /// </summary>
    public class ThumbnailParameters
    {
        /// <summary>
        /// Method of thumbnails generation
        /// 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ResizeType Method { get; set; }

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
        public Color Color { get; set; }

        /// <summary>
        /// Thumbnail alias (using to generate a thumbnail url as a suffix)  
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Anchor position to cropping
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AnchorPosition AnchorPosition { get; set; }
    }
}
