using System;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IImagesChangesProvider
    {
        bool GetTotalCountSupported { get; }

        long GetTotalChangesCount(string workPath, bool regenerate, DateTime? lastRunDate);

        ImageChange[] GetNextChangesBatch(string workPath, bool regenerate, DateTime? lastRunDate, long? skip, long? take);
    }
}