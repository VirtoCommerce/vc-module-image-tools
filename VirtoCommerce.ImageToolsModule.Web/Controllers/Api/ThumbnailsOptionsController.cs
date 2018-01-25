namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Http.Results;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/image/thumbnails/options")]
    public class ThumbnailsOptionsController : ApiController
    {
        private IThumbnailTaskSearchService thumbnailTaskSearchService;

        private IThumbnailOptionService thumbnailOptionService;

        private IThumbnailTaskService thumbnailTaskService;

        public ThumbnailsOptionsController(IThumbnailTaskSearchService thumbnailTaskSearchService, IThumbnailOptionService thumbnailOptionService, IThumbnailTaskService thumbnailTaskService)
        {
            this.thumbnailTaskSearchService = thumbnailTaskSearchService;
            this.thumbnailOptionService = thumbnailOptionService;
            this.thumbnailTaskService = thumbnailTaskService;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ThumbnailOption))]
        public IHttpActionResult Search(ThumbnailOptionSearchCriteria criteria)
        {
            return Ok();
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
        [Route("{optionId}")]
        [ResponseType(typeof(ThumbnailOption))]
        public IHttpActionResult Get(string optionId)
        {
            return Ok();
        }

        [HttpDelete]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(string[] optionIds)
        {
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}