using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        private readonly IThumbnailGenerator _generator;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator)
        {
            _generator = generator;
        }

        public async Task ProcessTasksAsync(ThumbnailTask[] tasks, Action<ThumbnailTaskProgress> progressCallback, CancellationToken token)
        {
            //find initial files count
            var progressInfo = new ThumbnailTaskProgress { Message = "Reading the options..." };
            progressCallback(progressInfo);

            foreach (var task in tasks)
            {
                foreach (var option in task.ThumbnailOptions)
                {
                    var result = await _generator.GenerateThumbnailsAsync(task.WorkPath, task.WorkPath, option, token);

                    progressCallback(progressInfo);
                }
            }
        }
    }
}