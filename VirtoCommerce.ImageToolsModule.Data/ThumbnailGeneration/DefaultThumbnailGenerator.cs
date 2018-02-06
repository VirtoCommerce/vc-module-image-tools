using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    /// <summary>
    /// Generates thumbnails by certain criteria
    /// </summary>
    public class DefaultThumbnailGenerator : IThumbnailGenerator
    {
        private IBlobStorageProvider _storageProvider;

        public DefaultThumbnailGenerator(IBlobStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Generates thumbnails asynchronously
        /// </summary>
        /// <param name="sourcePath">Contains source pictures</param>
        /// <param name="destPath">Target folder for generated thumbnails</param>
        /// <param name="option">Represents generation options</param>
        /// <param name="token">Allows cancel operation</param>
        /// <returns></returns>
        public async Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
            string sourcePath,
            string destPath,
            ThumbnailOption option,
            CancellationToken token)
        {
            var files = Directory.GetFiles(sourcePath);

            return await Task<ThumbnailGenerationResult>.Factory.StartNew(
                       () =>
                           {
                               var parallelOptions = new ParallelOptions();
                               parallelOptions.CancellationToken = token;
                               parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                               var rezVal = new ThumbnailGenerationResult();

                               try
                               {
                                   Parallel.ForEach(
                                       files,
                                       parallelOptions,
                                       (currentFile) =>
                                           {
                                               parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                                               var image = Image.FromFile(currentFile);
                                               Image destImage = null;
                                               var height = option.Height ?? image.Height;
                                               var width = option.Width ?? image.Width;

                                               try
                                               {
                                                   switch (option.ResizeMethod)
                                                   {
                                                       case ResizeMethod.FixedSize:
                                                           destImage = ScaleImage(image, width, height);
                                                           break;
                                                       case ResizeMethod.FixedHeight:
                                                           destImage = ResizeImage(image, image.Width, height);
                                                           break;
                                                       case ResizeMethod.FixedWidth:
                                                           destImage = ResizeImage(image, width, image.Height);
                                                           break;
                                                       case ResizeMethod.Crop:
                                                           destImage = CropImage(image, width, height);
                                                           break;
                                                   }

                                                   var newName =
                                                       Path.GetFileNameWithoutExtension(currentFile) + '_'
                                                                                                     + Guid.NewGuid()
                                                                                                     + option.FileSuffix;
                                                   var outFileName = Path.Combine(destPath, newName);

                                                   destImage?.Save(outFileName, GetImageFormat(option.FileSuffix));

                                                   rezVal.GeneratedThumbnails.Add(outFileName);
                                               }
                                               finally
                                               {
                                                   destImage?.Dispose();
                                                   image.Dispose();
                                               }
                                           });
                               }
                               catch (OperationCanceledException)
                               {
                               }

                               return rezVal;
                           }, token);
        }

        private static Image ScaleImage(Image image, decimal width, decimal height)
        {
            var widthRatio = image.Width / width;
            var heightRatio = image.Height / height;

            // Resize to the greatest ratio
            var ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
            var newWidth = Convert.ToInt32(Math.Floor(image.Width / ratio));
            var newHeight = Convert.ToInt32(Math.Floor(image.Height / ratio));

            return image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
        }

        private static Image ResizeImage(Image image, decimal width, decimal height)
        {
            var destImage = new Bitmap((int)width, (int)height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    var rectangle = new Rectangle(0, 0, (int)width, (int)height);
                    graphics.DrawImage(
                        image,
                        rectangle,
                        0,
                        0,
                        image.Width,
                        image.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            return destImage;
        }

        private static Image CropImage(Image image, decimal width, decimal height)
        {
            var leftMargin = (image.Width - width) / 2;
            var topMargin = (image.Height - height) / 2;
            var rectangle = new Rectangle((int)leftMargin, (int)topMargin, (int)width, (int)height);
            var bitmap = new Bitmap(image);
            return bitmap.Clone(rectangle, PixelFormat.DontCare);
        }

        private static ImageFormat GetImageFormat(string fileSuffix)
        {
            fileSuffix = fileSuffix.ToLower().TrimStart('.');

            if (fileSuffix == "jpg" || fileSuffix == "jpeg") return ImageFormat.Jpeg;
            if (fileSuffix == "gif") return ImageFormat.Gif;
            if (fileSuffix == "png") return ImageFormat.Png;
            if (fileSuffix == "tiff") return ImageFormat.Tiff;
            if (fileSuffix == "bmp") return ImageFormat.Bmp;

            return null;
        }
    }
}