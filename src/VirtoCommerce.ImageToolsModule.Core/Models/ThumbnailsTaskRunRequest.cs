namespace VirtoCommerce.ImageToolsModule.Core.Models
{
    /// <summary>
    /// ThumbnailsTaskRunRequest
    /// </summary>
    public class ThumbnailsTaskRunRequest
    {
        /// <summary>
        /// Ids of tasks
        /// </summary>
        public string[] TaskIds { get; set; }

        /// <summary>
        /// Regenerate
        /// </summary>
        public bool Regenerate { get; set; }
    }
}
