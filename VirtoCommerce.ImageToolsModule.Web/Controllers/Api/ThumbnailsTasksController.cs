using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using Omu.ValueInjecter;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.BackgroundJobs;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.ImageToolsModule.Web.Models.PushNotifications;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    [RoutePrefix("api/image/thumbnails/tasks")]
    public class ThumbnailsTasksController : ApiController
    {
        private readonly IThumbnailTaskSearchService _thumbnailTaskSearchService;
        private readonly IThumbnailTaskService _thumbnailTaskService;

        private readonly IThumbnailGenerationProcessor _thumbnailProcessor;
        private readonly IPushNotificationManager _pushNotifier;
        private readonly IUserNameResolver _userNameResolver;

        public ThumbnailsTasksController(IThumbnailTaskSearchService thumbnailTaskSearchService, IThumbnailTaskService thumbnailTaskService, IPushNotificationManager pushNotifier, IUserNameResolver userNameResolver, IThumbnailGenerationProcessor thumbnailProcessor)
        {
            _thumbnailTaskSearchService = thumbnailTaskSearchService;
            _thumbnailTaskService = thumbnailTaskService;
            _pushNotifier = pushNotifier;
            _userNameResolver = userNameResolver;
            _thumbnailProcessor = thumbnailProcessor;
        }

        /// <summary>
        /// Creates thumbnail task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ThumbnailTask))]
        public IHttpActionResult Create(ThumbnailTask task)
        {
            _thumbnailTaskService.SaveOrUpdate(new[] { task });
            return Ok(task);
        }

        /// <summary>
        /// Remove thumbnail tasks by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete([FromUri] string[] ids)
        {
            _thumbnailTaskService.RemoveByIds(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Returns thumbnail task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(ThumbnailTask))]
        public IHttpActionResult Get(string id)
        {
            var task = _thumbnailTaskService.GetByIds(new[] { id });
            return Ok(task.FirstOrDefault());
        }

        /// <summary>
        /// Searches thumbnail options by certain criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [ResponseType(typeof(SearchResult<ThumbnailTask>))]
        public SearchResult<ThumbnailTask> Search(ThumbnailTaskSearchCriteria criteria)
        {
            var result = _thumbnailTaskSearchService.Search(criteria);

            var searchResult = new SearchResult<ThumbnailTask> { Result = result.Results.ToArray(), TotalCount = result.TotalCount };

            return searchResult;
        }

        /// <summary>
        /// Updates thumbnail tasks
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Update(ThumbnailTask tasks)
        {
            _thumbnailTaskService.SaveOrUpdate(new[] { tasks });
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("{id}/cancel")]
        public IHttpActionResult Cancel(string id)
        {
            return Ok();
        }

        [HttpPost]
        [Route("run")]
        [ResponseType(typeof(ThumbnailProcessNotification))]
        public IHttpActionResult Run(ThumbnailsTaskRunRequest runRequest)
        {
            var notification = Enqueue(runRequest);
            _pushNotifier.Upsert(notification);
            return Ok(notification);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ThumbnailProcessNotification Enqueue(ThumbnailsTaskRunRequest runRequest)
        {
            var notification = new ThumbnailProcessNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Process images",
                Description = "starting process...."
            };
            _pushNotifier.Upsert(notification);

            BackgroundJob.Enqueue(() => BackgroundProcess(runRequest, notification, JobCancellationToken.Null));

            return notification;
        }

        // Only public methods can be invoked in the background. (Hangfire)
        // Hangfire will set the cancellation token.
        [ApiExplorerSettings(IgnoreApi = true)]
        public void BackgroundProcess(ThumbnailsTaskRunRequest generateRequest, ThumbnailProcessNotification notifyEvent, IJobCancellationToken cancellationToken)
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

                    _pushNotifier.Upsert(notifyEvent);
                };

                //wrap token 
                var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);

                var tasks = _thumbnailTaskService.GetByIds(generateRequest.TaskIds);

                _thumbnailProcessor.ProcessTasksAsync(tasks, generateRequest.Regenerate, progressCallback, cancellationTokenWrapper);
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
                _pushNotifier.Upsert(notifyEvent);
            }
        }
    }
}