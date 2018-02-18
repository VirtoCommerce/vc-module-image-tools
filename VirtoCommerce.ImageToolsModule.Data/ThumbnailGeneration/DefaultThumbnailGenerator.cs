using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
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
        /// <param name="options">Represents generation options</param>
        /// <param name="token">Allows cancel operation</param>
        /// <returns></returns>
        public async Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(string sourcePath, string destPath, IList<ThumbnailOption> options, ICancellationToken token)
        {
            token?.ThrowIfCancellationRequested();

            var originalImage = await LoadImageAsync(sourcePath);
            if (originalImage == null)
            {
                return new ThumbnailGenerationResult()
                {
                    Errors = {$"Cannot generate thumbnail: {sourcePath} does not have a valid image format" }
                };
            }

            var result = new ThumbnailGenerationResult();

            var format = GetImageFormat(originalImage);

            //one process only can use an Image object at the same time.
            Image clone;
            lock (_progressLock)
            {
                clone = (Image)originalImage.Clone();
            }

            foreach (var option in options)
            {
                var thumbnail = GenerateThumbnail(clone, option);
                var thumbnailUrl = sourcePath.GenerateThumnnailName(option.FileSuffix);

                if (thumbnail != null)
                {
                    SaveImage(thumbnailUrl, thumbnail, format);
                }
                else
                {
                    throw new Exception($"Cannot save thumbnail image {destPath}");
                }

                result.GeneratedThumbnails.Add(thumbnailUrl);
            }

            return result;
        }

        /// <summary>
        ///Generates a Thumbnail
        /// </summary>
        /// <param name="image"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private Image GenerateThumbnail(Image image, ThumbnailOption option)
        {
            var height = option.Height ?? image.Height;
            var width = option.Width ?? image.Width;
            var color = ColorTranslator.FromHtml(option.BackgroundColor);

            Image thumbnail = null;
            switch (option.ResizeMethod)
            {
                case ResizeMethod.FixedSize:
                    thumbnail = _imageResizer.FixedSize(image, width, height, color);
                    break;
                case ResizeMethod.FixedWidth:
                    thumbnail = _imageResizer.FixedWidth(image, width, color);
                    break;
                case ResizeMethod.FixedHeight:
                    thumbnail = _imageResizer.FixedHeight(image, height, color);
                    break;
                case ResizeMethod.Crop:
                    thumbnail = _imageResizer.Crop(image, width, height, option.AnchorPosition);
                    break;
            }
            return thumbnail;
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        protected virtual async Task<Image> LoadImageAsync(string imageUrl)
        {
            try
            {
                using (var blobStream = _storageProvider.OpenRead(imageUrl))
                using (var stream = new MemoryStream())
                {
                    await blobStream.CopyToAsync(stream);
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

