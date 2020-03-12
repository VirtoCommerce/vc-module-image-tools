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
	public class BlobImagesChangesProvider : IImagesChangesProvider
	{
		public bool IsTotalCountSupported => true;

		private readonly IBlobStorageProvider _storageProvider;
		private readonly IThumbnailOptionSearchService _thumbnailOptionSearchService;

		private readonly Dictionary<string, ICollection<BlobInfo>> _blobImageInfosCache = new Dictionary<string, ICollection<BlobInfo>>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, IList<ImageChange>> _blobChangesCache = new Dictionary<string, IList<ImageChange>>(StringComparer.InvariantCultureIgnoreCase);


		private readonly string[] _imageExtensions = { ".bmp", ".gif", ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi", ".png", ".tiff", ".tif" };
		private ICollection<ThumbnailOption> _availableOptions;

		public BlobImagesChangesProvider(IBlobStorageProvider storageProvider, IThumbnailOptionSearchService thumbnailOptionSearchService)
		{
			_storageProvider = storageProvider;
			_thumbnailOptionSearchService = thumbnailOptionSearchService;
		}

		protected virtual IList<ImageChange> ScanBlobForChanges(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
		{
			var options = GetOptionsCollection();
			var allBlobInfos = GetBlobImages(task.WorkPath, token);
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
			return result.Where(x => x.ChangeState != EntryState.Unchanged).ToList();
		}

		#region Implementation of IImagesChangesProvider

		public long GetTotalChangesCount(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
		{
			var changedBlobs = GetChangedBlobs(task, changedSince, token);

			return changedBlobs.Count;
		}

		public ImageChange[] GetNextChangesBatch(ThumbnailTask task, DateTime? changedSince, long? skip, long? take, ICancellationToken token)
		{
			var changedBlobs = GetChangedBlobs(task, changedSince, token);

			var count = changedBlobs.Count;

			if (skip >= count)
			{
				return Array.Empty<ImageChange>();
			}

			return changedBlobs.Skip((int)skip).Take((int)take).ToArray();
		}

		#endregion

		protected virtual ICollection<BlobInfo> ReadBlobFolder(string folderPath, ICancellationToken token)
		{
			token?.ThrowIfCancellationRequested();

			var result = new List<BlobInfo>();

			var searchResults = _storageProvider.Search(folderPath, null);
			searchResults.Items = searchResults.Items.Where(item => _imageExtensions.Contains(Path.GetExtension(item.FileName))).ToList();

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
			if (_availableOptions == null)
			{
				var options = _thumbnailOptionSearchService.Search(new ThumbnailOptionSearchCriteria()
				{
					Take = Int32.MaxValue
				});

				_availableOptions = options.Results.ToList();
			}

			return _availableOptions;
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
				var name = blobInfo.FileName;

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

		/// <summary>
		/// Gets changed blobs. Caches changes based on location path and task suffixes.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="changedSince"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		protected virtual IList<ImageChange> GetChangedBlobs(ThumbnailTask task, DateTime? changedSince, ICancellationToken token)
		{
			var cacheKey = $"{task.WorkPath}:{string.Join(":", task.ThumbnailOptions.Select(x => x.FileSuffix))}";

			if (!_blobChangesCache.ContainsKey(cacheKey))
			{
				_blobChangesCache.Add(cacheKey, ScanBlobForChanges(task, changedSince, token));
			}

			return _blobChangesCache[cacheKey];
		}

		/// <summary>
		/// Gets image blob infos from blob path. Caches results based on location path.
		/// </summary>
		/// <param name="blobPath"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		protected virtual ICollection<BlobInfo> GetBlobImages(string blobPath, ICancellationToken token)
		{
			if (!_blobImageInfosCache.ContainsKey(blobPath))
			{
				_blobImageInfosCache.Add(blobPath, ReadBlobFolder(blobPath, token));
			}

			return _blobImageInfosCache[blobPath];
		}
	}
}
