namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.Remoting.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

    public class DefaultThumbnailGenerator : IDefaultThumbnailGenerator
    {
        public async Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(string sourcePath, string destPath, ThumbnailOption option, CancellationToken token)
        {
            var files = Directory.GetFiles(sourcePath);

            List<Task<string>> tasks = new List<Task<string>>();
            TaskFactory factory = new TaskFactory(token);

            var rezVal = new ThumbnailGenerationResult();

            foreach (var file in files)
            {
                await factory.StartNew(
                    () =>
                        {
                            using (var image = Image.FromFile(file))
                            {
                                decimal widthRatio = (decimal)image.Width / option.Width;
                                decimal heightRatio = (decimal)image.Height / option.Height;

                                // Resize to the greatest ratio
                                decimal ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
                                int newWidth = Convert.ToInt32(Math.Floor((decimal)image.Width / ratio));
                                int newHeight = Convert.ToInt32(Math.Floor((decimal)image.Height / ratio));

                                // Create new file name
                                var fileSuffix = Path.GetExtension(file);
                                var newName = Path.GetFileNameWithoutExtension(file) + '_' + Guid.NewGuid()
                                              + fileSuffix;
                                var outFilename = Path.Combine(destPath, newName);

                                using (var thumbnail = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                                {
                                    thumbnail.Save(outFilename, GetImageFormat(fileSuffix));
                                }

                                rezVal.GeneratedThumbnails.Add(outFilename);
                            }
                        },
                    token);
            }

            return rezVal;
        }

        private static ImageFormat GetImageFormat(string fileSuffix)
        {
            fileSuffix = fileSuffix.ToLower().TrimStart('.');

            if(fileSuffix == "jpg" ||
               fileSuffix == "jpeg") return ImageFormat.Jpeg;
            if (fileSuffix == "gif") return ImageFormat.Gif;
            if (fileSuffix == "png") return ImageFormat.Png;
            if (fileSuffix == "tiff") return ImageFormat.Tiff;
            if (fileSuffix == "bmp") return ImageFormat.Bmp;

            return null;
        }
    }
}