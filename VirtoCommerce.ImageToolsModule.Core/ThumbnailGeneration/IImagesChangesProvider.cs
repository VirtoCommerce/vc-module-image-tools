using System;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IImagesChangesProvider
    {
        bool IsTotalCountSupported { get; }

        long GetTotalChangesCount(string workPath, DateTime? lastRunDate, bool regenerate);

        ImageChange[] GetNextChangesBatch(string workPath, DateTime? lastRunDate, bool regenerate, long? skip, long? take);
    }
}