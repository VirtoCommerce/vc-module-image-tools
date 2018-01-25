namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Http.Results;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/image/thumbnails/tasks")]
    public class ThumbnailsTasksController : ApiController
    {
        private readonly IThumbnailRepository _repository;

        public ThumbnailsTasksController(IThumbnailRepository repository)
        {
            this._repository = repository;
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Search(ThumbnailTaskSearchCriteria criteria)
        {
            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPut]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Update(ThumbnailTask tasks)
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ThumbnailTask))]
        public IHttpActionResult Create(ThumbnailTask task)
        {
            return Ok(task);
        }

        [HttpGet]
        [Route("{taskId}")]
        [ResponseType(typeof(ThumbnailTask))]
        public IHttpActionResult Get(string taskId)
        {
            return Ok();
        }

        [HttpDelete]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(string[] tasksIds)
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("{taskId}/run")]
        public IHttpActionResult Run(string taskId)
        {
            return Ok();
        }

        [HttpGet]
        [Route("{taskId}/cancel")]
        public IHttpActionResult Cancel(string taskId)
        {
            return Ok();
        }
    }
}