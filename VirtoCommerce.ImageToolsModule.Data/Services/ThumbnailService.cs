using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.ImageToolsModule.Data.Exceptions;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Thumbnail of an image is an image with another size(resolution). 
    /// The thumbnails size is less of original size.
    /// Thumbnails are using in an interface, where it doesn't need high resolution.
    /// For example, in listings, short views.
    /// The service allows to generate different thumbnails, get list of existed thumbnails.
    /// </summary>
    public class ThumbnailService : IThumbnailService
    {
        private const string _settingsName = "ImageTools.Thumbnails.Parameters";
        private readonly object _progressLock = new object();

        protected IBlobStorageProvider BlobStorageProvider { get; private set; }
        protected IImageResizer ImageResizer { get; private set; }
        protected ISettingsManager SettingsManager { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailService(IBlobStorageProvider blobStorageProvider, IImageResizer imageResizer, ISettingsManager settingsManager)
        {
            BlobStorageProvider = blobStorageProvider;
            ImageResizer = imageResizer;
            SettingsManager = settingsManager;
        }

        /// <summary>
        /// Generate different thumbnails by given image url.
        /// </summary>
        /// <param name="imageUrl">Original image.</param>
        /// <param name="isRegenerateAll">True to replace all existed thumbnails with a new ones.</param>
        public virtual async Task<bool> GenerateAsync(string imageUrl, bool isRegenerateAll)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");

            var thumbnailsParameters = GetThumbnailParameters();
            if (thumbnailsParameters.IsNullOrEmpty())
                return false;

            var originalImage = await LoadImageAsync(imageUrl);
            var format = GetImageFormat(originalImage);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = GetMaxDegreeOfParallelism() };

            Parallel.ForEach(thumbnailsParameters, parallelOptions, (parameters) =>
            {
                var thumbnailUrl = AddAliasToImageUrl(imageUrl, "_" + parameters.Alias);
                if (isRegenerateAll || !Exists(thumbnailUrl))
                {
                    //one process only can use an Image object at the same time.
                    Image clone;
                    lock (_progressLock)
                    {
                        clone = (Image)originalImage.Clone();
                    }

                    //Generate a Thumbnail
                    Image thumbnail = null;
                    switch (parameters.Method)
                    {
                        case ResizeType.FixedSize:
                            thumbnail = ImageResizer.FixedSize(clone, parameters.Width, parameters.Height, parameters.Color);
                            break;
                        case ResizeType.FixedWidth:
                            thumbnail = ImageResizer.FixedWidth(clone, parameters.Width, parameters.Color);
                            break;
                        case ResizeType.FixedHeight:
                            thumbnail = ImageResizer.FixedHeight(clone, parameters.Height, parameters.Color);
                            break;
                        case ResizeType.Crop:
                            thumbnail = ImageResizer.Crop(clone, parameters.Width, parameters.Height, parameters.AnchorPosition);
                            break;
                    }

                    //Save
                    if (thumbnail != null)
                    {
                        SaveImage(thumbnailUrl, thumbnail, format);
                    }
                    else
                    {
                        throw new ThumbnailGenetationException(string.Format(CultureInfo.InvariantCulture, "Cannot generate thumbnail for image '{0}'.", thumbnailUrl));
                    }
                }
            });

            return true;
        }

        /// <summary>
        /// Get all existed thumbnails urls of given image.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="aliases">Thumbnails aliases (suffixes).</param>
        /// <returns>List of existed thumbnails.</returns>
        public virtual string[] GetThumbnails(string imageUrl, string[] aliases)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");
            if (aliases == null)
                throw new ArgumentNullException("aliases");

            var thumbnailUrls = aliases
                .Select(x => AddAliasToImageUrl(imageUrl, "_" + x))
                .Where(Exists)
                .ToArray();

            return thumbnailUrls;
        }

        #region Protected methods

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        protected virtual async Task<Image> LoadImageAsync(string imageUrl)
        {
            using (var blobStream = BlobStorageProvider.OpenRead(imageUrl))
            using (var stream = new MemoryStream())
            {
                await blobStream.CopyToAsync(stream);
                var result = Image.FromStream(stream);
                return result;
            }
        }

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        protected virtual void SaveImage(string imageUrl, Image image, ImageFormat format)
        {
            using (var blobStream = BlobStorageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                stream.Position = 0;
                stream.CopyTo(blobStream);
            }
        }

        /// <summary>
        /// Check if image is exist in blob storage by url.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <returns>True if image exist.</returns>
        protected virtual bool Exists(string imageUrl)
        {
            var blobInfo = BlobStorageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
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
        /// Get image format by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        protected virtual ImageFormat GetImageFormat(Image image)
        {
            if (image.RawFormat.Equals(ImageFormat.Jpeg))
                return ImageFormat.Jpeg;
            if (image.RawFormat.Equals(ImageFormat.Bmp))
                return ImageFormat.Bmp;
            if (image.RawFormat.Equals(ImageFormat.Png))
                return ImageFormat.Png;
            if (image.RawFormat.Equals(ImageFormat.Emf))
                return ImageFormat.Emf;
            if (image.RawFormat.Equals(ImageFormat.Exif))
                return ImageFormat.Exif;
            if (image.RawFormat.Equals(ImageFormat.Gif))
                return ImageFormat.Gif;
            if (image.RawFormat.Equals(ImageFormat.Icon))
                return ImageFormat.Icon;
            if (image.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ImageFormat.MemoryBmp;
            if (image.RawFormat.Equals(ImageFormat.Tiff))
                return ImageFormat.Tiff;
            return ImageFormat.Wmf;
        }

        protected virtual int GetMaxDegreeOfParallelism()
        {
            var result = 4;
            try
            {
                var setting = ConfigurationManager.AppSettings["MaxDegreeOfParallelism"];
                int settingValue;
                if (!string.IsNullOrEmpty(setting) && int.TryParse(setting, out settingValue))
                {
                    result = settingValue;
                }
            }
            catch
            {
            }

            return result;
        }

        protected virtual IList<ThumbnailParameters> GetThumbnailParameters()
        {
            var setting = SettingsManager.GetSettingByName(_settingsName);
            if (setting == null)
                return new List<ThumbnailParameters>();

            var settings = setting.ArrayValues ?? new string[] { };
            var result = settings.Select(JsonConvert.DeserializeObject<ThumbnailParameters>).ToList();
            return result;
        }

        #endregion
    }
}
