using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Generates thumbnails by certain criteria
    /// </summary>
    public class DefaultThumbnailGenerator : IThumbnailGenerator
    {
        private readonly object _progressLock = new object();

        private readonly IBlobStorageProvider _storageProvider;
        private readonly IImageResizer _imageResizer;

        public DefaultThumbnailGenerator(IBlobStorageProvider storageProvider, IImageResizer imageResizer)
        {
            _storageProvider = storageProvider;
            _imageResizer = imageResizer;
        }

        /// <summary>
        /// Generates thumbnails asynchronously
        /// </summary>
        /// <param name="sourcePath">Path to source picture</param>
        /// <param name="destPath">Target for generated thumbnail</param>
        /// <param name="option">Represents generation options</param>
        /// <param name="token">Allows cancel operation</param>
        /// <returns></returns>
        public ThumbnailGenerationResult GenerateThumbnailsAsync(string sourcePath, string destPath, ThumbnailOption option, ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();

            var originalImage = LoadImageAsync(sourcePath);
            if (originalImage == null)
            {
                return new ThumbnailGenerationResult()
                {
                    Errors = {$"Cannot generate thumbnail for option {option.Name}: {sourcePath} does not have a valid image format" }
                };
            }

            var format = GetImageFormat(originalImage);

            //one process only can use an Image object at the same time.
            Image clone;
            lock (_progressLock)
            {
                clone = (Image)originalImage.Clone();
            }

            //Generate a Thumbnail
            var height = option.Height ?? originalImage.Height;
            var width = option.Width ?? originalImage.Width;
            var color = ColorTranslator.FromHtml(option.BackgroundColor);

            Image thumbnail = null;
            switch (option.ResizeMethod)
            {
                case ResizeMethod.FixedSize:
                    thumbnail = _imageResizer.FixedSize(clone, width, height, color);
                    break;
                case ResizeMethod.FixedWidth:
                    thumbnail = _imageResizer.FixedWidth(clone, width, color);
                    break;
                case ResizeMethod.FixedHeight:
                    thumbnail = _imageResizer.FixedHeight(clone, height, color);
                    break;
                case ResizeMethod.Crop:
                    thumbnail = _imageResizer.Crop(clone, width, height, option.AnchorPosition);
                    break;
            }

            if (thumbnail != null)
            {
                SaveImage(destPath, thumbnail, format);
            }
            else
            {
                throw new Exception($"Cannot save thumbnail image {destPath}");
                //string.Format(CultureInfo.InvariantCulture, "Cannot generate thumbnail for image '{0}'.", thumbnailUrl)
            }

            return new ThumbnailGenerationResult
            {
                GeneratedThumbnails = {destPath}
            };
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        protected virtual Image LoadImageAsync(string imageUrl)
        {
            try
            {
                using (var blobStream = _storageProvider.OpenRead(imageUrl))
                using (var stream = new MemoryStream())
                {
                    blobStream.CopyTo(stream);
                    var result = Image.FromStream(stream);
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
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

        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        protected virtual void SaveImage(string imageUrl, Image image, ImageFormat format)
        {
            using (var blobStream = _storageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                stream.Position = 0;
                stream.CopyTo(blobStream);
            }
        }
    }
}

