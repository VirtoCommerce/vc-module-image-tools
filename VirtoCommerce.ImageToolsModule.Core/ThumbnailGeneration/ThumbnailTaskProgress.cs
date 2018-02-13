using System.Collections.Generic;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public class ThumbnailTaskProgress
    {
        public ThumbnailTaskProgress()
        {
            Errors = new List<string>();
            TotalCount = ProcessedCount = default(long);
        }

        public string Message { get; set; }

        public long? TotalCount { get; set; }

        public long? ProcessedCount { get; set; }
        /// <summary>
        /// List of errors
        /// </summary>
        public IList<string> Errors { get; set; }
    }
}