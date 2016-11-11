namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    /// <summary>
    /// Request to generate Thumbnails for image in a platform blob storage.
    /// </summary>
    public class GenerateThumbnailsRequest
    {
        /// <summary>
        /// Url of a platform blob storage image.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// True to replace all existed thumbnails with a new ones. False to generate missed only.
        /// </summary>
        public bool IsRegenerateAll { get; set; }

    }
}
