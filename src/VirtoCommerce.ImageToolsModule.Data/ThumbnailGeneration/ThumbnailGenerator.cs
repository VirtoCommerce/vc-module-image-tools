using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Generates thumbnails by certain criteria
    /// </summary>
    public class ThumbnailGenerator : IThumbnailGenerator
    {
        private readonly IImageService _imageService;
        private readonly IImageResizer _imageResizer;
        private readonly ILogger<ThumbnailGenerator> _logger;

        public ThumbnailGenerator(IImageService imageService, IImageResizer imageResizer, ILogger<ThumbnailGenerator> logger)
        {
            _imageService = imageService;
            _imageResizer = imageResizer;
            _logger = logger;
        }

        /// <summary>
        /// Generates thumbnails asynchronously
        /// </summary>
        /// <param name="source">Path to source picture</param>
        /// <param name="destination">Target for generated thumbnail</param>
        /// <param name="options">Represents generation options</param>
        /// <param name="token">Allows cancel operation</param>
        /// <returns></returns>
        public virtual async Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(string source, string destination, IList<ThumbnailOption> options, ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();

            var originalImage = await _imageService.LoadImageAsync(source);
            if (originalImage == null)
            {
                return null;
            }

            var result = new ThumbnailGenerationResult();

            using (originalImage)
            {
                foreach (var option in options)
                {
                    var thumbnailUrl = source.GenerateThumbnailName(option.FileSuffix);
                    var thumbnail = GenerateThumbnail(originalImage, option);

                    using (thumbnail)
                    {
                        try
                        {
                            _ = thumbnail ?? throw new PlatformException($"Cannot save thumbnail image {thumbnailUrl}");

                            await _imageService.SaveImageAsync(thumbnailUrl, thumbnail, originalImage.Metadata.DecodedImageFormat, option.JpegQuality);

                            result.GeneratedThumbnails.Add(thumbnailUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, @"Cannot save thumbnail image {url}, error {ex}", thumbnailUrl, ex);
                            result.Errors.Add($"Cannot save thumbnail image {thumbnailUrl}");
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///Generates a Thumbnail
        /// </summary>
        /// <param name="image"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        protected virtual Image<Rgba32> GenerateThumbnail(Image<Rgba32> image, ThumbnailOption option)
        {
            var height = option.Height ?? image.Height;
            var width = option.Width ?? image.Width;

            var color = Color.Transparent;
            if (!string.IsNullOrWhiteSpace(option.BackgroundColor))
            {
                color = Rgba32.ParseHex(option.BackgroundColor);
            }

            var result = option.ResizeMethod switch
            {
                ResizeMethod.FixedSize => _imageResizer.FixedSize(image, width, height, color),
                ResizeMethod.FixedWidth => _imageResizer.FixedWidth(image, width, color),
                ResizeMethod.FixedHeight => _imageResizer.FixedHeight(image, height, color),
                ResizeMethod.Crop => _imageResizer.Crop(image, width, height, option.AnchorPosition),
                _ => throw new ArgumentOutOfRangeException($"ResizeMethod {option.ResizeMethod} not supported."),
            };

            return result;
        }
    }
}
