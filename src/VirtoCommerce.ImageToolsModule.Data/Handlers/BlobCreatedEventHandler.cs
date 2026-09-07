using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.AssetsModule.Core.Events;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Jobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Handlers
{
    public class BlobCreatedEventHandler : IEventHandler<BlobCreatedEvent>
    {
        private readonly IThumbnailTaskSearchService _thumbnailTaskSearchService;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;
        private readonly ISettingsManager _settingsManager;

        public BlobCreatedEventHandler(
            IThumbnailTaskSearchService thumbnailTaskSearchService,
            IThumbnailOptionSearchService thumbnailOptionSearchService,
            ISettingsManager settingsManager)
        {
            _thumbnailTaskSearchService = thumbnailTaskSearchService;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
            _settingsManager = settingsManager;
        }

        public async Task Handle(BlobCreatedEvent message)
        {
            if (!await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.EventBasedThumbnailGeneration))
            {
                return;
            }

            // exclude thumbnails from processing
            var assetUrls = message.ChangedEntries.Select(x => x.NewEntry?.Uri).Where(x => x != null).ToList();
            var options = await _thumbnailOptionSearchService.SearchAllNoCloneAsync(new ThumbnailOptionSearchCriteria());
            var suffixCollection = options.Select(x => x.FileSuffix).Distinct().ToList();
            assetUrls = GetOriginalItems(assetUrls, suffixCollection);

            if (assetUrls.Count == 0)
            {
                return;
            }

            // figure out tasks to run by asset urls
            var tasks = await _thumbnailTaskSearchService.SearchAllAsync(new ThumbnailTaskSearchCriteria());
            var tasksToRun = new List<ThumbnailTask>();
            foreach (var task in tasks)
            {
                var workPath = task.WorkPath.StartsWith('/') ? task.WorkPath[1..] : task.WorkPath;

                if (assetUrls.Any(x => x.StartsWith(workPath)))
                {
                    tasksToRun.Add(task);
                }
            }

            if (tasksToRun.Count == 0)
            {
                return;
            }

            var payload = AbstractTypeFactory<ThumbnailProcessJobPayload>.TryCreateInstance();
            payload.RunRequest = new ThumbnailsTaskRunRequest
            {
                TaskIds = tasksToRun.Select(x => x.Id).ToArray(),
            };
            // No notification: this run was triggered by an uploaded blob, not by a user watching a progress bar.
            // ThumbnailProcessJob.Process creates a throwaway one, exactly as it did when Hangfire passed null here.
            payload.Notification = null;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            // MaxRetryAttempts = 0 carries over [AutomaticRetry(Attempts = 0)] from the Hangfire job.
            await BackgroundJob.Enqueue<ThumbnailProcessJobHandler>(payload, new EnqueueOptions { MaxRetryAttempts = 0 });
        }

        protected virtual List<string> GetOriginalItems(List<string> assetUrls, List<string> suffixCollection)
        {
            var result = new List<string>();

            foreach (var assetUrl in assetUrls)
            {
                if (!suffixCollection.Any(suffix => assetUrl.Contains("_" + suffix)))
                {
                    result.Add(assetUrl);
                }
            }

            return result;
        }
    }
}
