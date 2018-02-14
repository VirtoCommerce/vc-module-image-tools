using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailGenerationProcessor
    {
        void ProcessTasksAsync(string[] taskIds, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token);
    }
}