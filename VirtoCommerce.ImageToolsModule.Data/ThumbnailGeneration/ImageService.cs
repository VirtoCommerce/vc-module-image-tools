using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
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
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        /// <summary>
        /// Save given image to blob storage.
        /// </summary>
        /// <param name="imageUrl">Image url.</param>
        /// <param name="image">Image object.</param>
        /// <param name="format">Image object format.</param>
        public virtual async Task SaveImage(string imageUrl, Image image, ImageFormat format)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            // Create an Encoder object based on the GUID  
            // for the Quality parameter category.  
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;
            // Create an EncoderParameters object.  
            // An EncoderParameters object has an array of EncoderParameter  
            // objects. In this case, there is only one  
            // EncoderParameter object in the array.  
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 87L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            using (var blobStream = _storageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, jpgEncoder, myEncoderParameters);
                stream.Position = 0;
                await stream.CopyToAsync(blobStream);
            }
        }

        /// <summary>
        /// Get image format by Image object.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public virtual ImageFormat GetImageFormat(Image image)
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

        #endregion
    }
}
