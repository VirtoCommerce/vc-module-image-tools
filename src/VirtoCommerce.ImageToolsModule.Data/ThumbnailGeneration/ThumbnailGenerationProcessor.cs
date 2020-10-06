using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator,
            ISettingsManager settingsManager,
            IImagesChangesProvider imageChangesProvider)
        {
            _generator = generator;
            _settingsManager = settingsManager;
            _imageChangesProvider = imageChangesProvider;
        }

        public async Task ProcessTasksAsync(ICollection<ThumbnailTask> tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            try
            {
                var progressInfo = new ThumbnailTaskProgress { Message = "Getting changes count…" };

                if (_imageChangesProvider.IsTotalCountSupported)
                {
                    foreach (var task in tasks)
                    {
                        var changesSince = GetChangesSinceDate(task, regenerate);
                        progressInfo.TotalCount += await _imageChangesProvider.GetTotalChangesCount(task, changesSince, token);
                    }
                }

                progressCallback(progressInfo);

                var pageSize = _settingsManager.GetValue(ModuleConstants.Settings.General.ProcessBatchSize.Name, 50);
                foreach (var task in tasks)
                {
                    progressInfo.Message = $"Processing task {task.Name}...";
                    progressCallback(progressInfo);

                    var skip = 0;
                    while (true)
                    {
                        var changes = await _imageChangesProvider.GetNextChangesBatch(task, GetChangesSinceDate(task, regenerate), skip, pageSize, token);
                        if (!changes.Any())
                            break;

                        foreach (var fileChange in changes)
                        {
                            var result = await _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);
                            progressInfo.ProcessedCount++;

                            if (result != null && !result.Errors.IsNullOrEmpty())
                            {
                                progressInfo.Errors.AddRange(result.Errors);
                            }
                        }

                        skip += changes.Length;

                        progressCallback(progressInfo);
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
