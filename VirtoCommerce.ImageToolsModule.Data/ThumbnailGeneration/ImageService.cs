using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public class ImageService : IImageService
    {
        private readonly IBlobStorageProvider _storageProvider;

        #region Implementation of IImageService

        public ImageService(IBlobStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Load to Image from blob.
        /// </summary>
        /// <param name="imageUrl">image url.</param>
        /// <returns>Image object.</returns>
        public virtual async Task<Image> LoadImageAsync(string imageUrl)
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
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="imageFormat">ImageFormat object.</param>
        /// <param name="quality">JpegQuality object.</param>
        public virtual async Task SaveImage(string imageUrl, Image image, ImageFormat imageFormat, JpegQuality quality)
        {
            using (var blobStream = _storageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                if (imageFormat.Guid == ImageFormat.Jpeg.Guid)
                {
                    var codecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == imageFormat.Guid);
                    var encoderParams = new EncoderParameters
                    {
                        Param = new EncoderParameter[]
                        {
                           new EncoderParameter(Encoder.Quality, (int)quality)
                        }
                    };
                    image.Save(stream, codecInfo, encoderParams);
                }
                else
                {
                    image.Save(stream, imageFormat);
                }
                stream.Position = 0;
                await stream.CopyToAsync(blobStream);
            }
        }

        #endregion
    }
}