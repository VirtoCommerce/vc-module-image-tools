using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.BackgroundJobs
{
    public class BackgroundJobsRunner
    {
        private readonly ISettingsManager _settingsManager;

        public BackgroundJobsRunner(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public async Task StartStopOrdersSynchronizationJob()
        {
            if (_settingsManager.GetValue(ModuleConstants.Settings.General.EnableImageProcessJob.Name, false))
            {
                var cronExpression = await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.ImageProcessJobCronExpression.Name, "0 0 * * *");
                RecurringJob.AddOrUpdate<ThumbnailProcessJob>("ProcessAllImageTasksJob", x => x.ProcessAll(JobCancellationToken.Null), cronExpression);
            }
            else
            {
                RecurringJob.RemoveIfExists("ProcessAllImageTasksJob");
            }
        }
    }
}
