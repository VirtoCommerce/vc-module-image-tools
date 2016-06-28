using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using VirtoCommerce.ImageToolsModule.Data.Exceptions;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Core.Assets;
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
        protected readonly IBlobStorageProvider _blobStorageProvider;
        protected readonly IImageResizer _imageResize;
        protected readonly ISettingsManager _settingsManager;
        private const string settingsName = "ImageTools.Thumbnails.Parameters";
        private object progressLock = new object();


        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailService(IBlobStorageProvider blobStorageProvider, IImageResizer imageResize, ISettingsManager settingsManager)
        {
            _blobStorageProvider = blobStorageProvider;
            _imageResize = imageResize;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Generate different thumbnails by given image url.
        /// </summary>
        /// <param name="imageUrl">Original image.</param>
        /// <param name="thumbnailsParameters">ImageTools.ImageSizes settings.</param>
        /// <param name="isRegenerateAll">True to replace all existed thumbnails with a new ones.</param>
        public async Task<bool> GenerateAsync(string imageUrl, bool isRegenerateAll)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");

            var thumbnailsParameters = GetThumbnailParameters();
            if (thumbnailsParameters == null)
                throw new ArgumentNullException("thumbnailsParameters");
            if (!thumbnailsParameters.Any())
                throw new ThumbnailsParametersException("None ore wrong thumbnails parameters");


            var originalImage = await LoadImageAsync(imageUrl);
            var format = GetImageFormat(originalImage);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = GetMaxDegreeOfParallelism() };

            Parallel.ForEach(thumbnailsParameters, parallelOptions, (parameters) =>
            {
                var thumbnailUrl = AddAliasToImageUrl(imageUrl, "_" + parameters.Alias);
                if (isRegenerateAll || !IsExists(thumbnailUrl))
                {
                        //one process only can use an Image object at the same time.
                        Image clone = null;
                        lock (progressLock)
                        {
                            clone = (Image)originalImage.Clone();
                        }
                        
                        //Generate a Thumbnail
                        Image thumbnail = null;
                        switch (parameters.Method)
                        {
                            case ResizeType.FixedSize:
                                thumbnail = _imageResize.FixedSize(clone, parameters.Width, parameters.Height, parameters.Color);
                                break;
                            case ResizeType.FixedWidth:
                                thumbnail = _imageResize.FixedWidth(clone, parameters.Width, parameters.Color);
                                break;
                            case ResizeType.FixedHeight:
                                thumbnail = _imageResize.FixedWidth(clone, parameters.Height, parameters.Color);
                                break;
                            case ResizeType.Cut:
                                thumbnail = _imageResize.Cut(clone, parameters.Width, parameters.Height, parameters.AnchorPosition);
                                break;
                        }

                        //Save
                        if (thumbnail != null)
                        {
                            SaveImage(thumbnailUrl, thumbnail, format);
                        }
                        else
                        {
                            throw new ThumbnailGenetationException(string.Format("Creation error of {0}, type", thumbnailUrl));
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
        public string[] GetThumbnails(string imageUrl, string[] aliases)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");
            if (aliases == null)
                throw new ArgumentNullException("settings");

            var thumbnailUrls = aliases
                .Select(x => AddAliasToImageUrl(imageUrl, "_" + x))
                .Where(x => IsExists(x))
                .ToArray();

            return thumbnailUrls;
        }

        #region Private methods

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        private async Task<Image> LoadImageAsync(string imageUrl)
        {
            using (var blobStream = _blobStorageProvider.OpenRead(imageUrl))
            using (var stream = new MemoryStream())
            {
                await blobStream.CopyToAsync(stream);
                var result = Bitmap.FromStream(stream);
                return result;
            }
        }

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        private void SaveImage(string imageUrl, Image image, ImageFormat format)
        {
            using (var blobStream = _blobStorageProvider.OpenWrite(imageUrl))
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
        private bool IsExists(string imageUrl)
        {
            var blobInfo = _blobStorageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
        }

        /// <summary>
        /// Add suffix to url.
        /// </summary>
        /// <param name="originalImageUrl"> original image url.</param>
        /// <param name="suffix">suffix.</param>
        /// <returns>Url with suffix.</returns>
        private string AddAliasToImageUrl(string originalImageUrl, string suffix)
        {
            var name = Path.GetFileNameWithoutExtension(originalImageUrl);
            var extention = Path.GetExtension(originalImageUrl);
            var newName = string.Concat(name, suffix, extention);

            var uri = new Uri(originalImageUrl);
            string uriWithoutLastSegment = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);

            var result = new Uri(new Uri(uriWithoutLastSegment), newName);

            return result.AbsoluteUri;
        }

        /// <summary>
        /// Get image format by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private ImageFormat GetImageFormat(Image image)
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
            else
                return ImageFormat.Wmf;
        }

        private int GetMaxDegreeOfParallelism()
        {
            int result = 4;
            try
            {
                var setting = WebConfigurationManager.AppSettings["MaxDegreeOfParallelism"];
                int settingValue = 0;
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

        private IEnumerable<ThumbnailParameters> GetThumbnailParameters()
        {
            var settings = _settingsManager.GetArray<string>(settingsName, null);
            var result = settings.Select(x => JsonConvert.DeserializeObject<ThumbnailParameters>(x));
            return result;
        }

        #endregion

    }
}