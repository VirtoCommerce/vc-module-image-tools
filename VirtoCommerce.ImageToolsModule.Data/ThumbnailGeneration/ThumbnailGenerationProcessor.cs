using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ThumbnailGenerationProcessor : IThumbnailGenerationProcessor
    {
        private readonly IThumbnailGenerator _generator;
        private readonly IBlobStorageProvider _storageProvider;
        private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;
        private readonly IThumbnailTaskService _thumbnailTaskService;

        public ThumbnailGenerationProcessor(IThumbnailGenerator generator, IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService, IThumbnailTaskService thumbnailTaskService)
        {
            _generator = generator;
            _storageProvider = storageProvider;
            _thumbnailOptionSearchService = thumbnailOptionSearchService;
            _thumbnailTaskService = thumbnailTaskService;
        }

        public void ProcessTasksAsync(string[] taskIds, bool regenerate, Action<ThumbnailTaskProgress> progressCallback, ICancellationToken token)
        {
            //find original files and count
            var progressInfo = new ThumbnailTaskProgress { Message = "Reading the tasks..." };

            var tasks = _thumbnailTaskService.GetByIds(taskIds);

            var suffixCollection = GetSuffixCollection();

            var lookup = new Dictionary<ThumbnailTask, ICollection<BlobInfo>>();
            foreach (var task in tasks)
            {
                var searchResult = ReadBlobFolder(task.WorkPath);
                var itemsToGenerateFrom = GetOriginalItems(searchResult, suffixCollection);
                lookup.Add(task, itemsToGenerateFrom);
                progressInfo.TotalCount += itemsToGenerateFrom.Count;
            }

            progressCallback(progressInfo);

            foreach (var task in lookup.Keys)
            {
                progressInfo.Message = $"Starting task {task.Name}...";
                progressCallback(progressInfo);

                foreach (var fileInfo in lookup[task])
                {
                    foreach (var option in task.ThumbnailOptions)
                    {
                        //skip non existent images
                        var thumbnailUrl = AddAliasToImageUrl(fileInfo.Url, "_" + option.FileSuffix);
                        if (!regenerate && Exists(thumbnailUrl))
                        {
                            continue;
                        }

                        progressInfo.Message = $"Generating thumbnails for {fileInfo.FileName}...";
                        progressCallback(progressInfo);

                        var result = _generator.GenerateThumbnailsAsync(fileInfo.Url, thumbnailUrl, option, token);
                        if (result.Errors.IsNullOrEmpty())
                        {
                            continue;
                        }

                        progressInfo.Errors.AddRange(result.Errors);
                        progressCallback(progressInfo);
                    }

                    progressInfo.ProcessedCount++;;
                    progressCallback(progressInfo);
                }
            }

            progressInfo.Message = "Finished generating thumbnails!";
            progressCallback(progressInfo);
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

        //get all options to create a map of all potential file names
        protected virtual ICollection<string> GetSuffixCollection()
        {
            var options = _thumbnailOptionSearchService.Search(new ThumbnailOptionSearchCriteria()
            {
                Take = Int32.MaxValue
            });

            return options.Results.Select(x => x.FileSuffix).ToList();
        }

        /// <summary>
        /// Add suffix to url.
        /// </summary>
        /// <param name="originalImageUrl"> original image url.</param>
        /// <param name="suffix">suffix.</param>
        /// <returns>Url with suffix.</returns>
        protected virtual string AddAliasToImageUrl(string originalImageUrl, string suffix)
        {
            var name = Path.GetFileNameWithoutExtension(originalImageUrl);
            var extention = Path.GetExtension(originalImageUrl);
            var newName = string.Concat(name, suffix, extention);

            var uri = new Uri(originalImageUrl);
            var uriWithoutLastSegment = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);

            var result = new Uri(new Uri(uriWithoutLastSegment), newName);

            return result.AbsoluteUri;
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

        protected ICollection<BlobInfo> ReadBlobFolder(string folderPath)
        {
            var result = new  List<BlobInfo>();

            var searchResults = _storageProvider.Search(folderPath, null);

            result.AddRange(searchResults.Items);
            foreach (var blobFolder in searchResults.Folders)
            {
                var folderResult = ReadBlobFolder(blobFolder.RelativeUrl);
                result.AddRange(folderResult);
            }

            return result;
        }
    }
}