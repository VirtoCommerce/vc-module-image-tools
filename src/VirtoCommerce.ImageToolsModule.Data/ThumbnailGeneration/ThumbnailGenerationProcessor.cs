using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        private readonly IThumbnailGenerator _generator;
        private readonly IThumbnailHandlerFactory _handlerFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly IImagesChangesProvider _imageChangesProvider;
        private readonly ILogger<ThumbnailGenerationProcessor> _logger;

        public ThumbnailGenerationProcessor(
            IThumbnailGenerator generator,
            IThumbnailHandlerFactory handlerFactory,
            ISettingsManager settingsManager,
            IImagesChangesProvider imageChangesProvider,
            ILogger<ThumbnailGenerationProcessor> logger)
        {
            _generator = generator;
            _handlerFactory = handlerFactory;
            _settingsManager = settingsManager;
            _imageChangesProvider = imageChangesProvider;
            _logger = logger;
        }

        public async Task ProcessTasksAsync(ICollection<ThumbnailTask> tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            try
            {
                var progressInfo = new ThumbnailTaskProgress { Message = "Getting changes count…" };

                progressCallback(progressInfo);

                var pageSize = await _settingsManager.GetValueAsync<int>(ModuleConstants.Settings.General.ProcessBatchSize);
                foreach (var task in tasks)
                {
                    progressInfo.Message = $"Processing task {task.Name}...";
                    progressCallback(progressInfo);

                    var changes = await _imageChangesProvider.GetNextChangesBatch(task, GetChangesSinceDate(task, regenerate), 0, int.MaxValue /*skip paging because no difference inside*/, token);

                    progressInfo.TotalCount = changes.Count;

                    if (!changes.Any())
                    {
                        break;
                    }

                    // ! Note: It was spend a lot of time considering replacement the next foreach to a Parallel.ForEach.
                    // Reasons it wasn't done:
                    // 1. High memory consumption and potential memory buggy leaks in ArrayPools (used inside of BlobClient and SixLabours) with multi-threading.
                    // 2. Network overload with reading heavy graphic files could cause non-reliable accessibility of other critical services (like Redis).
                    foreach (var fileChange in changes)
                    {
                        // Use format-specific handler if available
                        var handler = await _handlerFactory.GetHandlerAsync(fileChange.Url);
                        if (handler != null)
                        {
                            var result = await handler.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);
                            if (result?.Errors?.Count > 0)
                            {
                                progressInfo.Errors.AddRange(result.Errors);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No handler found for image: {Url}", fileChange.Url);
                        }

                        progressInfo.ProcessedCount++;

                        AfterPageProgress(progressCallback, progressInfo, pageSize);

                        token?.ThrowIfCancellationRequested();
                    }

                    ClearCache(task, regenerate);
                }
            }
            finally
            {
                ClearCache(tasks, regenerate);
            }
        }

        private void AfterPageProgress(Action<ThumbnailTaskProgress> progressCallback, ThumbnailTaskProgress progressInfo, int pageSize)
        {
            if (progressInfo.ProcessedCount % pageSize == 0 || progressInfo.ProcessedCount == progressInfo.TotalCount)
            {
                progressCallback(progressInfo);
                // Trace unmanaged resources, captured by SixLabours
                _logger.LogTrace(@"SixLabors...TotalUndisposedAllocationCount {count}", SixLabors.ImageSharp.Diagnostics.MemoryDiagnostics.TotalUndisposedAllocationCount);
            }
        }

        private void ClearCache(ICollection<ThumbnailTask> tasks, bool regenerate)
        {
            foreach (var task in tasks)
            {
                ClearCache(task, regenerate);
            }
        }

        private void ClearCache(ThumbnailTask task, bool regenerate)
        {
            BlobChangesCacheRegion.ExpireTaskRun(task, GetChangesSinceDate(task, regenerate));
        }

        private static DateTime? GetChangesSinceDate(ThumbnailTask task, bool regenerate)
        {
            return regenerate ? null : task.LastRun;
        }
    }
}
