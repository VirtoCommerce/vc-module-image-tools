namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net.Mime;
    using System.Runtime.Remoting.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

    public class DefaultThumbnailGenerator : IDefaultThumbnailGenerator
    {
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

                                               using (var image = Image.FromFile(currentFile))
                                               {
                                                   var newName =
                                                       Path.GetFileNameWithoutExtension(currentFile) + '_'
                                                                                                     + Guid.NewGuid()
                                                                                                     + option.FileSuffix;
                                                   var outFileName = Path.Combine(destPath, newName);

                                                   var imageFormat = GetImageFormat(option.FileSuffix);

                                                   switch (option.ResizeMethod)
                                                   {
                                                       case ResizeMethod.FixedSize:
                                                           ScaleImage(
                                                               image,
                                                               outFileName,
                                                               option.Width,
                                                               option.Height,
                                                               imageFormat);
                                                           break;
                                                       case ResizeMethod.FixedHeight:
                                                           ScaleImage(
                                                               image,
                                                               outFileName,
                                                               image.Width,
                                                               option.Height,
                                                               imageFormat);
                                                           break;
                                                       case ResizeMethod.FixedWidth:
                                                           ScaleImage(
                                                               image,
                                                               outFileName,
                                                               image.Width,
                                                               option.Height,
                                                               imageFormat);
                                                           break;
                                                       case ResizeMethod.Crop:
                                                           CropImage(
                                                               image,
                                                               outFileName,
                                                               image.Width,
                                                               option.Height,
                                                               imageFormat);
                                                           break;
                                                   }

                                                   rezVal.GeneratedThumbnails.Add(outFileName);
                                               }
                                           });
                               }
                               catch (OperationCanceledException)
                               {
                               }

                               return rezVal;
                           });
        }

        private static void ScaleImage(
            Image image,
            string outFileName,
            decimal width,
            decimal height,
            ImageFormat imageFormat)
        {
            decimal widthRatio = (decimal)image.Width / width;
            decimal heightRatio = (decimal)image.Height / height;

            // Resize to the greatest ratio
            decimal ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
            int newWidth = Convert.ToInt32(Math.Floor((decimal)image.Width / ratio));
            int newHeight = Convert.ToInt32(Math.Floor((decimal)image.Height / ratio));

            using (var thumbnail = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
            {
                thumbnail.Save(outFileName, imageFormat);
            }
        }

        private static void ResizeImage(
            Image image,
            string outFileName,
            decimal width,
            decimal height,
            ImageFormat imageFormat)
        {
            var rectangle = new Rectangle(0, 0, (int)width, (int)height);
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
                    graphics.DrawImage(image, rectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            destImage.Save(outFileName, imageFormat);
        }

        private static void CropImage(
            Image image,
            string outFileName,
            decimal width,
            decimal height,
            ImageFormat imageFormat)
        {
            var leftMargin = (image.Width - width) / 2;
            var topMargin = (image.Height - height) / 2;
            var rectangle = new Rectangle((int)leftMargin, (int)topMargin, (int)width, (int)height);
            var bitmap = new Bitmap(image);
            var outImage = bitmap.Clone(rectangle, PixelFormat.DontCare);
            outImage.Save(outFileName, imageFormat);
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