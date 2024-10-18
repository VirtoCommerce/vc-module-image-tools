using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class DefaultImageService : IImageService
    {
        private readonly IBlobStorageProvider _storageProvider;
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger<DefaultImageService> _logger;

        private IList<IImageFormat> _allowedImageFormats;

        public DefaultImageService(IBlobStorageProvider storageProvider, ISettingsManager settingsManager, ILogger<DefaultImageService> logger)
        {
            _storageProvider = storageProvider;
            _settingsManager = settingsManager;
            _logger = logger;
        }

        public virtual async Task<bool> IsFileExtensionAllowed(string path)
        {
            var allowedImageFormats = await GetAllowedImageFormats();

            var extension = Path.GetExtension(path).TrimStart('.');

            return allowedImageFormats.SelectMany(x => x.FileExtensions).Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public virtual async Task<bool> IsImageFormatAllowed(IImageFormat format)
        {
            var allowedImageFormats = await GetAllowedImageFormats();

            return allowedImageFormats.Any(f => string.Equals(f.Name, format.Name, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<IList<IImageFormat>> GetAllowedImageFormats()
        {
            if (_allowedImageFormats == null)
            {
                var allowedImageFormatsSetting = await _settingsManager.GetObjectSettingAsync(ModuleConstants.Settings.General.AllowedImageFormats.Name);
                var allowedFormatNames = allowedImageFormatsSetting.AllowedValues.OfType<string>().ToArray();

                _allowedImageFormats = Configuration.Default.ImageFormats
                    .Where(x => allowedFormatNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                    .ToArray();
            }

            return _allowedImageFormats;
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <param name="format">image format.</param>
        /// <returns>Image object.</returns>
        public virtual async Task<Image<Rgba32>> LoadImageAsync(string imageUrl)
        {
            _logger.LogInformation("Loading image {imageUrl}", imageUrl);
            try
            {
                using var blobStream = _storageProvider.OpenRead(imageUrl);

                var imageFormat = await Image.DetectFormatAsync(blobStream);

                if (await IsImageFormatAllowed(imageFormat))
                {
                    return await Image.LoadAsync<Rgba32>(blobStream);
                }
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
        public virtual async Task SaveImageAsync(string imageUrl, Image<Rgba32> image, IImageFormat format, JpegQuality jpegQuality)
        {
            await using var blobStream = await _storageProvider.OpenWriteAsync(imageUrl);
            using var stream = new MemoryStream();

            if (format.DefaultMimeType == JpegFormat.Instance.DefaultMimeType)
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
