using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.ImageToolsModule.Data.Jobs
{
    /// <summary>
    /// Runs every configured thumbnail task. Target of the recurring schedule.
    /// </summary>
    public class ThumbnailProcessAllJobHandler(ThumbnailProcessJob job) : IBackgroundJobHandler<ThumbnailProcessAllJobPayload>
    {
        public virtual Task Execute(ThumbnailProcessAllJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return job.ProcessAll(cancellationToken);
        }
    }
}
