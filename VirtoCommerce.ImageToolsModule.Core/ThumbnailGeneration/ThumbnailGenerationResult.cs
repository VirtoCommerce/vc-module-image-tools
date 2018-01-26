namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    using System.Collections.Generic;

    public class ThumbnailGenerationResult
    {
        public ThumbnailGenerationResult()
        {
            GeneratedThumbnails = new List<string>();
            Errors = new List<string>();
        }

        public IList<string> GeneratedThumbnails { get; }
        public IList<string> Errors { get; }
    }
}