using System;
using System.Collections.Generic;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        private readonly IThumbnailGenerator _generator;
        private readonly IThumbnailTaskService _thumbnailTaskService;
        private readonly Func<ThumbnailTask, bool, IImagesChangesProvider> _factory;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator,
            IThumbnailTaskService thumbnailTaskService,
            Func<ThumbnailTask, bool, IImagesChangesProvider> factory)
        {
            _generator = generator;
            _thumbnailTaskService = thumbnailTaskService;
            _factory = factory;
        }

        public void ProcessTasksAsync(string[] taskIds, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            var progressInfo = new ThumbnailTaskProgress { Message = "Reading the tasks..." };

            var tasks = _thumbnailTaskService.GetByIds(taskIds);

            var lookup = new Dictionary<ThumbnailTask, IImagesChangesProvider>();
            foreach (var task in tasks)
            {
                var changesProvider = _factory(task, regenerate);
                progressInfo.TotalCount += changesProvider.GetTotalChangesCount();
                lookup.Add(task, changesProvider);
            }

            progressCallback(progressInfo);

            foreach (var task in lookup.Keys)
            {
                progressInfo.Message = $"Procesing task {task.Name}...";
                progressCallback(progressInfo);

                var changesProvide = lookup[task];
                while (true)
                {
                    var changes = changesProvide.GetNextChangesBatch();
                    if (changes == null)
                        break;

                    foreach (var fileInfo in changes.ImageChanges)
                    {
                        var result = _generator.GenerateThumbnailsAsync(fileInfo.Url, task.WorkPath, fileInfo.ThumbnailOptions, token);
                        progressInfo.ProcessedCount++;

                        if (!result.Errors.IsNullOrEmpty())
                        {
                            progressInfo.Errors.AddRange(result.Errors);
                        }
                    }

                    progressCallback(progressInfo);

                    token?.ThrowIfCancellationRequested();
                }
            }

            progressInfo.Message = "Finished generating thumbnails!";
            progressCallback(progressInfo);
        }
    }

}