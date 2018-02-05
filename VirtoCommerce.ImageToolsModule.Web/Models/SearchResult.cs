namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    /// <summary>
    /// 
    /// </summary>
    public class SearchResult<TItem>
    {
        /// <summary>
        /// 
        /// </summary>
        public TItem[] Result { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalCount { get; set; }
    }
}