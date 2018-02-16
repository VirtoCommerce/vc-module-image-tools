using System;
using System.Collections.Generic;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public class ImageChange
    {
        public ImageChange()
        {
            ThumbnailOptions = new List<ThumbnailOption>();
        }

        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public IList<ThumbnailOption> ThumbnailOptions { get; set; }

    }

    public class ImageChangeResult
    {
        public ThumbnailTask ThumbnailTask { get; set; }

        public int TotalCount { get; set; }

        public IList<ImageChange> ImageChanges { get; set; }
    }
}
