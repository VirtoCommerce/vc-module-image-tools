using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Test
{
    public class ThumbnailServiceGrayBorderFixTest
    {
        const string _settingsName = "ImageTools.Thumbnails.Parameters";

        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public async Task Check_Gray_Border_In_Thumbnails(string[] thumbnailParams)
        {
            var thumbnailService = InitializeService(Resource.Proff_img, thumbnailParams);

            var result = await thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);

            DirectoryInfo di = new DirectoryInfo(GetResultImagePath());
            foreach (FileInfo file in di.GetFiles())
            {
                var thumbnail = new Bitmap(file.FullName);
                Color pixelColor = thumbnail.GetPixel(0, 0);

                Assert.Equal(pixelColor.A, Color.White.A);
                Assert.Equal(pixelColor.R, Color.White.R);
                Assert.Equal(pixelColor.G, Color.White.G);
                Assert.Equal(pixelColor.B, Color.White.B);
            }
        }

        public ThumbnailService InitializeService(Bitmap image, string[] thumbnailParams)
        {
            var settingsManager = GetSettingsManager(thumbnailParams);
            var blobStorageProvider = GetBlobStorageProvider(image);
            var imageResizer = new ImageResizer();
            var thumbnailService = new ThumbnailService(blobStorageProvider, imageResizer, settingsManager);

            return thumbnailService;
        }

        private ISettingsManager GetSettingsManager(string[] thumbnailParams)
        {
            var settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = thumbnailParams,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(manager => manager.GetSettingByName(It.IsAny<string>()))
                .Returns(() => settings.First(x => x.Name == _settingsName));

            return settingsManager.Object;
        }

        private IBlobStorageProvider GetBlobStorageProvider(Bitmap image)
        {
            var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            stream.Position = 0;

            var blobStorageProvider = new Mock<IBlobStorageProvider>();
            blobStorageProvider.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(stream);
            blobStorageProvider.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns<string>(x =>
            {
                var directoryPath = GetResultImagePath();
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                var uri = new Uri(x);
                string path = directoryPath + "//" + uri.Segments.Last();
                var fs = new FileStream(path, FileMode.Append);
                return fs;
            });
            return blobStorageProvider.Object;
        }

        private string GetResultImagePath()
        {
            string directoryPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "//Result_thumbnails").LocalPath;
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            if (!di.Exists)
                di.Create();

            return directoryPath;
        }
    }

    public class TestDataGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new [] { new[] { "{width: 160, height: 160, alias: \"compact\", color: \"#FFFFFF\", method: \"Crop\", anchorposition:\"Center\"}" } },
            new [] { new[] { "{width: 50, height: 50, alias: \"thumb\", color: \"#FFFFFF\", method: \"Crop\", anchorposition:\"Center\"}" } },
            new [] { new[] { "{width: 100, height: 100, alias: \"small\", color: \"#FFFFFF\", method: \"Crop\", anchorposition:\"Center\"}" } },
            new [] { new[] { "{width: 480, height: 480, alias: \"large\", color: \"#FFFFFF\", method: \"Crop\", anchorposition:\"Center\"}" } }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
