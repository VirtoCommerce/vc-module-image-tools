using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings.Events;

namespace VirtoCommerce.ImageToolsModule.Data.Handlers
{
    public class ObjectSettingEntryChangedEventHandler : IEventHandler<ObjectSettingChangedEvent>
    {
        private readonly BackgroundJobsRunner _backgroundJobsRunner;

        public ObjectSettingEntryChangedEventHandler(BackgroundJobsRunner backgroundJobsRunner)
        {
            _backgroundJobsRunner = backgroundJobsRunner;
        }

        public virtual async Task Handle(ObjectSettingChangedEvent message)
        {
            if (message.ChangedEntries.Any(x => (x.EntryState == EntryState.Modified
                                              || x.EntryState == EntryState.Added)
                                  && (x.NewEntry.Name == ModuleConstants.Settings.General.EnableImageProcessJob.Name
                                   || x.NewEntry.Name == ModuleConstants.Settings.General.ImageProcessJobCronExpression.Name)))
            {
                await _backgroundJobsRunner.StartStopOrdersSynchronizationJob();
            }
        }
    }
}
