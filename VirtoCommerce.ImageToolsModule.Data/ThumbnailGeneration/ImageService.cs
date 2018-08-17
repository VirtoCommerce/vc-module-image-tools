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
        /// <param name="codecInfo">Image codec info.</param>
        /// <param name="encoderParams">Image encoder parameters.</param>
        public virtual async Task SaveImage(string imageUrl, Image image, ImageCodecInfo codecInfo, EncoderParameters encoderParams = null)
        {
            using (var blobStream = _storageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, codecInfo, encoderParams);
                stream.Position = 0;
                await stream.CopyToAsync(blobStream);
            }
        }

        /// <summary>
        /// Get codec info by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public virtual ImageCodecInfo GetImageCodecInfo(Image image)
        {
            var format = image.RawFormat;
            var codec = new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders()).FirstOrDefault(c => c.FormatID == format.Guid);
            return codec;
        }

        /// <summary>
        /// Get encoder parameters by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public virtual EncoderParameters GetEncoderParameters(Image image, ThumbnailOption option)
        {
            EncoderParameters encoderParams = null;
            if (image.RawFormat.Equals(ImageFormat.Jpeg))
            {
                var myEncoder = Encoder.Quality;
                encoderParams = new EncoderParameters(1);
                var myEncoderParameter = new EncoderParameter(myEncoder, GetJpegQualityParameter(option.JpegQuality));
                encoderParams.Param[0] = myEncoderParameter;
            }

            return encoderParams;
        }

        public virtual long GetJpegQualityParameter(JpegQuality quality)
        {
            if (quality == JpegQuality.Low)
                return 50;
            if (quality == JpegQuality.Medium)
                return 72;
            if (quality == JpegQuality.High)
                return 85;
            if (quality == JpegQuality.VeryHigh)
                return 92;

            return -1;
        }

        #endregion
    }
}