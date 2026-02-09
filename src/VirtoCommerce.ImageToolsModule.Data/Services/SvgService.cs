using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Extensions;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// SVG service for loading and saving SVG files
    /// </summary>
    public class SvgService : ISvgService
    {
        private static readonly string[] SvgExtensions = ["svg"];

        private readonly IBlobStorageProvider _storageProvider;
        private readonly ISvgResizer _svgResizer;
        private readonly ILogger<SvgService> _logger;

        public SvgService(
            IBlobStorageProvider storageProvider,
            ISvgResizer svgResizer,
            ILogger<SvgService> logger)
        {
            _storageProvider = storageProvider;
            _svgResizer = svgResizer;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> LoadSvgAsync(string svgUrl)
        {
            _logger.LogDebug("Loading SVG {svgUrl}", svgUrl);

            try
            {
                await using var stream = await _storageProvider.OpenReadAsync(svgUrl);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load SVG {svgUrl}", svgUrl);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task SaveSvgAsync(string svgUrl, string svgContent)
        {
            _logger.LogDebug("Saving SVG {svgUrl}", svgUrl);

            try
            {
                await using var blobStream = await _storageProvider.OpenWriteAsync(svgUrl);
                await using var writer = new StreamWriter(blobStream);
                await writer.WriteAsync(svgContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save SVG {svgUrl}", svgUrl);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<SvgDimensions> GetDimensionsAsync(string svgUrl)
        {
            var content = await LoadSvgAsync(svgUrl);
            if (string.IsNullOrEmpty(content))
            {
                return new SvgDimensions();
            }

            return _svgResizer.ParseDimensions(content);
        }

        /// <inheritdoc />
        public bool IsSvgFile(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var extension = UrlExtensions.GetFileExtensionWithoutDot(url);
            return Array.Exists(SvgExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
