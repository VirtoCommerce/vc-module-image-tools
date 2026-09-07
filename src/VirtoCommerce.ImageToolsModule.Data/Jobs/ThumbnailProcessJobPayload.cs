using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;

namespace VirtoCommerce.ImageToolsModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that runs a named set of thumbnail tasks.
    /// </summary>
    public class ThumbnailProcessJobPayload
    {
        public ThumbnailsTaskRunRequest RunRequest { get; set; }

        /// <summary>
        /// The notification created by the request that started the job, or null when the job was started by an event
        /// rather than by a user. Carried in the payload, as the Hangfire job this replaces did, because the job
        /// reports its progress against that same notification id.
        /// </summary>
        public ThumbnailProcessNotification Notification { get; set; }
    }
}
