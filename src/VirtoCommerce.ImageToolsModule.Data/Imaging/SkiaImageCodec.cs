using System;
using System.Runtime.InteropServices;
using SkiaSharp;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Data.Imaging;

/// <summary>
/// Decodes and encodes the supported raster formats with SkiaSharp (BMP is written by <see cref="BmpEncoder"/>).
/// </summary>
public static class SkiaImageCodec
{
    /// <summary>
    /// WebP quality for lossy thumbnails; JPEG quality comes from the thumbnail option.
    /// </summary>
    public const int WebpQuality = 75;

    /// <summary>
    /// Decodes the first frame of an image. Returns null when the content is not an image in a supported format.
    /// Embedded color profiles are not applied and EXIF orientation is not applied.
    /// </summary>
    public static SkiaRasterImage Decode(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);

        var format = codec?.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => RasterImageFormat.Jpeg,
            SKEncodedImageFormat.Png => RasterImageFormat.Png,
            SKEncodedImageFormat.Webp => RasterImageFormat.Webp,
            SKEncodedImageFormat.Bmp => RasterImageFormat.Bmp,
            _ => null,
        };

        if (format is null)
        {
            return null;
        }

        var alphaType = codec.Info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, alphaType);
        var bitmap = new SKBitmap(info);

        var result = codec.GetPixels(info, bitmap.GetPixels());
        if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
        {
            bitmap.Dispose();
            return null;
        }

        var isLossless = format == RasterImageFormat.Webp && WebpInspector.IsLossless(content);

        return new SkiaRasterImage(bitmap, format, isLossless);
    }

    /// <summary>
    /// Encodes an image. JPEG drops alpha (transparent pixels turn black), BMP is written as 24-bit.
    /// </summary>
    public static byte[] Encode(SkiaRasterImage image, RasterImageFormat format, JpegQuality jpegQuality)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(format);

        if (format == RasterImageFormat.Bmp)
        {
            return BmpEncoder.Encode(ReadUnpremultipliedPixels(image.Bitmap), image.Width, image.Height);
        }

        using var pixmap = image.Bitmap.PeekPixels();
        using var data = format switch
        {
            _ when format == RasterImageFormat.Jpeg => pixmap.Encode(GetJpegOptions(jpegQuality)),
            _ when format == RasterImageFormat.Png => pixmap.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, zLibLevel: 6)),
            _ when format == RasterImageFormat.Webp => pixmap.Encode(new SKWebpEncoderOptions(image.IsLossless ? SKWebpEncoderCompression.Lossless : SKWebpEncoderCompression.Lossy, WebpQuality)),
            _ => throw new NotSupportedException($"Encoding to {format} is not supported."),
        };

        return data?.ToArray() ?? throw new InvalidOperationException($"Could not encode the image as {format}.");
    }

    private static SKJpegEncoderOptions GetJpegOptions(JpegQuality jpegQuality)
    {
        // Full-resolution chroma for the highest qualities, 4:2:0 otherwise
        var quality = (int)jpegQuality;
        var downsample = quality >= 91 ? SKJpegEncoderDownsample.Downsample444 : SKJpegEncoderDownsample.Downsample420;

        return new SKJpegEncoderOptions(quality, downsample, SKJpegEncoderAlphaOption.Ignore);
    }

    private static byte[] ReadUnpremultipliedPixels(SKBitmap bitmap)
    {
        var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var pixels = new byte[info.BytesSize];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        try
        {
            using var pixmap = bitmap.PeekPixels();
            if (!pixmap.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes))
            {
                throw new InvalidOperationException("Could not read the image pixels.");
            }
        }
        finally
        {
            handle.Free();
        }

        return pixels;
    }
}
