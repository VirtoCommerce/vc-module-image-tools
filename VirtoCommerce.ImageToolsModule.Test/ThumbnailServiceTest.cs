using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Test
{
    public class ThumbnailServiceTest
    {
        const string _settingsName = "ImageTools.Thumbnails.Parameters";
        private List<SettingEntry> _settings = new List<SettingEntry>();
        private readonly IThumbnailService _thumbnailService;

        public ThumbnailServiceTest()
        {
            var settingsManager = GetSettingsManager();
            var blobStorageProvider = GetBlobStorageProvider();
            var imageResizer = new ImageResizer();
            _thumbnailService = new ThumbnailService(blobStorageProvider, imageResizer, settingsManager);
        }

        [Fact]
        public async Task Generate_FixedSize_Thumbnail()
        {
            var arrayValues = new[]
            {
                "{method: 'FixedSize', alias: 'grande', width: 800, height:600, background: '#B20000'} "
            };

            _settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = arrayValues,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var result = await _thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);
        }

        [Fact]
        public async Task Generate_FixedHeight_Thumbnail()
        {
            var arrayValues = new[]
            {
                "{method: 'FixedHeight', alias: 'medium', height:240}"
            };

            _settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = arrayValues,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var result = await _thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);
        }

        [Fact]
        public async Task Generate_FixedWidth_Thumbnail()
        {
            var arrayValues = new[]
            {
                "{method: 'FixedWidth', alias: 'large', width: 480}"
            };

            _settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = arrayValues,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var result = await _thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);
        }

        [Fact]
        public async Task Generate_Crop_Thumbnail()
        {
            var arrayValues = new[]
            {
                "{method: 'Crop', alias: 'compact', width: 160, height:160, anchorposition:'TopLeft'}"
            };

            _settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = arrayValues,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var result = await _thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);
        }

        [Fact]
        public async Task Generate_All_Thumbnails()
        {
            var arrayValues = new[]
            {
                "{method: 'FixedSize', alias: 'grande', width: 800, height:600, background: '#B20000'}",
                "{method: 'FixedHeight', alias: 'medium', height:240}",
                "{method: 'FixedWidth', alias: 'large', width: 480}",
                "{method: 'Crop', alias: 'compact', width: 160, height:160, anchorposition:'TopLeft'}"
            };

            _settings = new List<SettingEntry> { new SettingEntry
            {
                Name = _settingsName,
                ArrayValues = arrayValues,
                IsArray = true,
                ValueType = SettingValueType.ShortText
            } };

            var result = await _thumbnailService.GenerateAsync("https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg", true);
            Assert.True(result);
        }

        #region Interfaces

        private ISettingsManager GetSettingsManager()
        {
            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(manager => manager.GetArray(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(() => _settings.First(x => x.Name == _settingsName).ArrayValues);

            return settingsManager.Object;
        }

        private IBlobStorageProvider GetBlobStorageProvider()
        {
            var stream = GetImageFromResource("Image1");

            var blobStorageProvider = new Mock<IBlobStorageProvider>();
            blobStorageProvider.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(stream);
            blobStorageProvider.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => { var aa = new MemoryStream(); return aa; });
            return blobStorageProvider.Object;
        }

        private IImageResizer GetImageResizer()
        {
            var imageResizer = new Mock<IImageResizer>();
            imageResizer
                .Setup(manager => manager.FixedSize(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Color>()))
                .Returns((Image image, int width, int height, Color color) => (Image)image.Clone());

            return imageResizer.Object;
        }

        #endregion

        #region Private

        private Stream GetImageFromResource(string fileName)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var mfStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[0]);

            using (var resReader = new ResourceReader(mfStream))
            {
                string resType;
                byte[] resData;

                var aa = resReader.GetEnumerator();

                resReader.GetResourceData(fileName, out resType, out resData);

                var startIndex = 0;
                // Search begin of image
                for (var i = 0; i < resData.Length - 1; i++)
                {
                    if (resData[i] == 0xFF && resData[i + 1] == 0xD8)
                    {
                        startIndex = i;
                        break;
                    }
                }

                var result = new MemoryStream();

                result.Write(resData, startIndex, resData.Length - startIndex);
                result.Position = 0;
                return result;
            }
        }

        #endregion
    }
}
