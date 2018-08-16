using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class DefaultThumbnailGeneratorTests
    {

        [Fact]
        public async void GenerateThumbnailsAsync_CancellationTokenNotNull_CancellationFired()
        {
            var options = new List<ThumbnailOption>();
            CancellationToken token = new CancellationToken(true);
            var tokenWrapper = new CancellationTokenWrapper(token);

            var mockStorage = new Mock<IImageService>();
            var mockResizer = new Mock<IImageResizer>();

            var target = new DefaultThumbnailGenerator(mockStorage.Object, mockResizer.Object);
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, tokenWrapper));
        }

        [Fact]
        public async void GenerateThumbnailsAsync_FixedSize_FixedSizeCalled()
        {
            var options = new List<ThumbnailOption>()
             {
                 new ThumbnailOption()
                 {
                     ResizeMethod = ResizeMethod.FixedSize,
                     FileSuffix = "test",
                 }
             };

            var image = new Bitmap(50, 50);
            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.GetImageCodecInfo(It.IsAny<Image>())).Returns((new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders())).Find(x => x.MimeType == "image/bmp"));
            mockStorage.Setup(x => x.GetEncoderParameters(It.IsAny<Image>(), It.IsAny<ThumbnailOption>())).Returns(new EncoderParameters(1) { Param = new EncoderParameter[1] { new EncoderParameter(Encoder.Quality, 92) } });
            //mockStorage.Setup(x => x.GetImageFormat(It.IsAny<Image>())).Returns(ImageFormat.Bmp);
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult<Image>(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedSize(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Color>())).Returns(image);

            var target = new DefaultThumbnailGenerator(mockStorage.Object, mockResizer.Object);
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            mockResizer.Verify(x => x.FixedSize(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Color>()));
        }

        [Fact]
        public async void GenerateThumbnailsAsync_Crop_CropCalled()
        {
            var options = new List<ThumbnailOption>()
            {
                new ThumbnailOption()
                {
                    ResizeMethod = ResizeMethod.Crop,
                    FileSuffix = "test",
                }
            };

            var image = new Bitmap(50, 50);
            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.GetImageCodecInfo(It.IsAny<Image>())).Returns((new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders())).Find(x => x.MimeType == "image/bmp"));
            mockStorage.Setup(x => x.GetEncoderParameters(It.IsAny<Image>(), It.IsAny<ThumbnailOption>())).Returns(new EncoderParameters(1) { Param = new EncoderParameter[1] { new EncoderParameter(Encoder.Quality, 92) } });
            //mockStorage.Setup(x => x.GetImageFormat(It.IsAny<Image>())).Returns(ImageFormat.Bmp);
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult<Image>(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.Crop(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnchorPosition>())).Returns(image);

            var target = new DefaultThumbnailGenerator(mockStorage.Object, mockResizer.Object);
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            mockResizer.Verify(x => x.Crop(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AnchorPosition>()));
        }

        [Fact]
        public async void GenerateThumbnailsAsync_FixedWidth_FFixedWidthCalled()
        {
            var options = new List<ThumbnailOption>()
            {
                new ThumbnailOption()
                {
                    ResizeMethod = ResizeMethod.FixedWidth,
                    FileSuffix = "test",
                }
            };

            var image = new Bitmap(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.GetImageCodecInfo(It.IsAny<Image>())).Returns((new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders())).Find(x => x.MimeType == "image/bmp"));
            mockStorage.Setup(x => x.GetEncoderParameters(It.IsAny<Image>(), It.IsAny<ThumbnailOption>())).Returns(new EncoderParameters(1) { Param = new EncoderParameter[1] { new EncoderParameter(Encoder.Quality, 92) } });
            //mockStorage.Setup(x => x.GetImageFormat(It.IsAny<Image>())).Returns(ImageFormat.Bmp);
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult<Image>(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedWidth(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<Color>())).Returns(image);

            var target = new DefaultThumbnailGenerator(mockStorage.Object, mockResizer.Object);
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            mockResizer.Verify(x => x.FixedWidth(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<Color>()));
        }

        [Fact]
        public async void GenerateThumbnailsAsync_FixedHeight_FixedHeightCalled()
        {
            var options = new List<ThumbnailOption>()
            {
                new ThumbnailOption()
                {
                    ResizeMethod = ResizeMethod.FixedHeight,
                    FileSuffix = "test",
                }
            };

            var image = new Bitmap(50, 50);

            var mockStorage = new Mock<IImageService>();
            mockStorage.Setup(x => x.GetImageCodecInfo(It.IsAny<Image>())).Returns((new List<ImageCodecInfo>(ImageCodecInfo.GetImageEncoders())).Find(x => x.MimeType == "image/bmp"));
            mockStorage.Setup(x => x.GetEncoderParameters(It.IsAny<Image>(), It.IsAny<ThumbnailOption>())).Returns(new EncoderParameters(1) { Param = new EncoderParameter[1] { new EncoderParameter(Encoder.Quality, 92) } });
            //mockStorage.Setup(x => x.GetImageFormat(It.IsAny<Image>())).Returns(ImageFormat.Bmp);
            mockStorage.Setup(x => x.LoadImageAsync(It.IsAny<string>())).Returns(Task.FromResult<Image>(image));

            var mockResizer = new Mock<IImageResizer>();
            mockResizer.Setup(x => x.FixedHeight(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<Color>())).Returns(image);

            var target = new DefaultThumbnailGenerator(mockStorage.Object, mockResizer.Object);
            var result = await target.GenerateThumbnailsAsync("http://pathToFile.bmp", "dest", options, null);

            mockResizer.Verify(x => x.FixedHeight(It.IsAny<Image>(), It.IsAny<int>(), It.IsAny<Color>()));
        }
    }
}