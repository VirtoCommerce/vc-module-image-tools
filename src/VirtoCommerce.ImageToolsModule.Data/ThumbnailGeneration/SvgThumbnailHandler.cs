using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Handler for SVG vector format.
    /// SVG "thumbnails" are created by modifying dimensions, not rasterizing.
    /// The output remains as SVG files, preserving vector scalability.
    /// </summary>
    public class SvgThumbnailHandler : IFormatThumbnailHandler
    {
        private readonly ISvgService _svgService;
        private readonly ISvgResizer _svgResizer;
        private readonly IImageFormatDetector _formatDetector;
        private readonly ILogger<SvgThumbnailHandler> _logger;

        public SvgThumbnailHandler(
            ISvgService svgService,
            ISvgResizer svgResizer,
            IImageFormatDetector formatDetector,
            ILogger<SvgThumbnailHandler> logger)
        {
            _svgService = svgService;
            _svgResizer = svgResizer;
            _formatDetector = formatDetector;
            _logger = logger;
        }

        /// <inheritdoc />
        public ImageFormatType SupportedFormatType => ImageFormatType.Vector;

        /// <inheritdoc />
        public int Priority => 0; // Default priority

        /// <inheritdoc />
        public Task<bool> CanHandleAsync(string imageUrl)
        {
            var formatType = _formatDetector.DetectFormatType(imageUrl);
            return Task.FromResult(formatType == ImageFormatType.Vector);
        }

        /// <inheritdoc />
        public async Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
            string source,
            string destination,
            IList<ThumbnailOption> options,
            ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();

            var result = new ThumbnailGenerationResult();

            try
            {
                var svgContent = await _svgService.LoadSvgAsync(source);
                if (string.IsNullOrEmpty(svgContent))
                {
                    result.Errors.Add($"Could not load SVG: {source}");
                    return result;
                }

                // Ensure SVG has viewBox for proper scaling
                svgContent = _svgResizer.EnsureViewBox(svgContent);

                foreach (var option in options)
                {
                    token?.ThrowIfCancellationRequested();

                    var thumbnailUrl = source.GenerateThumbnailName(option.FileSuffix);

                    try
                    {
                        var resizedSvg = _svgResizer.Resize(
                            svgContent,
                            option.Width,
                            option.Height,
                            option.ResizeMethod);

                        await _svgService.SaveSvgAsync(thumbnailUrl, resizedSvg);
                        result.GeneratedThumbnails.Add(thumbnailUrl);

                        _logger.LogInformation("Generated SVG thumbnail {url}", thumbnailUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not generate SVG thumbnail {url}", thumbnailUrl);
                        result.Errors.Add($"Could not generate SVG thumbnail {thumbnailUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SVG {source}", source);
                result.Errors.Add($"Error processing SVG: {source}");
            }

            return result;
        }
    }
}
