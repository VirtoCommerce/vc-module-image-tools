using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.Model;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.ImageToolsModule.Web.BackgroundJobs
{
    public class ThumbnailProcessJob
    {
        private readonly IThumbnailGenerationProcessor _thumbnailProcessor;
        private readonly IPushNotificationManager _pushNotifier;
        private readonly IThumbnailTaskService _taskService;
        private readonly IThumbnailTaskSearchService _taskSearchService;

        public ThumbnailProcessJob(IPushNotificationManager pushNotifier, IThumbnailGenerationProcessor thumbnailProcessor, IThumbnailTaskService taskService, IThumbnailTaskSearchService taskSearchService)
        {
            _pushNotifier = pushNotifier;
            _thumbnailProcessor = thumbnailProcessor;
            _taskService = taskService;
            _taskSearchService = taskSearchService;
        }

        /// <summary>
        /// Thumbnail generation process
        /// </summary>
        /// <param name="generateRequest"></param>
        /// <param name="notifyEvent"></param>
        /// <param name="cancellationToken">Hangfire sets the cancellation token</param>
        /// <param name="context">Hangfire sets the process context</param>
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task Process(ThumbnailsTaskRunRequest generateRequest, ThumbnailProcessNotification notifyEvent, IJobCancellationToken cancellationToken, PerformContext context)
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

                    _pushNotifier.Send(notifyEvent);
                };

                //wrap token 
                var tasks = await _taskService.GetByIdsAsync(generateRequest.TaskIds);

                await PerformGeneration(tasks, generateRequest.Regenerate, progressCallback, cancellationToken);
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
                _pushNotifier.Send(notifyEvent);
            }
        }

        /// <summary>
        /// Find all tasks and run them
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisableConcurrentExecution(10)]
        public async Task ProcessAll(IJobCancellationToken cancellationToken)
        {
            var thumbnailTasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria() { Take = 0, Skip = 0 });
            var tasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria() { Take = thumbnailTasks.TotalCount, Skip = 0 });

            Action<ThumbnailTaskProgress> progressCallback = x => { };

            await PerformGeneration(tasks.Results, false, progressCallback, cancellationToken);
        }

        private async Task PerformGeneration(IEnumerable<ThumbnailTask> tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, IJobCancellationToken cancellationToken)
        {
            try
            {
                using (JobStorage.Current.GetConnection().AcquireDistributedLock("ThumbnailProcessJob", TimeSpan.Zero))
                {
                    var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);

                    foreach (var task in tasks)
                    {
                        // Better to run and save tasks one by one to save LastRun date once every task is completed, opposing to waiting all tasks completion, as it could be a long process.
                        var oneTaskArray = new[] { task };
                        //Need to save runTime at start in order to not loose changes that may be done between the moment of getting changes and the task completion.
                        var runTime = DateTime.UtcNow;

                        await _thumbnailProcessor.ProcessTasksAsync(oneTaskArray, regenerate, progressCallback, cancellationTokenWrapper);

                        task.LastRun = runTime;

                        await _taskService.SaveChangesAsync(oneTaskArray);
                    }

                    var progressInfo = new ThumbnailTaskProgress { Message = "Finished generating thumbnails!" };
                    progressCallback(progressInfo);

                }
            }
            catch (DistributedLockTimeoutException)
            {
                var progressInfo = new ThumbnailTaskProgress
                {
                    Message = "Indexation is already in progress.",
                    Errors = new List<string> { "Indexation is already in progress." }
                };
                progressCallback(progressInfo);
            }
        }
    }
}
