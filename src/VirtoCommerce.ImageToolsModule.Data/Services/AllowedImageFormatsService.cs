using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Allowed image formats service implementation.
    /// Retrieves allowed formats from module settings.
    /// </summary>
    public class AllowedImageFormatsService : IAllowedImageFormatsService
    {
        private readonly ISettingsManager _settingsManager;
        private string[] _allowedImageFormats;

        public AllowedImageFormatsService(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        /// <inheritdoc />
        public async Task<bool> IsAllowedAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var allowedImageFormats = await GetAllowedFormatsAsync();
            var extension = Path.GetExtension(url).TrimStart('.');
            return allowedImageFormats.Any(x => x.EqualsIgnoreCase(extension));
        }

        private async Task<string[]> GetAllowedFormatsAsync()
        {
            var formats = Volatile.Read(ref _allowedImageFormats);
            if (formats != null)
            {
                return formats;
            }

            var allowedImageFormatsSetting = await _settingsManager.GetObjectSettingAsync(ModuleConstants.Settings.General.AllowedImageFormats.Name);
            var allowedFormatNames = allowedImageFormatsSetting.AllowedValues.OfType<string>().ToArray();
            Interlocked.CompareExchange(ref _allowedImageFormats, allowedFormatNames, null);

            return _allowedImageFormats;
        }
    }
}
