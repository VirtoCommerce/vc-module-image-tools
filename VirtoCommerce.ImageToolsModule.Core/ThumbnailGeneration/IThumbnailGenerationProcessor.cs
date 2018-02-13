using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailGenerationProcessor
    {
        void ProcessTasksAsync(ThumbnailTask[] tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token);
    }
}