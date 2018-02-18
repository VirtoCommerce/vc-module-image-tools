using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        protected readonly IThumbnailGenerator _generator;
        protected readonly IImagesChangesProvider _imageChangesProvider;

        protected readonly int _pageSize;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator,
            ISettingsManager settingsManager,
            IImagesChangesProvider imageChangesProvider)
        {
            _generator = generator;
            _imageChangesProvider = imageChangesProvider;

            _pageSize = settingsManager.GetValue("ImageTools.Thumbnails.ProcessBacthSize", 50);
        }

        public async Task ProcessTasksAsync(ThumbnailTask[] tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            var progressInfo = new ThumbnailTaskProgress { Message = "Reading the tasks..." };

            if (_imageChangesProvider.IsTotalCountSupported)
            {
                foreach (var task in tasks)
                {
                    progressInfo.TotalCount = _imageChangesProvider.GetTotalChangesCount(task.WorkPath, task.LastRun, regenerate);
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
                    var changes = _imageChangesProvider.GetNextChangesBatch(task.WorkPath, task.LastRun, regenerate, skip, _pageSize);
                    if (!changes.Any())
                        break;

                    foreach (var fileChange in changes)
                    {
                        var result = await _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);
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