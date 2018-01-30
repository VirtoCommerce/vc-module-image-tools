using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        private IThumbnailGenerator _generator;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator)
        {
            _generator = generator;
        }

        public Task ProcessTasksAsync(ThumbnailTask[] tasks, Action<ThumbnailTaskProgress> progressCallback, CancellationToken token)
        {

        }
    }
}