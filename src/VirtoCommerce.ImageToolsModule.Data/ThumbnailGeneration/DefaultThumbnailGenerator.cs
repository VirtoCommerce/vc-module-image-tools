using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Generates thumbnails by certain criteria
    /// </summary>
    public class DefaultThumbnailGenerator : IThumbnailGenerator
    {
        private readonly IImageService _imageService;
        private readonly IImageResizer _imageResizer;
        private readonly ILogger<DefaultThumbnailGenerator> _logger;

        public DefaultThumbnailGenerator(IImageService imageService, IImageResizer imageResizer, ILogger<DefaultThumbnailGenerator> logger)
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

            var originalImage = await _imageService.LoadImageAsync(source, out var format);
            if (originalImage == null)
            {
                return null;
            }

            var result = new ThumbnailGenerationResult();

            foreach (var option in options)
            {
                var thumbnail = GenerateThumbnail(originalImage, option);
                var thumbnailUrl = source.GenerateThumbnailName(option.FileSuffix);

                try
                {
                    _ = thumbnail ?? throw new PlatformException($"Cannot save thumbnail image {thumbnailUrl}");

                    await _imageService.SaveImageAsync(thumbnailUrl, thumbnail, format, option.JpegQuality);

                    result.GeneratedThumbnails.Add(thumbnailUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Cannot save thumbnail image {thumbnailUrl}, error {ex}");
                    result.Errors.Add($"Cannot save thumbnail image {thumbnailUrl}");
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

            Image<Rgba32> result;
            switch (option.ResizeMethod)
            {
                case ResizeMethod.FixedSize:
                    result = _imageResizer.FixedSize(image, width, height, color);
                    break;
                case ResizeMethod.FixedWidth:
                    result = _imageResizer.FixedWidth(image, width, color);
                    break;
                case ResizeMethod.FixedHeight:
                    result = _imageResizer.FixedHeight(image, height, color);
                    break;
                case ResizeMethod.Crop:
                    result = _imageResizer.Crop(image, width, height, option.AnchorPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"ResizeMethod {option.ResizeMethod.ToString()} not supported.");
            }

            return result;
        }
    }
}
