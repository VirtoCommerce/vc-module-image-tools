using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Default implementation of thumbnail handler factory.
    /// Routes image processing to format-specific handlers based on format detection.
    /// </summary>
    public class DefaultThumbnailHandlerFactory : IThumbnailHandlerFactory
    {
        private readonly List<IFormatThumbnailHandler> _handlers;

        public DefaultThumbnailHandlerFactory(IEnumerable<IFormatThumbnailHandler> handlers)
        {
            // Order handlers by priority (highest first)
            _handlers = handlers.OrderByDescending(h => h.Priority).ToList();
        }

        /// <inheritdoc />
        public async Task<IFormatThumbnailHandler> GetHandlerAsync(string imageUrl)
        {
            foreach (var handler in _handlers)
            {
                if (await handler.CanHandleAsync(imageUrl))
                {
                    return handler;
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
    }
}
