namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Description;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Web.Models;

    [RoutePrefix("api/image/thumbnails/tasks")]
    public class ThumbnailsTasksController : ApiController
    {
        private readonly IThumbnailTaskSearchService _thumbnailTaskSearchService;
        private readonly IThumbnailTaskService _thumbnailTaskService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thumbnailTaskSearchService"></param>
        /// <param name="thumbnailTaskService"></param>
        public ThumbnailsTasksController(
            IThumbnailTaskSearchService thumbnailTaskSearchService,
            IThumbnailTaskService thumbnailTaskService)
        {
            this._thumbnailTaskSearchService = thumbnailTaskSearchService;
            this._thumbnailTaskService = thumbnailTaskService;
        }

        /// <summary>
        /// Cancels thumbnail task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/cancel")]
        public IHttpActionResult Cancel(string id)
        {
            return this.Ok();
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
            this._thumbnailTaskService.SaveOrUpdate(new[] { task });
            return this.Ok(task);
        }

        /// <summary>
        /// Remove thumbnail tasks by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(string[] ids)
        {
            this._thumbnailTaskService.RemoveByIds(ids);
            return this.StatusCode(HttpStatusCode.NoContent);
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
            var task = this._thumbnailTaskService.GetByIds(new[] { id });
            return this.Ok(task);
        }

        /// <summary>
        /// Runs thumbnail task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/run")]
        public IHttpActionResult Run(string id)
        {
            return this.Ok();
        }

        /// <summary>
        /// Searches thumbnail options by certain criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(SearchResult<ThumbnailTask>))]
        public SearchResult<ThumbnailTask> Search(ThumbnailTaskSearchCriteria criteria)
        {
            var result = this._thumbnailTaskSearchService.SerchTasks(criteria);

            var searchResult =
                new SearchResult<ThumbnailTask> { Result = result.Results.ToArray(), TotalCount = result.TotalCount };

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
            this._thumbnailTaskService.SaveOrUpdate(new[] { tasks });
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }
}