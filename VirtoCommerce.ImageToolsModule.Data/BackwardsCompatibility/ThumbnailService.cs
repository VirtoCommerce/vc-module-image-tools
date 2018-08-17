using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Exceptions;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Thumbnail of an image is an image with another size(resolution). 
    /// The thumbnails size is less of original size.
    /// Thumbnails are using in an interface, where it doesn't need high resolution.
    /// For example, in listings, short views.
    /// The service allows to generate different thumbnails, get list of existed thumbnails.
    /// </summary>
#pragma warning disable 612, 618
    [Obsolete("Obsolete, only for backwards compatibility", false)]
#pragma warning restore 612, 618
    public class ThumbnailService : IThumbnailService
    {
        protected class ThumbnailParameters
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public ResizeMethod Method { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public Color Color { get; set; }
            public string Alias { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public AnchorPosition AnchorPosition { get; set; }
            public JpegQuality JpegQuality { get; set; }
        }

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

        public virtual async Task<bool> GenerateAsync(string imageUrl, bool isRegenerateAll)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");

            var thumbnailsParameters = GetThumbnailParameters();
            if (thumbnailsParameters.IsNullOrEmpty())
                return false;

            var originalImage = await LoadImageAsync(imageUrl);
            var codecInfo = GetImageCodecInfo(originalImage);
            foreach (var parameters in thumbnailsParameters)
            {
                var thumbnailUrl = AddAliasToImageUrl(imageUrl, parameters.Alias);
                if (isRegenerateAll || !Exists(thumbnailUrl))
                {
                    //one process only can use an Image object at the same time.
                    Image clone;
                    lock (_progressLock)
                    {
                        clone = (Image)originalImage.Clone();
                    }

                    // Get encoder parameters.
                    var encoderParams = GetEncoderParameters(originalImage, parameters);

                    //Generate a Thumbnail
                    Image thumbnail = null;
                    switch (parameters.Method)
                    {
                        case ResizeMethod.FixedSize:
                            thumbnail = ImageResizer.FixedSize(clone, parameters.Width, parameters.Height, parameters.Color);
                            break;
                        case ResizeMethod.FixedWidth:
                            thumbnail = ImageResizer.FixedWidth(clone, parameters.Width, parameters.Color);
                            break;
                        case ResizeMethod.FixedHeight:
                            thumbnail = ImageResizer.FixedHeight(clone, parameters.Height, parameters.Color);
                            break;
                        case ResizeMethod.Crop:
                            thumbnail = ImageResizer.Crop(clone, parameters.Width, parameters.Height, parameters.AnchorPosition);
                            break;
                    }

                    //Save
                    if (thumbnail != null)
                    {
                        await SaveImage(thumbnailUrl, thumbnail, codecInfo, encoderParams);
                    }
                    else
                    {
                        throw new ThumbnailGenetationException(string.Format(CultureInfo.InvariantCulture, "Cannot generate thumbnail for image '{0}'.", thumbnailUrl));
                    }
                }
            }
            return true;
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

        public string[] GetThumbnails(string imageUrl, string[] aliases)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");
            if (aliases == null)
                throw new ArgumentNullException("aliases");

            var thumbnailUrls = aliases
                .Select(imageUrl.GenerateThumbnailName)
                .Where(Exists)
                .ToArray();

            return thumbnailUrls;
        }

        protected virtual bool Exists(string imageUrl)
        {
            var blobInfo = BlobStorageProvider.GetBlobInfo(imageUrl);
            return blobInfo != null;
        }

        protected virtual string AddAliasToImageUrl(string originalImageUrl, string suffix)
        {
            return originalImageUrl.GenerateThumbnailName(suffix);
        }

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

        protected virtual async Task SaveImage(string imageUrl, Image image, ImageCodecInfo codecInfo, EncoderParameters encoderParams = null)
        {
            using (var blobStream = BlobStorageProvider.OpenWrite(imageUrl))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, codecInfo, encoderParams);
                stream.Position = 0;
                await stream.CopyToAsync(blobStream);
            }
        }

        protected virtual ImageCodecInfo GetImageCodecInfo(Image image)
        {
            var format = image.RawFormat;
            var codec = new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders()).FirstOrDefault(c => c.FormatID == format.Guid);
            return codec;
        }

        protected virtual EncoderParameters GetEncoderParameters(Image image, ThumbnailParameters parameters)
        {
            EncoderParameters encoderParams = null;
            if (image.RawFormat == ImageFormat.Jpeg)
            {
                var myEncoder = Encoder.Quality;
                encoderParams = new EncoderParameters(1);
                var myEncoderParameter = new EncoderParameter(myEncoder, GetJpegQualityParameter(parameters.JpegQuality));
                encoderParams.Param[0] = myEncoderParameter;
            }

            return encoderParams;
        }

        private long GetJpegQualityParameter(JpegQuality quality)
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
    }
}
