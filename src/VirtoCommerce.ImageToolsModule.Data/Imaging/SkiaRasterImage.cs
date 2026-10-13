using System;
using SkiaSharp;
using VirtoCommerce.ImageToolsModule.Core.Imaging;

namespace VirtoCommerce.ImageToolsModule.Data.Imaging;

/// <summary>
/// A raster image held in a SkiaSharp bitmap: RGBA 8888, premultiplied, or opaque when the source has no alpha.
/// </summary>
public sealed class SkiaRasterImage : RasterImage
{
    public SkiaRasterImage(SKBitmap bitmap, RasterImageFormat format, bool isLossless = false)
        : base(format, isLossless)
    {
        Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
    }

    public SKBitmap Bitmap { get; }

    public override int Width => Bitmap.Width;

    public override int Height => Bitmap.Height;

    public override bool HasAlpha => Bitmap.AlphaType != SKAlphaType.Opaque;

    /// <summary>
    /// Creates a blank image of the given size, filled with <paramref name="color"/>.
    /// </summary>
    /// <remarks>Without alpha the color is written opaque: its alpha is dropped, not blended.</remarks>
    public static SkiaRasterImage Create(ImageSize size, RasterImageFormat format, bool hasAlpha, ImageColor color, bool isLossless = false)
    {
        var bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height, SKColorType.Rgba8888, hasAlpha ? SKAlphaType.Premul : SKAlphaType.Opaque));
        var fill = hasAlpha ? color : color.ToOpaque();
        bitmap.Erase(new SKColor(fill.R, fill.G, fill.B, fill.A));

        return new SkiaRasterImage(bitmap, format, isLossless);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Bitmap.Dispose();
        }
    }
}
