using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Handler for raster image formats (JPEG, PNG, WebP, etc.).
    /// Delegates processing to the existing IThumbnailGenerator implementation.
    /// </summary>
    public class RasterThumbnailHandler : IFormatThumbnailHandler
    {
        private readonly IThumbnailGenerator _thumbnailGenerator;

        public RasterThumbnailHandler(
            IThumbnailGenerator thumbnailGenerator)
        {
            _thumbnailGenerator = thumbnailGenerator;
        }

        /// <inheritdoc />
        public int Priority => 0; // Default priority for base raster handler

        /// <inheritdoc />
        public Task<bool> CanHandleAsync(string imageUrl)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
            string source,
            string destination,
            IList<ThumbnailOption> options,
            ICancellationToken token)
        {
            // Delegate to existing implementation - zero changes to raster processing
            return _thumbnailGenerator.GenerateThumbnailsAsync(source, destination, options, token);
        }
    }
}
