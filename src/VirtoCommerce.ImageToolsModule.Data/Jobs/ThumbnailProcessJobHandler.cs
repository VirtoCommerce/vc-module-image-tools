using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.ImageToolsModule.Data.Jobs
{
    /// <summary>
    /// Runs a named set of thumbnail tasks, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="ThumbnailProcessJob"/>, which still owns the logic and stays callable by background
    /// jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class ThumbnailProcessJobHandler(ThumbnailProcessJob job) : IBackgroundJobHandler<ThumbnailProcessJobPayload>
    {
        public virtual Task Execute(ThumbnailProcessJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return job.Process(payload.RunRequest, payload.Notification, context, cancellationToken);
        }
    }
}
