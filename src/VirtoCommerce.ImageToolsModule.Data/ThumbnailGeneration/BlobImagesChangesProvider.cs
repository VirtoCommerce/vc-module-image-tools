using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;
        private static readonly string[] SupportedImageExtensions = { ".bmp", ".gif", ".jpg", ".jpeg", ".png" };

        public BlobImagesChangesProvider(IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService, IPlatformMemoryCache platformMemoryCache)
        {
            _platformMemoryCache = platformMemoryCache;
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
        }

        protected virtual async Task<IList<ImageChange>> GetChangeFiles(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
        {
            var options = await GetOptionsCollection();
            var cacheKey = CacheKey.With(GetType(), "GetChangeFiles", task.WorkPath, changedSince?.ToString(), string.Join(":", options.Select(x => x.FileSuffix)));
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(BlobChangesCacheRegion.CreateChangeToken(task, changedSince));

                var allBlobInfos = await ReadBlobFolderAsync(task.WorkPath, token);
                var orignalBlobInfos = GetOriginalItems(allBlobInfos.Values, options.Select(x => x.FileSuffix).ToList());

                var result = new ConcurrentBag<ImageChange>();
                await Parallel.ForEachAsync(orignalBlobInfos, (blobInfo, token) =>
                {
                    token.ThrowIfCancellationRequested();

                    var imageChange = new ImageChange
                    {
                        Name = blobInfo.Name,
                        Url = blobInfo.Url,
                        ModifiedDate = blobInfo.ModifiedDate,
                        ChangeState = !changedSince.HasValue ? EntryState.Added : GetItemState(blobInfo, changedSince, task.ThumbnailOptions, allBlobInfos)
                    };
                    result.Add(imageChange);

                    return ValueTask.CompletedTask;
                });

                return result.Where(x => x.ChangeState != EntryState.Unchanged).ToList();
            });
        }

        #region Implementation of IImagesChangesProvider

        public async Task<long> GetTotalChangesCount(ThumbnailTask task, DateTime? changedSince,
            ICancellationToken token)
        {
            var changedFiles = await GetChangeFiles(task, changedSince, token);

            return changedFiles.Count;
        }

        public async Task<ImageChange[]> GetNextChangesBatch(ThumbnailTask task, DateTime? changedSince, long? skip,
            long? take, ICancellationToken token)
        {

            var changedFiles = await GetChangeFiles(task, changedSince, token);

            var count = changedFiles.Count;

            if (skip >= count)
            {
                return Array.Empty<ImageChange>();
            }

            return changedFiles.Skip((int)(skip ?? 0)).Take((int)(take ?? 0)).ToArray();
        }

        #endregion

        protected virtual async Task<ConcurrentDictionary<string, BlobEntry>> ReadBlobFolderAsync(string folderPath, ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();

            var result = new ConcurrentDictionary<string, BlobEntry>();

            var searchResults = await _storageProvider.SearchAsync(folderPath, null);

            result.AddRange(searchResults.Results.Where(item => SupportedImageExtensions.Contains(Path.GetExtension(item.Name).ToLowerInvariant())).Select(x => KeyValuePair.Create(x.Url, x)));

            Parallel.ForEach(searchResults.Results.Where(x => x.Type == "folder"), blobFolder =>
            {
                var folderResult = ReadBlobFolderAsync(blobFolder.RelativeUrl, token).GetAwaiter().GetResult();
                
                result.AddRange(folderResult);
            });

            return result;
        }

        /// <summary>
        /// Check if image is exist in blob storage by url.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <returns>
        /// EntryState if image exist.
        /// Null is image is empty
        /// </returns>
        protected virtual async Task<bool> ExistsAsync(string imageUrl, ConcurrentDictionary<string, BlobEntry> earlyReadBlobInfos = null)
        {
            bool result;
            if (earlyReadBlobInfos == null)
            {
                result = null != await _storageProvider.GetBlobInfoAsync(imageUrl);
            }
            else
            {
                result = earlyReadBlobInfos.ContainsKey(imageUrl);
            }

            return result;
        }

        protected virtual EntryState GetItemState(BlobEntry blobInfo, DateTime? changedSince, IList<ThumbnailOption> options, ConcurrentDictionary<string, BlobEntry> earlyReadBlobInfos = null)
        {
            if (!changedSince.HasValue)
            {
                return EntryState.Added;
            }

            foreach (var option in options)
            {
                if (!ExistsAsync(blobInfo.Url.GenerateThumbnailName(option.FileSuffix), earlyReadBlobInfos).GetAwaiter().GetResult())
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

        //get all options to create a map of all potential file names
        protected virtual async Task<ICollection<ThumbnailOption>> GetOptionsCollection()
        {
            var options = await _thumbnailOptionSearchService.SearchAsync(new ThumbnailOptionSearchCriteria()
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

                var present = false;
                foreach (var suffix in suffixCollection)
                {
                    if (name.Contains("_" + suffix))
                    {
                        present = true;
                        break;
                    }
                }

                if (!present)
                {
                    result.Add(blobInfo);
                }
            }

            return result;
        }
    }
}
