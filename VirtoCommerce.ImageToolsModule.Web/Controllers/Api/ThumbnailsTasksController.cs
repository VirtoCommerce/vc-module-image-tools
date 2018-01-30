using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;

namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/image/thumbnails/tasks")]
    public class ThumbnailsTasksController : ApiController
    {
        private IThumbnailTaskSearchService thumbnailTaskSearchService;

        private IThumbnailOptionService thumbnailOptionService;

        private IThumbnailTaskService thumbnailTaskService;

        public ThumbnailsTasksController(IThumbnailTaskSearchService thumbnailTaskSearchService, IThumbnailOptionService thumbnailOptionService, IThumbnailTaskService thumbnailTaskService)
        {
            this.thumbnailTaskSearchService = thumbnailTaskSearchService;
            this.thumbnailOptionService = thumbnailOptionService;
            this.thumbnailTaskService = thumbnailTaskService;
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