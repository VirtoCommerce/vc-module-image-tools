using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.ImageToolsModule.Data.Caching
{
    public class BlobChangesCacheRegion : CancellableCacheRegion<BlobChangesCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _regionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static IChangeToken CreateChangeToken(ThumbnailTask task, DateTime? changedSince)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var regionTokenKey = GetRegionTokenKey(task, changedSince);
            var cancellationTokenSource = _regionTokenLookup.GetOrAdd(regionTokenKey, new CancellationTokenSource());

            return new CompositeChangeToken(new[] { CreateChangeToken(), new CancellationChangeToken(cancellationTokenSource.Token) });
        }

        public static void ExpireTaskRun(ThumbnailTask task, DateTime? changesSince)
        {
            var regionTokenKey = GetRegionTokenKey(task, changesSince);

            if (_regionTokenLookup.TryRemove(regionTokenKey, out var token))
            {
                token.Cancel();
            }
        }

        private static string GetRegionTokenKey(ThumbnailTask task, DateTime? changedSince)
        {
            return CacheKey.With(task.WorkPath, changedSince?.ToString());
        }
    }
}
