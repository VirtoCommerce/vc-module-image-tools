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
        public static IChangeToken CreateChangeToken(ThumbnailTask task, DateTime? changedSince)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var regionTokenKey = GetRegionTokenKey(task, changedSince);

            return CreateChangeTokenForKey(regionTokenKey);
        }

        public static void ExpireTaskRun(ThumbnailTask task, DateTime? changedSince)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var regionTokenKey = GetRegionTokenKey(task, changedSince);

            ExpireTokenForKey(regionTokenKey);
        }

        private static string GetRegionTokenKey(ThumbnailTask task, DateTime? changedSince)
        {
            return CacheKey.With(task.WorkPath, changedSince?.ToString());
        }
    }
}
