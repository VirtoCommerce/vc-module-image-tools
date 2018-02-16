using System;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        public int Take { get; set; }

        private readonly IThumbnailGenerator _generator;
        private readonly IThumbnailTaskService _thumbnailTaskService;
        private readonly IImagesChangesProvider _imageChangesProvider;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator,
            IThumbnailTaskService thumbnailTaskService,
            ISettingsManager settingsManager,
            IImagesChangesProvider imageChangesProvider)
        {
            _generator = generator;
            _thumbnailTaskService = thumbnailTaskService;
            _imageChangesProvider = imageChangesProvider;

            Take = settingsManager.GetValue("ImageTools.Thumbnails.ProcessBacthSize", 50);
        }


        public void ProcessTasksAsync(string[] taskIds, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            var progressInfo = new ThumbnailTaskProgress { Message = "Reading the tasks..." };

            var tasks = _thumbnailTaskService.GetByIds(taskIds);

            if (_imageChangesProvider.GetTotalCountSupported)
            {
                foreach (var task in tasks)
                {
                    progressInfo.TotalCount = _imageChangesProvider.GetTotalChangesCount(task.WorkPath, regenerate, task.LastRun);
                }
            }
           
            progressCallback(progressInfo);
            foreach (var task in tasks)
            {
                progressInfo.Message = $"Procesing task {task.Name}...";
                progressCallback(progressInfo);

                var skip = 0;
                while (true)
                {
                    var changes = _imageChangesProvider.GetNextChangesBatch(task.WorkPath, regenerate, task.LastRun, skip, Take);
                    if (!changes.Any())
                        break;

                    foreach (var fileChange in changes)
                    {
                        var result = _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);
                        progressInfo.ProcessedCount++;

                        if (!result.Errors.IsNullOrEmpty())
                        {
                            progressInfo.Errors.AddRange(result.Errors);
                        }
                    }

                    skip += changes.Length;

                    progressCallback(progressInfo);
                    token?.ThrowIfCancellationRequested();
                }
            }

            progressInfo.Message = "Finished generating thumbnails!";
            progressCallback(progressInfo);
        }
    }

}