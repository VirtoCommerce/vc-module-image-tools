using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Default implementation of image format detector using file extensions
    /// </summary>
    public class DefaultImageFormatDetector : IImageFormatDetector
    {
        private static readonly string[] VectorExtensions = { ".svg", ".svgz" };
        private static readonly string[] RasterExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff", ".tif" };

        private readonly IImageService _imageService;
        private readonly ISettingsManager _settingsManager;

        public DefaultImageFormatDetector(IImageService imageService, ISettingsManager settingsManager)
        {
            _imageService = imageService;
            _settingsManager = settingsManager;
        }

        /// <inheritdoc />
        public ImageFormatType DetectFormatType(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return ImageFormatType.Raster;
            }

            var extension = Path.GetExtension(imageUrl)?.ToLowerInvariant();
            return VectorExtensions.Contains(extension)
                ? ImageFormatType.Vector
                : ImageFormatType.Raster;
        }

        /// <inheritdoc />
        public Task<ImageFormatType> DetectFormatTypeAsync(Stream stream)
        {
            // For now, use header-based detection for SVG
            // SVG files typically start with "<?xml" or "<svg"
            if (stream == null || !stream.CanRead)
            {
                return Task.FromResult(ImageFormatType.Raster);
            }

            try
            {
                var position = stream.Position;
                var buffer = new byte[256];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                stream.Position = position; // Reset position

                if (bytesRead > 0)
                {
                    var header = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimStart();
                    if (header.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                        header.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(ImageFormatType.Vector);
                    }
                }
            }
            catch
            {
                // Ignore errors and default to raster
            }

            return Task.FromResult(ImageFormatType.Raster);
        }

        /// <inheritdoc />
        public async Task<bool> IsFormatSupportedAsync(string imageUrl)
        {
            var formatType = DetectFormatType(imageUrl);

            if (formatType == ImageFormatType.Vector)
            {
                var extension = Path.GetExtension(imageUrl)?.ToLowerInvariant();
                if (!VectorExtensions.Contains(extension))
                {
                    return false;
                }

                // Check if SVG is enabled in AllowedImageFormats setting
                var allowedFormatsSetting = await _settingsManager.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.AllowedImageFormats.Name);
                var allowedFormats = allowedFormatsSetting?.AllowedValues?.OfType<string>() ?? [];

                return allowedFormats.Contains(ModuleConstants.SvgFormatName, StringComparer.OrdinalIgnoreCase);
            }

            // For raster formats, delegate to existing IImageService
            return await _imageService.IsFileExtensionAllowedAsync(imageUrl);
        }

        /// <summary>
        /// Check if the file extension is a known vector format
        /// </summary>
        public static bool IsVectorExtension(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            var extension = Path.GetExtension(imageUrl)?.ToLowerInvariant();
            return VectorExtensions.Contains(extension);
        }

        /// <summary>
        /// Check if the file extension is a known raster format
        /// </summary>
        public static bool IsRasterExtension(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            var extension = Path.GetExtension(imageUrl)?.ToLowerInvariant();
            return RasterExtensions.Contains(extension);
        }
    }
}
