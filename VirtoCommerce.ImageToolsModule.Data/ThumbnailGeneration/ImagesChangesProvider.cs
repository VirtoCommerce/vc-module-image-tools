using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class BlobImagesChangesProvider : IImagesChangesProvider
    {
        public bool IsTotalCountSupported => true;

        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;

        private IList<ImageChange> _changeBlobs;

        public BlobImagesChangesProvider(IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService)
        {
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
        }

        protected virtual IList<ImageChange> GetChangeFiles(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
        {
            var options = GetOptionsCollection();
            var allBlobInfos = ReadBlobFolder(task.WorkPath, token);
            var orignalBlobInfos = GetOriginalItems(allBlobInfos, options.Select(x => x.FileSuffix).ToList());

            var result = new List<ImageChange>();
            foreach (var blobInfo in orignalBlobInfos)
            {
                token?.ThrowIfCancellationRequested();

                var imageChange = new ImageChange
                {
                    Name = blobInfo.FileName,
                    Url = blobInfo.Url,
                    ModifiedDate = blobInfo.ModifiedDate,
                    ChangeState = !changedSince.HasValue ? EntryState.Added : GetItemState(blobInfo, changedSince, task.ThumbnailOptions)
                };
                result.Add(imageChange);
            }
            return result.Where(x=>x.ChangeState != EntryState.Unchanged).ToList();
        }

        #region Implementation of IImagesChangesProvider

        public long GetTotalChangesCount(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles(task, changedSince, token);
            }
            return _changeBlobs.Count;
        }

        public ImageChange[] GetNextChangesBatch(ThumbnailTask task, DateTime? changedSince, long? skip, long? take, ICancellationToken token)
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles(task, changedSince, token);
            }

            var count = _changeBlobs.Count;

            if (skip >= count)
            {
                return new ImageChange[] {};
            }

            return _changeBlobs.Skip((int)skip).Take((int)take).ToArray();
        }

        #endregion

        protected virtual ICollection<BlobInfo> ReadBlobFolder(string folderPath, ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();
            
            var result = new List<BlobInfo>();

            var searchResults = _storageProvider.Search(folderPath, null);

            result.AddRange(searchResults.Items);
            foreach (var blobFolder in searchResults.Folders)
            {
                var folderResult = ReadBlobFolder(blobFolder.RelativeUrl, token);
                result.AddRange(folderResult);
            }

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
        protected virtual bool Exists(string imageUrl)
        {
            var blobInfo = _storageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
        }

        protected virtual EntryState GetItemState(BlobInfo blobInfo, DateTime? changedSince, IList<ThumbnailOption> options)
        {
            if (!changedSince.HasValue)
            {
                return EntryState.Added;
            }

            foreach (var option in options)
            {
                if (!Exists(blobInfo.Url.GenerateThumbnailName(option.FileSuffix)))
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
        protected virtual ICollection<ThumbnailOption> GetOptionsCollection()
        {
            var options = _thumbnailOptionSearchService.Search(new ThumbnailOptionSearchCriteria()
            {
                Take = Int32.MaxValue
            });

            return options.Results.ToList();
        }

        /// <summary>
        /// Calculate the original images
        /// </summary>
        /// <param name="source"></param>
        /// <param name="suffixCollection"></param>
        /// <returns></returns>
        protected virtual ICollection<BlobInfo> GetOriginalItems(ICollection<BlobInfo> source, ICollection<string> suffixCollection)
        {
            var result = new List<BlobInfo>();

            foreach (var blobInfo in source)
            {
                var present = false;
                foreach (var suffix in suffixCollection)
                {
                    var name = blobInfo.FileName;
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
