using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using VirtoCommerce.ImageToolsModule.Web.Exceptions;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.ImageToolsModule.Web.Services
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
        protected readonly IImageResize _imageResize;
        private object progressLock = new object();
        
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailService(IBlobStorageProvider blobStorageProvider, IImageResize imageResize)
        {
            _blobStorageProvider = blobStorageProvider;
            _imageResize = imageResize;
        }

        /// <summary>
        /// Generate different thumbnails by given image url.
        /// </summary>
        /// <param name="imageUrl">Original image.</param>
        /// <param name="thumbnailsParameters">ImageTools.ImageSizes settings.</param>
        /// <param name="isRegenerateAll">True to replace all existed thumbnails with a new ones.</param>
        public async Task<bool> GenerateAsync(string imageUrl, string[] thumbnailsParameters, bool isRegenerateAll)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");
            if (thumbnailsParameters == null)
                throw new ArgumentNullException("thumbnailsParameters");

            var thumbnailsSettings = thumbnailsParameters.Select(x => JsonConvert.DeserializeObject<ThumbnailParameters>(x)).ToArray();
            if (!thumbnailsSettings.Any())
                throw new ThumbnailsParametersException("None ore wrong thumbnails parameters");


            var originalImage = await LoadImageAsync(imageUrl);
            var format = GetImageFormat(originalImage);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = GetMaxDegreeOfParallelism() };

            //foreach (var specifications in resizeImageSpecifications)
            Parallel.ForEach(thumbnailsSettings, parallelOptions, (thumbnailSettings) =>
            {
                var thumbnailUrl = AddAliasToImageUrl(imageUrl, "_" + thumbnailSettings.Alias);
                if (isRegenerateAll || !IsExists(thumbnailUrl))
                {
                    //one process could use Image object at the same time.
                    Bitmap clone = null;
                    lock (progressLock)
                    {
                        clone = (Bitmap)originalImage.Clone();
                    }
                    //Generate a Thumbnail
                    var thumbnail = _imageResize.FixedSize(clone, thumbnailSettings.Width, thumbnailSettings.Height, thumbnailSettings.Color);
                    //Save
                    SaveImage(thumbnailUrl, thumbnail, format);
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

        #endregion

    }
}