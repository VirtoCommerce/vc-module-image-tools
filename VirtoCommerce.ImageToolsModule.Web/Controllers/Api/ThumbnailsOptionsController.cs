using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Web.Models;

namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    /// <summary>
    /// Thumbnails options controller
    /// </summary>
    [RoutePrefix("api/image/thumbnails/options")]
    public class ThumbnailsOptionsController : ApiController
    {
        private readonly IThumbnailOptionService _thumbnailOptionService;

        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thumbnailOptionService"></param>
        /// <param name="thumbnailOptionSearchService"></param>
        public ThumbnailsOptionsController(IThumbnailOptionService thumbnailOptionService, IThumbnailOptionSearchService thumbnailOptionSearchService)
        {
            this._thumbnailOptionService = thumbnailOptionService;
            this._thumbnailOptionSearchService = thumbnailOptionSearchService;
        }

        /// <summary>
        /// Creates thumbnail option
        /// </summary>
        /// <param name="option">thumbnail option</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ThumbnailOption))]
        public IHttpActionResult Create(ThumbnailOption option)
        {
            this._thumbnailOptionService.SaveOrUpdate(new[] { option });
            return Ok(option);
        }

        /// <summary>
        /// Remove thumbnail options by ids
        /// </summary>
        /// <param name="ids">options ids</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete([FromUri] string[] ids)
        {
            this._thumbnailOptionService.RemoveByIds(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Gets thumbnail options
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(ThumbnailOption))]
        public IHttpActionResult Get(string id)
        {
            var option = this._thumbnailOptionService.GetByIds(new[] { id });
            return Ok(option);
        }

        /// <summary>
        /// Searches thumbnail options
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(SearchResult<ThumbnailOption>))]
        public SearchResult<ThumbnailOption> Search(ThumbnailOptionSearchCriteria criteria)
        {
            var result = this._thumbnailOptionSearchService.Search(criteria);

            var searchResult = new SearchResult<ThumbnailOption>
            {
                Result = result.Results.ToArray(),
                TotalCount = result.TotalCount
            };

            return searchResult;
        }

        /// <summary>
        /// Updates thumbnail options
        /// </summary>
        /// <param name="option">Thumbnail options</param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Update(ThumbnailOption option)
        {
            this._thumbnailOptionService.SaveOrUpdate(new[] { option });
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}