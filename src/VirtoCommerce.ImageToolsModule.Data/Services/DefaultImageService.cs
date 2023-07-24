using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class DefaultImageService : IImageService
    {
        private readonly IBlobStorageProvider _storageProvider;

        public DefaultImageService(IBlobStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <param name="format">image format.</param>
        /// <returns>Image object.</returns>
        public virtual Task<Image<Rgba32>> LoadImageAsync(string imageUrl, out IImageFormat format)
        {
            try
            {
                using var blobStream = _storageProvider.OpenRead(imageUrl);
                var result = Image.Load<Rgba32>(blobStream, out format);
                return Task.FromResult(result);
            }
            catch (Exception)
            {
                format = null!;
                return Task.FromResult<Image<Rgba32>>(null!);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        /// <param name="jpegQuality">Target image quality.</param>
        public virtual async Task SaveImageAsync(string imageUrl, Image<Rgba32> image, IImageFormat format, JpegQuality jpegQuality)
        {
            await using var blobStream = await _storageProvider.OpenWriteAsync(imageUrl);
            using var stream = new MemoryStream();

            if (format.DefaultMimeType == "image/jpeg")
            {
                var options = new JpegEncoder
                {
                    Quality = (int)jpegQuality
                };

                await image.SaveAsync(stream, options);
            }
            else
            {
                await image.SaveAsync(stream, format);
            }

            stream.Position = 0;
            await stream.CopyToAsync(blobStream);
        }
    }
}
