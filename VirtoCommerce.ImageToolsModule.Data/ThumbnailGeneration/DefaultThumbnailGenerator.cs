namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
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

                                               var image = Image.FromFile(currentFile);
                                               Image destImage = null;

                                               try
                                               {
                                                   switch (option.ResizeMethod)
                                                   {
                                                       case ResizeMethod.FixedSize:
                                                           destImage = ScaleImage(image, option.Width, option.Height);
                                                           break;
                                                       case ResizeMethod.FixedHeight:
                                                           destImage = ResizeImage(image, image.Width, option.Height);
                                                           break;
                                                       case ResizeMethod.FixedWidth:
                                                           destImage = ResizeImage(image, option.Width, image.Height);
                                                           break;
                                                       case ResizeMethod.Crop:
                                                           destImage = CropImage(image, option.Width, option.Height);
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
                           });
        }

        private static Image ScaleImage(Image image, decimal width, decimal height)
        {
            decimal widthRatio = (decimal)image.Width / width;
            decimal heightRatio = (decimal)image.Height / height;

            // Resize to the greatest ratio
            decimal ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
            int newWidth = Convert.ToInt32(Math.Floor((decimal)image.Width / ratio));
            int newHeight = Convert.ToInt32(Math.Floor((decimal)image.Height / ratio));

            return image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
        }

        private static Image ResizeImage(Image image, decimal width, decimal height)
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
            var destImage = bitmap.Clone(rectangle, PixelFormat.DontCare);
            return destImage;
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