namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    /// <summary>
    /// SearchResult
    /// </summary>
    public class SearchResult<TItem>
    {
        /// <summary>
        /// Result
        /// </summary>
        public TItem[] Result { get; set; }

        /// <summary>
        /// TotalCount
        /// </summary>
        public int TotalCount { get; set; }
    }
}