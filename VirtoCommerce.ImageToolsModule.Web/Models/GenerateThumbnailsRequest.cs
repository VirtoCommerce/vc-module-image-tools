
namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    /// <summary>
    /// Request to generate Thumbnails
    /// </summary>
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

    }
}