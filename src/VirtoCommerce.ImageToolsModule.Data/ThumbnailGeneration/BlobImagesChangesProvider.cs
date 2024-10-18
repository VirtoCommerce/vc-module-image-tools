using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class BlobImagesChangesProvider : IImagesChangesProvider
    {
        public bool IsTotalCountSupported => true;

        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;
        private readonly IImageService _imageService;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public BlobImagesChangesProvider(
            IBlobStorageProvider storageProvider,
            IThumbnailOptionSearchService thumbnailOptionSearchService,
            IImageService imageService,
            IPlatformMemoryCache platformMemoryCache)
        {
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
            _imageService = imageService;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<long> GetTotalChangesCount(ThumbnailTask task, DateTime? changedSince, ICancellationToken cancellationToken)
        {
            var changedFiles = await GetChangeFiles(task, changedSince, cancellationToken);

            return changedFiles.Count;
        }

        public async Task<IList<ImageChange>> GetNextChangesBatch(ThumbnailTask task, DateTime? changedSince, long? skip, long? take, ICancellationToken cancellationToken)
        {
            var changedFiles = await GetChangeFiles(task, changedSince, cancellationToken);

            return skip >= changedFiles.Count
                ? Array.Empty<ImageChange>()
                : changedFiles
                    .Skip((int)(skip ?? 0))
                    .Take((int)(take ?? 0))
                    .ToList();
        }


        protected virtual async Task<IList<ImageChange>> GetChangeFiles(ThumbnailTask task, DateTime? changedSince, ICancellationToken cancellationToken)
        {
            var options = await GetOptionsCollection();
            var cacheKey = CacheKey.With(GetType(), "GetChangeFiles", task.WorkPath, changedSince?.ToString(), string.Join(":", options.Select(x => x.FileSuffix)));
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(BlobChangesCacheRegion.CreateChangeToken(task, changedSince));

                var allBlobInfos = await ReadBlobFolderAsync(task.WorkPath, cancellationToken);
                var originalBlobInfos = GetOriginalItems(allBlobInfos.Values, options.Select(x => x.FileSuffix).ToList());

                var result = new ConcurrentBag<ImageChange>();
                await Parallel.ForEachAsync(originalBlobInfos, async (blobInfo, token) =>
                {
                    token.ThrowIfCancellationRequested();

                    var imageChange = new ImageChange
                    {
                        Name = blobInfo.Name,
                        Url = blobInfo.Url,
                        ModifiedDate = blobInfo.ModifiedDate,
                        ChangeState = !changedSince.HasValue ? EntryState.Added : await GetItemStateAsync(blobInfo, changedSince, task.ThumbnailOptions, allBlobInfos)
                    };
                    result.Add(imageChange);
                });

                return result.Where(x => x.ChangeState != EntryState.Unchanged).OrderBy(x => x.Url).ToList();
            });
        }

        protected virtual async Task<ConcurrentDictionary<string, BlobEntry>> ReadBlobFolderAsync(string folderPath, ICancellationToken cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            var result = new ConcurrentDictionary<string, BlobEntry>();

            var searchResults = await _storageProvider.SearchAsync(folderPath, null);

            // Add supported images
            foreach (var imageBlob in searchResults.Results)
            {
                if (await IsSupportedImage(imageBlob))
                {
                    result.TryAdd(imageBlob.Url, imageBlob);
                }
            }

            // Add images from child folders recursively
            await Parallel.ForEachAsync(searchResults.Results.Where(IsFolder), async (folderBlob, token) =>
            {
                var childFolderImages = await ReadBlobFolderAsync(folderBlob.RelativeUrl, new CancellationTokenWrapper(token));

                foreach (var imageBlob in childFolderImages.Values)
                {
                    result.TryAdd(imageBlob.Url, imageBlob);
                }
            });

            return result;
        }

        protected virtual Task<bool> IsSupportedImage(BlobEntry blobEntry)
        {
            return _imageService.IsFileExtensionAllowed(blobEntry.Name);
        }

        protected virtual bool IsFolder(BlobEntry blobEntry)
        {
            return blobEntry.Type.EqualsIgnoreCase("folder");
        }

        protected virtual async Task<EntryState> GetItemStateAsync(
            BlobEntry blobInfo,
            DateTime? changedSince,
            IList<ThumbnailOption> options,
            ConcurrentDictionary<string, BlobEntry> earlyReadBlobInfos = null)
        {
            if (!changedSince.HasValue)
            {
                return EntryState.Added;
            }

            foreach (var option in options)
            {
                if (!await ExistsAsync(blobInfo.Url.GenerateThumbnailName(option.FileSuffix), earlyReadBlobInfos))
                {
                    return EntryState.Added;
                }
            }

            if (blobInfo.ModifiedDate.HasValue && blobInfo.ModifiedDate >= changedSince)
            {
                return EntryState.Modified;
            }

            return EntryState.Unchanged;
        }

        /// <summary>
        /// Check if an image exist in the blob storage by url.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="earlyReadBlobInfos"></param>
        protected virtual async Task<bool> ExistsAsync(string imageUrl, ConcurrentDictionary<string, BlobEntry> earlyReadBlobInfos = null)
        {
            bool result;

            if (earlyReadBlobInfos == null)
            {
                result = await _storageProvider.GetBlobInfoAsync(imageUrl) != null;
            }
            else
            {
                result = earlyReadBlobInfos.ContainsKey(imageUrl);
            }

            return result;
        }

        //get all options to create a map of all potential file names
        protected virtual async Task<ICollection<ThumbnailOption>> GetOptionsCollection()
        {
            var options = await _thumbnailOptionSearchService.SearchNoCloneAsync(new ThumbnailOptionSearchCriteria
            {
                Take = int.MaxValue
            });

            return options.Results.ToList();
        }

        /// <summary>
        /// Calculate the original images
        /// </summary>
        /// <param name="source"></param>
        /// <param name="suffixCollection"></param>
        /// <returns></returns>
        protected virtual ICollection<BlobEntry> GetOriginalItems(ICollection<BlobEntry> source, ICollection<string> suffixCollection)
        {
            var result = new List<BlobEntry>();

            foreach (var blobInfo in source)
            {
                var name = blobInfo.Name;

                if (!suffixCollection.Any(suffix => name.Contains("_" + suffix)))
                {
                    result.Add(blobInfo);
                }
            }

            return result;
        }
    }
}
