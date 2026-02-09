using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Thumbnail handler factory.
    /// Routes image processing to format-specific handlers based on format detection.
    /// </summary>
    public class ThumbnailHandlerFactory : IThumbnailHandlerFactory
    {
        private readonly List<IFormatThumbnailHandler> _handlers;
        private readonly IAllowedImageFormatsService _allowedImageFormatsService;

        public ThumbnailHandlerFactory(
            IEnumerable<IFormatThumbnailHandler> handlers,
            IAllowedImageFormatsService allowedImageFormatsService)
        {
            _allowedImageFormatsService = allowedImageFormatsService;
            // Order handlers by priority (highest first)
            _handlers = handlers.OrderByDescending(h => h.Priority).ToList();
        }

        /// <inheritdoc />
        public async Task<IFormatThumbnailHandler> GetHandlerAsync(string imageUrl)
        {
            if (await _allowedImageFormatsService.IsAllowedAsync(imageUrl))
            {
                foreach (var handler in _handlers)
                {
                    if (await handler.CanHandleAsync(imageUrl))
                    {
                        return handler;
                    }
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void RegisterHandler(IFormatThumbnailHandler handler)
        {
            _handlers.Add(handler);
            // Re-sort by priority
            _handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public void UnregisterHandler(IFormatThumbnailHandler handler)
        {
            _handlers.Remove(handler);
            // Re-sort by priority
            _handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
    }
}
