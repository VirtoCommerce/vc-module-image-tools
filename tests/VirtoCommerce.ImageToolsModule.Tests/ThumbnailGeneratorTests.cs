using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailGeneratorTests
    {

        [Fact]
        public Task GenerateThumbnailsAsync_CancellationTokenNotNull_CancellationFired()
        {
            var options = new List<ThumbnailOption>();
            var token = new CancellationToken(true);
            var tokenWrapper = new CancellationTokenWrapper(token);

            var mockStorage = new Mock<IImageService>();
            var mockResizer = new Mock<IImageResizer>();

            var target = new ThumbnailGenerator(mockStorage.Object, mockResizer.Object, Mock.Of<ILogger<ThumbnailGenerator>>());
            return Assert.ThrowsAsync<OperationCanceledException>(async () => await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, tokenWrapper));
        }

        [Fact]
        public async Task GenerateThumbnailsAsync_FixedSize_FixedSizeCalled()
        {
            var options = new List<ThumbnailOption>
            {
                 new ThumbnailOption
                 {
                     ResizeMethod = ResizeMethod.FixedSize,
                     FileSuffix = "test",
                 }
             };

            var image = new Image<Rgba32>(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult(image));


            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedSize(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Rgba32>())).Returns(image);

            var target = new ThumbnailGenerator(mockStorage.Object, mockResizer.Object, Mock.Of<ILogger<ThumbnailGenerator>>());
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            Assert.NotNull(result);

            mockResizer.Verify(x => x.FixedSize(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Rgba32>()));
        }

        [Fact]
        public async Task GenerateThumbnailsAsync_Crop_CropCalled()
        {
            var options = new List<ThumbnailOption>
            {
                new ThumbnailOption
                {
                    ResizeMethod = ResizeMethod.Crop,
                    FileSuffix = "test",
                    Width = 10,
                    Height = 20
                }
            };

            var image = new Image<Rgba32>(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.Crop(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnchorPosition>())).Returns(image);

            var target = new ThumbnailGenerator(mockStorage.Object, mockResizer.Object, Mock.Of<ILogger<ThumbnailGenerator>>());
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            Assert.NotNull(result);

            mockResizer.Verify(x => x.Crop(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnchorPosition>()));
        }

        [Fact]
        public async Task GenerateThumbnailsAsync_FixedWidth_FFixedWidthCalled()
        {
            var options = new List<ThumbnailOption>
            {
                new ThumbnailOption
                {
                    ResizeMethod = ResizeMethod.FixedWidth,
                    FileSuffix = "test",
                }
            };

            var image = new Image<Rgba32>(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedWidth(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<Rgba32>())).Returns(image);

            var target = new ThumbnailGenerator(mockStorage.Object, mockResizer.Object, Mock.Of<ILogger<ThumbnailGenerator>>());
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            Assert.NotNull(result);

            mockResizer.Verify(x => x.FixedWidth(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<Rgba32>()));
        }

        [Fact]
        public async Task GenerateThumbnailsAsync_FixedHeight_FixedHeightCalled()
        {
            var options = new List<ThumbnailOption>
            {
                new ThumbnailOption
                {
                    ResizeMethod = ResizeMethod.FixedHeight,
                    FileSuffix = "test",
                }
            };

            var image = new Image<Rgba32>(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedHeight(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<Rgba32>())).Returns(image);

            var target = new ThumbnailGenerator(mockStorage.Object, mockResizer.Object, Mock.Of<ILogger<ThumbnailGenerator>>());
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            Assert.NotNull(result);

            mockResizer.Verify(x => x.FixedHeight(It.IsAny<Image<Rgba32>>(), It.IsAny<int>(), It.IsAny<Rgba32>()));
        }
    }
}
