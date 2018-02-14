using System;
using System.Linq;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.ImageToolsModule.Web.Models.PushNotifications;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.ImageToolsModule.Web.BackgroundJobs
{
    public class ThumbnailProcessJob
    {
        private readonly IThumbnailGenerationProcessor _thumbnailProcessor;
        private readonly IPushNotificationManager _pushNotifier;

        public ThumbnailProcessJob(IPushNotificationManager pushNotifier, IThumbnailGenerationProcessor thumbnailProcessor)
        {
            _pushNotifier = pushNotifier;
            _thumbnailProcessor = thumbnailProcessor;
        }

        /// <summary>
        /// Thumbnail generation process
        /// </summary>
        /// <param name="generateRequest"></param>
        /// <param name="notifyEvent"></param>
        /// <param name="cancellationToken">Hangfire sets the cancellation token</param>
        /// <param name="context">Hangfire sets the process context</param>
        [DisableConcurrentExecution(60 * 60 * 24)]
        public void Process(ThumbnailsTaskRunRequest generateRequest, ThumbnailProcessNotification notifyEvent, IJobCancellationToken cancellationToken, PerformContext context)
        {
            try
            {
                Action<ThumbnailTaskProgress> progressCallback = x =>
                {
                    notifyEvent.Description = x.Message;
                    notifyEvent.Errors = x.Errors;
                    notifyEvent.ErrorCount = notifyEvent.Errors.Count;
                    notifyEvent.TotalCount = x.TotalCount ?? 0;
                    notifyEvent.ProcessedCount = x.ProcessedCount ?? 0;
                    notifyEvent.JobId = context.BackgroundJob.Id;

                    _pushNotifier.Upsert(notifyEvent);
                };

                //wrap token 
                var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);
                _thumbnailProcessor.ProcessTasksAsync(generateRequest.TaskIds, generateRequest.Regenerate, progressCallback, cancellationTokenWrapper);
            }
            catch (JobAbortedException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                notifyEvent.Description = "Error";
                notifyEvent.ErrorCount++;
                notifyEvent.Errors.Add(ex.ToString());
            }
            finally
            {
                notifyEvent.Finished = DateTime.UtcNow;
                notifyEvent.Description = "Process finished" + (notifyEvent.Errors.Any() ? " with errors" : " successfully");
                _pushNotifier.Upsert(notifyEvent);
            }
        }
    }
}