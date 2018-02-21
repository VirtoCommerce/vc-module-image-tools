using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.BackwardsCompatibility;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.ImageToolsModule.Web.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Web.Security;

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
        private readonly IThumbnailGenerator _thumbnailsGenerator;
        private readonly ISettingsManager _settingsManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailsController(IThumbnailGenerator thumbnailsGenerator, ISettingsManager settingsManager)
        {
            _thumbnailsGenerator = thumbnailsGenerator;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Generate a number thumbnails of original image by given settings.
        /// </summary>
        /// <returns>True, if successfully done</returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(GenerateThumbnailsResponse))]
        [CheckPermission(Permission = ThumbnailPredefinedPermissions.Read)]
        public async Task<IHttpActionResult> GenerateAsync(GenerateThumbnailsRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var setting = _settingsManager.GetSettingByName("ImageTools.Thumbnails.Parameters");
            if (setting == null)
                return Ok(new GenerateThumbnailsResponse());

            var settings = setting.ArrayValues ?? new string[] { };
            var options = settings.Select(x => JsonConvert.DeserializeObject<ThumbnailOption>(x, new SettingJsonConverter())).ToList();
            var result = await _thumbnailsGenerator.GenerateThumbnailsAsync(request.ImageUrl, null, options, null);

            return Ok(new GenerateThumbnailsResponse());
        }
    }
}