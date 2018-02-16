using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class BlobImagesChangesProvider : IImagesChangesProvider
    {
        protected long Skip { get; set; }
        protected long Take { get; set; }

        private ICollection<ImageChange> _changeBlobs;
        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;

        private readonly ThumbnailTask _task;
        private readonly bool _regenerate;

        public BlobImagesChangesProvider(ThumbnailTask task, bool regenerage, IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService, ISettingsManager settingsManager)
        {
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
            
            _task = task;
            _regenerate = regenerage;

            Take = settingsManager.GetValue("ImageTools.Thumbnails.ProcessBacthSize", 50);
        }

        protected virtual ICollection<ImageChange> GetChangeFiles()
        {
            var options = GetOptionsCollection();
            var allBlobInfos = ReadBlobFolder(_task.WorkPath);
            var orignalBlobInfos = GetOriginalItems(allBlobInfos, options.Select(x => x.FileSuffix).ToList());

            var result = new List<ImageChange>();
            foreach (var blobInfo in orignalBlobInfos)
            {
                var imageChange = new ImageChange
                {
                    Name = blobInfo.FileName,
                    Url = blobInfo.Url,
                    ModifiedDate = blobInfo.ModifiedDate
                };

                foreach (var option in _task.ThumbnailOptions)
                {
                    if (_regenerate || !Exists(blobInfo.Url.GenerateThumnnailName(option.FileSuffix)))
                    {
                        imageChange.ThumbnailOptions.Add(option);
                    }
                }

                if (imageChange.ThumbnailOptions.Any())
                {
                    result.Add(imageChange);
                }
            }
            return result;
        }

        #region Implementation of IImagesChangesProvider

        public long GetTotalChangesCount()
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles();
            }
            return _changeBlobs.Count;
        }

        public ImageChangeResult GetNextChangesBatch()
        {
            if (_changeBlobs == null)
            {
                _changeBlobs = GetChangeFiles();
            }
            var count = _changeBlobs.Count;

            if (Skip >= count)
            {
                return null;
            }

            var page = _changeBlobs
                .Skip((int)Skip)
                .Take((int)Take)
                .ToList();

            Skip += page.Count;

            return new ImageChangeResult()
            {
                ThumbnailTask = _task,
                ImageChanges = page,
                TotalCount = count
            };
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
        /// <returns>True if image exist.</returns>
        protected virtual bool Exists(string imageUrl)
        {
            var blobInfo = _storageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
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
