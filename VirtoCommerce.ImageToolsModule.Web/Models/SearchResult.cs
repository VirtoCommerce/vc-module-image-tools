namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    /// <summary>
    /// 
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ThumbnailOption[] ThumbnailOptions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalCount { get; set; }
    }
}