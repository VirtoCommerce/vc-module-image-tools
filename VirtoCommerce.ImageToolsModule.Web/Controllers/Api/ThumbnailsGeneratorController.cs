using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.ImageToolsModule.Web.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.ImageToolsModule.Web.Controllers.Api
{
    /// <summary>
    /// Thumbnail of an image is an image with another size(resolution). 
    /// The thumbnails size is less of original size.
    /// Thumbnails are using in an interface, where it doesn't need high resolution.
    /// For example, in listings, short views.
    /// The Api controller allows to generate different thumbnails, get list of existed thumbnails
    /// </summary>
    [RoutePrefix("api/image/thumbnails")]
    public class ThumbnailsController : ApiController
    {
        private readonly IThumbnailService _thumbnailsGenerator;
        private readonly IBlobUrlResolver _blobUrlResolver;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailsController(IThumbnailService thumbnailsGenerator, IBlobUrlResolver blobUrlResolver)
        {
            _thumbnailsGenerator = thumbnailsGenerator;
            _blobUrlResolver = blobUrlResolver;
        }


        /// <summary>
        /// Get all existed thumbnails urls of given image.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="aliases">Thumbnails aliases (suffixes).</param>
        /// <returns>List of existed thumbnails.</returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult GetThumbnails(string imageUrl, [FromUri] string[] aliases)
        {
            var result = new string[0];
            try
            {
                result = _thumbnailsGenerator.GetThumbnails(imageUrl, aliases);
            }
            catch (Exception e)
            {
            }

            return Ok(result);
        }

        /// <summary>
        /// Generate a number thumbnails of original image by given settings.
        /// </summary>
        /// <returns>True, if successfully done</returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(GenerateThumbnailsResponse))]
        public async Task<IHttpActionResult> GenerateAsync(GenerateThumbnailsRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            try
            {
                var result = await _thumbnailsGenerator.GenerateAsync(request.ImageUrl, request.ThumbnailsParameters, request.IsRegenerateAll);

                return Ok(new GenerateThumbnailsResponse());
            }
            catch (Exception e)
            {
            }

            return Ok(new GenerateThumbnailsResponse { Error= "Thumbnails creation error" });
        }
    }
}
