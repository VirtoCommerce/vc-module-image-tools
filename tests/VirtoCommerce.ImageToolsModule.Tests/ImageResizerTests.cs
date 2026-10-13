using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Data.Imaging;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ImageResizerTests
    {
        private static readonly ImageColor Green = new(0, 255, 0, 255);

        private class ImageResizerTestClass : ImageResizer
        {
            public RasterImage RenderTest(RasterImage source, ResizePlan plan, ImageColor background)
            {
                return Render(source, plan, background);
            }
        }

        private static RasterImage CreateImage(int width, int height)
        {
            return SkiaRasterImage.Create(new ImageSize(width, height), RasterImageFormat.Png, hasAlpha: true, ImageColor.Transparent);
        }

        [Fact]
        public void ScaleByPercent()
        {
            var resizer = new ImageResizerTestClass();
            using var image = CreateImage(10, 100);
            using var result = resizer.ScaleByPercent(image, 20);
            Assert.True(result.Width == 2 && result.Height == 20);
        }

        [Fact]
        public void FixedHeight()
        {
            var resizer = new ImageResizerTestClass();
            using var image = CreateImage(10, 100);
            using var result = resizer.FixedHeight(image, 20, Green);
            Assert.True(result.Width == 2 && result.Height == 20);
        }

        [Fact]
        public void FixedWidth()
        {
            var resizer = new ImageResizerTestClass();
            using var image = CreateImage(100, 10);
            using var result = resizer.FixedWidth(image, 20, Green);
            Assert.True(result.Width == 20 && result.Height == 2);
        }

        [Fact]
        public void FixedSize()
        {
            var resizer = new ImageResizerTestClass();
            using var image = CreateImage(100, 100);
            using var result = resizer.FixedSize(image, 20, 10, Green);
            Assert.True(result.Width == 20 && result.Height == 10);
        }

        [Fact]
        public void FixedSize_VeryWideImage_KeepsAtLeastOneRowOfImage()
        {
            var resizer = new ImageResizerTestClass();
            using var image = SkiaRasterImage.Create(new ImageSize(1920, 60), RasterImageFormat.Png, hasAlpha: false, new ImageColor(255, 0, 0, 255));

            using var result = (SkiaRasterImage)resizer.FixedSize(image, 16, 16, Green);

            Assert.True(result.Width == 16 && result.Height == 16);
            Assert.Equal(new SkiaSharp.SKColor(0, 255, 0), result.Bitmap.GetPixel(8, 2));
            Assert.Equal(new SkiaSharp.SKColor(255, 0, 0), result.Bitmap.GetPixel(8, 8));
        }

        [Fact]
        public void Render()
        {
            var resizer = new ImageResizerTestClass();
            using var image = CreateImage(100, 100);
            var plan = new ResizePlan(new ImageSize(50, 50), new ImageRectangle(10, 10, 20, 20));

            using var result = resizer.RenderTest(image, plan, ImageColor.Transparent);
            Assert.True(result.Width == 50 && result.Height == 50);
            Assert.Equal(image.Format, result.Format);
        }
    }
}
