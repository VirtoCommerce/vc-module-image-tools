using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Run set of thumbnail tasks by TaskIds.
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
                if (notifyEvent.Errors.Any())
                    notifyEvent.Description = $"Thumbnail generation process completed with errors. {notifyEvent.Errors.Count} issues need your attention.";
                else
                    notifyEvent.Description = $"Thumbnails generated successfully!";
                _pushNotifier.Send(notifyEvent);
            }
        }

        /// <summary>
        /// Run all thumbnails tasks.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(10)]
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

                    var progressInfo = new ThumbnailTaskProgress { Message = "Thumbnails generated successfully!" };
                    progressCallback(progressInfo);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                var errorMsg = "A thumbnail generation process is currently running. Please wait until the process is complete before attempting to start another one.";
                var progressInfo = new ThumbnailTaskProgress
                {
                    Message = errorMsg,
                    Errors = new List<string> { errorMsg }
                };
                progressCallback(progressInfo);
            }
        }
    }
}
