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
        private readonly ISettingsManager _settingsManager;
        private readonly IImagesChangesProvider _imageChangesProvider;
        private readonly ILogger<ThumbnailGenerationProcessor> _logger;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator,
            ISettingsManager settingsManager,
            IImagesChangesProvider imageChangesProvider,
            ILogger<ThumbnailGenerationProcessor> logger)
        {
            _generator = generator;
            _settingsManager = settingsManager;
            _imageChangesProvider = imageChangesProvider;
            _logger = logger;
        }

        public async Task ProcessTasksAsync(ICollection<ThumbnailTask> tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            try
            {
                var progressInfo = new ThumbnailTaskProgress { Message = "Getting changes count…" };

                /*
                if (_imageChangesProvider.IsTotalCountSupported)
                {
                    foreach (var task in tasks)
                    {
                        var changesSince = GetChangesSinceDate(task, regenerate);
                        progressInfo.TotalCount += await _imageChangesProvider.GetTotalChangesCount(task, changesSince, token);
                    }
                }
                */

                progressCallback(progressInfo);

                var pageSize = _settingsManager.GetValue(ModuleConstants.Settings.General.ProcessBatchSize.Name, 50);
                foreach (var task in tasks)
                {                    
                    progressInfo.Message = $"Processing task {task.Name}...";
                    progressCallback(progressInfo);

                    var changes = await _imageChangesProvider.GetNextChangesBatch(task, GetChangesSinceDate(task, regenerate), 0, int.MaxValue /*skip paging because no difference inside*/, token);

                    progressInfo.TotalCount = changes.Count();

                    if (!changes.Any())
                        break;

                    _ = Parallel.ForEach(changes, fileChange =>
                    {
                        var result = _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token).GetAwaiter().GetResult();

                        lock (progressInfo)
                        {
                            progressInfo.ProcessedCount++;

                            if (result != null && !result.Errors.IsNullOrEmpty())
                            {
                                progressInfo.Errors.AddRange(result.Errors);
                            }

                            if (progressInfo.ProcessedCount % pageSize == 0 || progressInfo.ProcessedCount == progressInfo.TotalCount)
                            {
                                progressCallback(progressInfo);
                                // Trace unmanaged resources, captured by SixLabours
                                _logger.LogTrace(@"SixLabors...TotalUndisposedAllocationCount {count}", SixLabors.ImageSharp.Diagnostics.MemoryDiagnostics.TotalUndisposedAllocationCount);

                                // Trigger a few Gen2 GCs to make sure the ArrayPools (used in BlobClient and SixLabours Libs) has appropriately time stamped buffers.
                                // Then force a GC to get some buffers returned
                                // Otherwise ArrayPools will consume too much memory and drain it.
                                // Look here problems with ArrayPools: https://github.com/dotnet/runtime/issues/52098, https://github.com/dotnet/runtime/pull/56316.
                                // It's not a wonderful solution to call GC directly, but have no idea what else we can do.
                                for (var i = 0; i < 3; i++)
                                {
#pragma warning disable S1215 // "GC.Collect" should not be called
                                    GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called
                                    GC.WaitForPendingFinalizers();
                                }
                            }

                            token?.ThrowIfCancellationRequested();
                        }
                    });

                    ClearCache(task, regenerate);
                }
            }
            finally
            {
                ClearCache(tasks, regenerate);
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
