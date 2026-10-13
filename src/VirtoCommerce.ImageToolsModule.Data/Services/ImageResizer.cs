using System;
using SkiaSharp;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Imaging;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// Image resize library
    /// </summary>
    public class ImageResizer : IImageResizer
    {
        /// <summary>
        /// Scale image by given percent
        /// </summary>
        public virtual RasterImage ScaleByPercent(RasterImage image, int percent)
        {
            return Render(image, ResizePlanner.Scale(image.Size, percent), ImageColor.Transparent);
        }

        /// <summary>
        /// Resize image vertically with keeping it aspect rate.
        /// </summary>
        public virtual RasterImage FixedHeight(RasterImage image, int height, ImageColor background)
        {
            return Render(image, ResizePlanner.FitWithin(image.Size, new ImageSize(image.Width, height)), background);
        }

        /// <summary>
        /// Resize image horizontally with keeping it aspect rate
        /// </summary>
        public virtual RasterImage FixedWidth(RasterImage image, int width, ImageColor background)
        {
            return Render(image, ResizePlanner.FitWithin(image.Size, new ImageSize(width, image.Height)), background);
        }

        /// <summary>
        /// Resize image.
        /// Original image will be resized proportionally to fit given Width, Height.
        /// Original image will not be cropped.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right).
        /// The empty spaces will be filled with given color.
        /// </summary>
        public virtual RasterImage FixedSize(RasterImage image, int width, int height, ImageColor background)
        {
            return Render(image, ResizePlanner.Pad(image.Size, new ImageSize(width, height)), background);
        }

        /// <summary>
        /// Resize and trim excess.
        /// The image will have given size
        /// </summary>
        public virtual RasterImage Crop(RasterImage image, int width, int height, AnchorPosition anchor)
        {
            if (!Enum.IsDefined(anchor))
            {
                throw new ArgumentOutOfRangeException(nameof(anchor), $"AnchorPosition {anchor} not supported.");
            }

            return Render(image, ResizePlanner.Crop(image.Size, new ImageSize(width, height), anchor), ImageColor.Transparent);
        }

        /// <summary>
        /// Draws the whole source scaled into <see cref="ResizePlan.Destination"/> on a canvas filled with <paramref name="background"/>.
        /// The result keeps the source format; a source without alpha produces an opaque result.
        /// </summary>
        protected virtual RasterImage Render(RasterImage source, ResizePlan plan, ImageColor background)
        {
            var skiaSource = source as SkiaRasterImage
                ?? throw new ArgumentException($"Expected an image created by {nameof(ImageService)}, got {source?.GetType().Name ?? "null"}.", nameof(source));

            var result = SkiaRasterImage.Create(plan.Canvas, source.Format, source.HasAlpha, background, source.IsLossless);
            var destination = plan.Destination;

            using var canvas = new SKCanvas(result.Bitmap);
            using var image = SKImage.FromBitmap(skiaSource.Bitmap);
            canvas.DrawImage(image, SKRect.Create(destination.X, destination.Y, destination.Width, destination.Height), GetSampling(source.Size, destination));

            return result;
        }

        /// <summary>
        /// Mipmapped linear filtering averages detail when shrinking; Catmull-Rom keeps edges sharp when enlarging.
        /// </summary>
        protected virtual SKSamplingOptions GetSampling(ImageSize source, ImageRectangle destination)
        {
            return destination.Width < source.Width || destination.Height < source.Height
                ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)
                : new SKSamplingOptions(SKCubicResampler.CatmullRom);
        }
    }
}
