using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.ImageToolsModule.Data.BackgroundJobs
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
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        // Why attributes above are set. 
        // "DisableConcurrentExecutionAttribute" should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // We can change this default behavior using "AutomaticRetryAttribute". This allows to manage retries and reject jobs in case of retries exhaust.
        // In our case, the job, awaiting for previous the same job more than 10 seconds, will fall into "deleted" state with no retries.
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

                var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);
                await _thumbnailProcessor.ProcessTasksAsync(tasks, generateRequest.Regenerate, progressCallback, cancellationTokenWrapper);

                //update tasks in case of successful generation
                foreach (var task in tasks)
                {
                    task.LastRun = DateTime.UtcNow;
                }

                await _taskService.SaveChangesAsync(tasks);
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
        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        // Why attributes above are set. 
        // "DisableConcurrentExecutionAttribute" should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // We can change this default behavior using "AutomaticRetryAttribute". This allows to manage retries and reject jobs in case of retries exhaust.
        // In our case, the job, awaiting for previous the same job more than 10 seconds, will fall into "deleted" state with no retries.
        public async Task ProcessAll(IJobCancellationToken cancellationToken)
        {
            var thumbnailTasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria() { Take = 0, Skip = 0 });
            var tasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria() { Take = thumbnailTasks.TotalCount, Skip = 0 });

            Action<ThumbnailTaskProgress> progressCallback = x => { };
            var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);
            await _thumbnailProcessor.ProcessTasksAsync(tasks.Results, false, progressCallback, cancellationTokenWrapper);

            //update tasks in case of successful generation
            foreach (var task in tasks.Results)
            {
                task.LastRun = DateTime.UtcNow;
            }

            await _taskService.SaveChangesAsync(tasks.Results);
        }
    }
}
