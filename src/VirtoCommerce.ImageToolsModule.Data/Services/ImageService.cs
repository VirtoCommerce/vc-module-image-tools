using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Imaging;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ImageService : IImageService
    {
        private readonly IBlobStorageProvider _storageProvider;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IBlobStorageProvider storageProvider, ILogger<ImageService> logger)
        {
            _storageProvider = storageProvider;
            _logger = logger;
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object, or null when the blob is missing, corrupt or not in a supported format.</returns>
        public virtual async Task<RasterImage> LoadImageAsync(string imageUrl)
        {
            _logger.LogInformation("Loading image {imageUrl}", imageUrl);

            try
            {
                await using var blobStream = await _storageProvider.OpenReadAsync(imageUrl);
                using var content = new MemoryStream();
                await blobStream.CopyToAsync(content);

                var image = SkiaImageCodec.Decode(content.ToArray());
                if (image == null)
                {
                    _logger.LogError("Could not load image {imageUrl}: not an image in a supported format", imageUrl);
                }

                return image;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load image {imageUrl}", imageUrl);
            }

            return null!;
        }

        /// <inheritdoc />
        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        /// <param name="jpegQuality">Target image quality.</param>
        public virtual async Task SaveImageAsync(string imageUrl, RasterImage image, RasterImageFormat format, JpegQuality jpegQuality)
        {
            var content = SkiaImageCodec.Encode(AsSkia(image), format, jpegQuality);

            await using var blobStream = await _storageProvider.OpenWriteAsync(imageUrl);
            await blobStream.WriteAsync(content);
        }

        private static SkiaRasterImage AsSkia(RasterImage image)
        {
            return image as SkiaRasterImage
                ?? throw new ArgumentException($"Expected an image created by {nameof(ImageService)}, got {image?.GetType().Name ?? "null"}.", nameof(image));
        }
    }
}
