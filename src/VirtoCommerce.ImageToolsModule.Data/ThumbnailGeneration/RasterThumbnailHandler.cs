using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
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
        private readonly IImageFormatDetector _formatDetector;

        public RasterThumbnailHandler(
            IThumbnailGenerator thumbnailGenerator,
            IImageFormatDetector formatDetector)
        {
            _thumbnailGenerator = thumbnailGenerator;
            _formatDetector = formatDetector;
        }

        /// <inheritdoc />
        public ImageFormatType SupportedFormatType => ImageFormatType.Raster;

        /// <inheritdoc />
        public int Priority => 0; // Default priority for base raster handler

        /// <inheritdoc />
        public Task<bool> CanHandleAsync(string imageUrl)
        {
            var formatType = _formatDetector.DetectFormatType(imageUrl);
            return Task.FromResult(formatType == ImageFormatType.Raster);
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
