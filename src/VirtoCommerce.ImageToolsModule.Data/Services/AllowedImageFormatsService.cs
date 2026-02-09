using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Extensions;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Allowed image formats service implementation.
    /// Retrieves allowed formats from module settings.
    /// </summary>
    public class AllowedImageFormatsService : IAllowedImageFormatsService
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public AllowedImageFormatsService(ISettingsManager settingsManager, IPlatformMemoryCache platformMemoryCache)
        {
            _settingsManager = settingsManager;
            _platformMemoryCache = platformMemoryCache;
        }

        /// <inheritdoc />
        public async Task<bool> IsAllowedAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var allowedImageFormats = await GetAllowedFormatsAsync();
            var extension = UrlExtensions.GetFileExtensionWithoutDot(url);

            return allowedImageFormats.Any(x => x.EqualsIgnoreCase(extension));
        }

        private Task<string[]> GetAllowedFormatsAsync()
        {
            var cacheKey = CacheKey.With(GetType(), "GetAllowedFormatsAsync");

            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                var allowedImageFormatsSetting = await _settingsManager.GetObjectSettingAsync(ModuleConstants.Settings.General.AllowedImageFormats.Name);

                cacheEntry.AddExpirationToken(SettingsCacheRegion.CreateChangeToken(allowedImageFormatsSetting));

                var allowedFormatNames = allowedImageFormatsSetting.AllowedValues.OfType<string>().ToArray();

                return allowedFormatNames;
            });
        }
    }
}
