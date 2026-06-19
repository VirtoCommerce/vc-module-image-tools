using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IImagesChangesProvider
    {
        bool IsTotalCountSupported { get; }

        Task<long> GetTotalChangesCount(ThumbnailTask task, DateTime? changedSince, CancellationToken cancellationToken);

        Task<IList<ImageChange>> GetNextChangesBatch(ThumbnailTask task, DateTime? changedSince, long? skip, long? take, CancellationToken cancellationToken);
    }
}
