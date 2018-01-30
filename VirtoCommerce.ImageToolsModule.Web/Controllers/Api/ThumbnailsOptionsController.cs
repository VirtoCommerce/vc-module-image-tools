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
    [RoutePrefix("api/image/thumbnails/options")]
    public class ThumbnailsOptionsController : ApiController
    {
        private IThumbnailTaskSearchService thumbnailTaskSearchService;

        private IThumbnailOptionService thumbnailOptionService;

        private IThumbnailTaskService thumbnailTaskService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thumbnailTaskSearchService"></param>
        /// <param name="thumbnailOptionService"></param>
        /// <param name="thumbnailTaskService"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
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