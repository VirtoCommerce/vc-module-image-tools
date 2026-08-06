using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.ImageToolsModule.Data.BackgroundJobs
{
    public class ThumbnailProcessJob
    {
        /// <summary>
        /// Resource key of the distributed lock that keeps two generation runs from overlapping. Same name the
        /// Hangfire implementation used, so a mixed-version cluster mid-upgrade still contends on one key.
        /// </summary>
        public const string GenerationLockKey = "ThumbnailProcessJob";

        /// <summary>
        /// How long the generation lock is held. Unlike Hangfire's connection-scoped lock, which lived exactly as long
        /// as the enclosing using block, this one is a TTL: if a run outlasts it, another instance may start a parallel
        /// run. Set generously - a full regeneration over a large asset store is measured in hours - and revisit if a
        /// deployment ever legitimately exceeds it.
        /// </summary>
        private static readonly TimeSpan _generationLockTimeout = TimeSpan.FromHours(8);

        private readonly IPushNotificationManager _pushNotifier;
        private readonly IThumbnailGenerationProcessor _thumbnailProcessor;
        private readonly IThumbnailTaskService _taskService;
        private readonly IThumbnailTaskSearchService _taskSearchService;
        private readonly IDistributedLockService _distributedLockService;

        public ThumbnailProcessJob(
            IPushNotificationManager pushNotifier,
            IThumbnailGenerationProcessor thumbnailProcessor,
            IThumbnailTaskService taskService,
            IThumbnailTaskSearchService taskSearchService,
            IDistributedLockService distributedLockService)
        {
            _pushNotifier = pushNotifier;
            _thumbnailProcessor = thumbnailProcessor;
            _taskService = taskService;
            _taskSearchService = taskSearchService;
            _distributedLockService = distributedLockService;
        }

        /// <summary>
        /// Run set of thumbnail tasks by TaskIds.
        /// </summary>
        /// <param name="generateRequest">Which tasks to run, and whether to regenerate existing thumbnails.</param>
        /// <param name="notifyEvent">Notification to report progress against; a fresh one is created when null.</param>
        /// <param name="context">Job execution context supplied by the engine. Null when called outside a job.</param>
        /// <param name="cancellationToken">Cancelled on shutdown and on job cancellation.</param>
        /// <remarks>
        /// The retry policy that used to sit here as [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = Delete)] is now
        /// set per enqueue, through EnqueueOptions.MaxRetryAttempts = 0 - see the callers.
        /// </remarks>
        public async Task Process(
            ThumbnailsTaskRunRequest generateRequest,
            ThumbnailProcessNotification notifyEvent,
            IJobExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            notifyEvent ??= new ThumbnailProcessNotification(Guid.NewGuid().ToString());

            var canceled = false;
            try
            {
                Action<ThumbnailTaskProgress> progressCallback = x =>
                {
                    notifyEvent.Description = x.Message;
                    notifyEvent.Errors = x.Errors;
                    notifyEvent.ErrorCount = notifyEvent.Errors.Count;
                    notifyEvent.TotalCount = x.TotalCount ?? 0;
                    notifyEvent.ProcessedCount = x.ProcessedCount ?? 0;
                    // Engine-assigned id, so the admin UI can address this run - previously PerformContext.BackgroundJob.Id.
                    notifyEvent.JobId = context?.JobId ?? notifyEvent.JobId;

                    _pushNotifier.Send(notifyEvent);
                };

                var tasks = await _taskService.GetAsync(generateRequest.TaskIds);

                await PerformGeneration(tasks, generateRequest.Regenerate, progressCallback, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Stopped by shutdown or by an explicit cancellation - the JobAbortedException equivalent. Flag it so
                // the finally reports "canceled" rather than mislabeling a partial run as success or failure.
                canceled = true;
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

                notifyEvent.Description = canceled
                    ? $"Thumbnail generation was canceled. Processed {notifyEvent.ProcessedCount} of {notifyEvent.TotalCount} images."
                    : notifyEvent.Errors.Count != 0
                        ? $"Thumbnail generation process completed with errors. {notifyEvent.Errors.Count} issues need your attention."
                        : "Thumbnails generated successfully!";

                await _pushNotifier.SendAsync(notifyEvent);
            }
        }

        /// <summary>
        /// Run all thumbnails tasks.
        /// </summary>
        /// <remarks>
        /// The [DisableConcurrentExecution(10)] that used to guard this method is gone with the Hangfire reference, and
        /// is not needed: <see cref="PerformGeneration"/> takes a distributed lock covering this path and the on-demand
        /// one alike, and it does so across the whole worker fleet rather than one Hangfire server.
        /// </remarks>
        public async Task ProcessAll(CancellationToken cancellationToken = default)
        {
            var thumbnailTasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria { Take = 0, Skip = 0 });
            var tasks = await _taskSearchService.SearchAsync(new ThumbnailTaskSearchCriteria { Take = thumbnailTasks.TotalCount, Skip = 0 });

            Action<ThumbnailTaskProgress> progressCallback = _ => { };

            await PerformGeneration(tasks.Results, false, progressCallback, cancellationToken);
        }

        private async Task PerformGeneration(IEnumerable<ThumbnailTask> tasks, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, CancellationToken cancellationToken)
        {
            try
            {
                // tryLockTimeout stays null on purpose: fail immediately when another run holds the lock, matching the
                // TimeSpan.Zero that Hangfire's AcquireDistributedLock was called with. Waiting would queue duplicates.
                await _distributedLockService.ExecuteAsync(
                    GenerationLockKey,
                    async () =>
                    {
                        foreach (var task in tasks)
                        {
                            // Better to run and save tasks one by one to save LastRun date once every task is completed, opposing to waiting all tasks completion, as it could be a long process.
                            var oneTaskArray = new[] { task };
                            //Need to save runTime at start in order to not loose changes that may be done between the moment of getting changes and the task completion.
                            var runTime = DateTime.UtcNow;

                            await _thumbnailProcessor.ProcessTasksAsync(oneTaskArray, regenerate, progressCallback, cancellationToken);

                            task.LastRun = runTime;

                            await _taskService.SaveChangesAsync(oneTaskArray);
                        }

                        var progressInfo = new ThumbnailTaskProgress { Message = "Thumbnails generated successfully!" };
                        progressCallback(progressInfo);

                        return true;
                    },
                    lockTimeout: _generationLockTimeout,
                    cancellationToken: cancellationToken);
            }
            catch (PlatformException)
            {
                // Another run holds the lock. IDistributedLockService signals that as PlatformException, where Hangfire
                // raised DistributedLockTimeoutException; the message shown to the user is unchanged.
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
