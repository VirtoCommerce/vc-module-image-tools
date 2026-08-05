using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Jobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

using Permission = VirtoCommerce.ImageToolsModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    [Route("api/image/thumbnails/tasks")]
    public class ThumbnailsTasksController : Controller
    {
        private readonly IThumbnailTaskSearchService _thumbnailTaskSearchService;
        private readonly IThumbnailTaskService _thumbnailTaskService;
        private readonly IPushNotificationManager _pushNotifier;
        private readonly IUserNameResolver _userNameResolver;

        public ThumbnailsTasksController(
            IThumbnailTaskSearchService thumbnailTaskSearchService,
            IThumbnailTaskService thumbnailTaskService,
            IPushNotificationManager pushNotifier,
            IUserNameResolver userNameResolver)
        {
            _thumbnailTaskSearchService = thumbnailTaskSearchService;
            _thumbnailTaskService = thumbnailTaskService;
            _pushNotifier = pushNotifier;
            _userNameResolver = userNameResolver;
        }

        /// <summary>
        /// Creates thumbnail task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [Authorize(Permission.Create)]
        public async Task<ActionResult<ThumbnailTask>> CreateThumbnailTask([FromBody] ThumbnailTask task)
        {
            await _thumbnailTaskService.SaveChangesAsync(new[] { task });
            return Ok(task);
        }

        /// <summary>
        /// Remove thumbnail tasks by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        [Authorize(Permission.Delete)]
        public async Task<ActionResult> DeleteThumbnailTask([FromQuery] string[] ids)
        {
            await _thumbnailTaskService.DeleteAsync(ids);
            return Ok();
        }

        /// <summary>
        /// Returns thumbnail task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Permission.Read)]
        public async Task<ActionResult<ThumbnailTask>> GetThumbnailTask([FromRoute] string id)
        {
            var task = await _thumbnailTaskService.GetNoCloneAsync(id);
            return Ok(task);
        }

        /// <summary>
        /// Searches thumbnail options by certain criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize(Permission.Read)]
        public async Task<ActionResult<ThumbnailTaskSearchResult>> SearchThumbnailTask([FromBody] ThumbnailTaskSearchCriteria criteria)
        {
            var result = await _thumbnailTaskSearchService.SearchNoCloneAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Updates thumbnail tasks
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [Authorize(Permission.Update)]
        public async Task<ActionResult> UpdateThumbnailTask([FromBody] ThumbnailTask tasks)
        {
            await _thumbnailTaskService.SaveChangesAsync(new[] { tasks });
            return Ok();
        }

        [HttpPost]
        [Route("{jobId}/cancel")]
        [Authorize(Permission.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult> Cancel([FromRoute] string jobId, CancellationToken cancellationToken)
        {
            // Cancellation is engine-dependent: Hangfire can recall a job by id, RabbitMQ cannot - a published message
            // is gone. Report that explicitly instead of answering 200 to a request that did nothing, so the admin UI
            // can disable the button rather than pretend the run was stopped.
            if (!BackgroundJob.SupportsCancellation)
            {
                return Problem(
                    statusCode: StatusCodes.Status501NotImplemented,
                    title: "Cancellation is not supported",
                    detail: "The active background job engine cannot cancel a running job. Wait for the process to finish.");
            }

            await BackgroundJob.Delete(jobId, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("run")]
        [Authorize(Permission.Read)]
        public async Task<ActionResult<ThumbnailProcessNotification>> Run([FromBody] ThumbnailsTaskRunRequest runRequest)
        {
            var notification = await Enqueue(runRequest);
            _pushNotifier.Send(notification);
            return Ok(notification);
        }

        private async Task<ThumbnailProcessNotification> Enqueue(ThumbnailsTaskRunRequest runRequest)
        {
            var notification = new ThumbnailProcessNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Process images",
                Description = "starting process...."
            };
            _pushNotifier.Send(notification);

            var payload = AbstractTypeFactory<ThumbnailProcessJobPayload>.TryCreateInstance();
            payload.RunRequest = runRequest;
            payload.Notification = notification;

            // MaxRetryAttempts = 0 carries over [AutomaticRetry(Attempts = 0)] from the Hangfire job: a failed
            // generation run is reported through the notification, not retried behind the user's back.
            var jobId = await BackgroundJob.Enqueue<ThumbnailProcessJobHandler>(payload, new EnqueueOptions { MaxRetryAttempts = 0 });

            // Set after the enqueue, as before: the payload copy the engine serialized carries no id, and the running
            // job fills it in from IJobExecutionContext.JobId. This assignment is for the HTTP response only.
            notification.JobId = jobId;

            return notification;
        }
    }
}
