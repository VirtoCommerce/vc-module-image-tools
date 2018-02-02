using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailGenerationProcessor
    {
        Task ProcessTasksAsync(ThumbnailTask[] tasks, Action<ThumbnailTaskProgress> progressCallback,
            CancellationToken token);
    }
}