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
        private Dictionary<string, ImageChange> _changeBlobs = new Dictionary<string, ImageChange>();

        public bool GetTotalCountSupported => true;

        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;

        public BlobImagesChangesProvider(IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService)
        {
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
        }

        protected virtual ICollection<ImageChange> GetChangeFiles(string workPath, bool regenerate, DateTime? lastRunDate)
        {
            var options = GetOptionsCollection();
            var allBlobInfos = ReadBlobFolder(workPath);
            var orignalBlobInfos = GetOriginalItems(allBlobInfos, options.Select(x => x.FileSuffix).ToList());

            var result = new List<ImageChange>();
            foreach (var blobInfo in orignalBlobInfos)
            {
                var imageChange = new ImageChange
                {
                    Name = blobInfo.FileName,
                    Url = blobInfo.Url,
                    ModifiedDate = blobInfo.ModifiedDate,
                    ChangeState = regenerate ? EntryState.Modified : GetItemState(blobInfo, lastRunDate)
                };
                result.Add(imageChange);
            }
            return result.Where(x=>x.ChangeState != EntryState.Unchanged).ToList();
        }

        #region Implementation of IImagesChangesProvider

        public long GetTotalChangesCount(string workPath, bool regenerate, DateTime? lastRunDate)
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles(workPath, regenerate, lastRunDate).ToDictionary(x=>x.Url, x=>x);
            }
            return _changeBlobs.Count;
        }

        public ImageChange[] GetNextChangesBatch(string workPath, bool regenerate, DateTime? lastRunDate, long? skip, long? take)
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles(workPath, regenerate, lastRunDate).ToDictionary(x => x.Url, x => x);
            }

            var count = _changeBlobs.Count;

            if (skip >= count)
            {
                return new ImageChange[] {};
            }

            return _changeBlobs.Skip((int)skip).Take((int)take).Select(x=>x.Value).ToArray();
        }

        #endregion

        protected virtual ICollection<BlobInfo> ReadBlobFolder(string folderPath)
        {
            var result = new List<BlobInfo>();

            var searchResults = _storageProvider.Search(folderPath, null);

            result.AddRange(searchResults.Items);
            foreach (var blobFolder in searchResults.Folders)
            {
                var folderResult = ReadBlobFolder(blobFolder.RelativeUrl);
                result.AddRange(folderResult);
            }

            return result;
        }

        /// <summary>
        /// Check if image is exist in blob storage by url.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="lastRunDate">Image url.</param>
        /// <returns>
        /// EntryState if image exist.
        /// Null is image is empty
        /// </returns>
        protected virtual bool Exists(string imageUrl, DateTime? lastRunDate)
        {
            var blobInfo = _storageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
        }

        protected virtual EntryState GetItemState(BlobInfo blobInfo, DateTime? lastRunDate)
        {
            if (!lastRunDate.HasValue)
            {
                return EntryState.Added;
            }

            if (blobInfo.ModifiedDate.HasValue && blobInfo.ModifiedDate >= lastRunDate)
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
