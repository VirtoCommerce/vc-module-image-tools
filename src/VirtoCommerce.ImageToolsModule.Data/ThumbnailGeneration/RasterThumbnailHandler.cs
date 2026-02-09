using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Extensions;
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

        private static readonly Lazy<HashSet<string>> _supportedExtensions = new(() =>
            new HashSet<string>(
                Configuration.Default.ImageFormats.SelectMany(f => f.FileExtensions),
                StringComparer.OrdinalIgnoreCase));

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
            var extension = UrlExtensions.GetFileExtensionWithoutDot(imageUrl);
            return Task.FromResult(
                extension.Length > 0 && _supportedExtensions.Value.Contains(extension));
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
