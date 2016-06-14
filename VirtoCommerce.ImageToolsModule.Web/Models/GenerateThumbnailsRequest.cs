
namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    public class GenerateThumbnailsRequest
    {
        /// <summary>
        /// Original image url.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// True to replace all existed thumbnails with a new ones.
        /// </summary>
        public bool IsRegenerateAll { get; set; }

        /// <summary>
        /// ImageTools.Thumbnails.Parameters setting values.
        /// </summary>
        public string[] ThumbnailsParameters { get; set; }
    }
}