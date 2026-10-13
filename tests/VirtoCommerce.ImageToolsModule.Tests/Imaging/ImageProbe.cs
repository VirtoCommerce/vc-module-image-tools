using System;
using System.Globalization;
using System.Text;
using SkiaSharp;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests.Imaging;

internal readonly record struct Rgba(byte R, byte G, byte B, byte A)
{
    public static readonly Rgba Red = new(255, 0, 0, 255);
    public static readonly Rgba Green = new(0, 255, 0, 255);
    public static readonly Rgba Blue = new(0, 0, 255, 255);
    public static readonly Rgba Black = new(0, 0, 0, 255);
    public static readonly Rgba White = new(255, 255, 255, 255);
    public static readonly Rgba Transparent = new(0, 0, 0, 0);

    /// <summary>
    /// Parses #RRGGBB or #RRGGBBAA, the form used in theory data.
    /// </summary>
    public static Rgba Parse(string hex)
    {
        var value = hex.TrimStart('#');
        var alpha = value.Length == 8 ? byte.Parse(value[6..8], NumberStyles.HexNumber) : (byte)255;

        return new Rgba(
            byte.Parse(value[0..2], NumberStyles.HexNumber),
            byte.Parse(value[2..4], NumberStyles.HexNumber),
            byte.Parse(value[4..6], NumberStyles.HexNumber),
            alpha);
    }

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";
}

internal sealed record ProbedImage(int Width, int Height, byte[] Pixels)
{
    public Rgba this[int x, int y]
    {
        get
        {
            var i = ((y * Width) + x) * 4;
            return new Rgba(Pixels[i], Pixels[i + 1], Pixels[i + 2], Pixels[i + 3]);
        }
    }
}

/// <summary>
/// Inspects generated thumbnails independently of the code under test:
/// container format by file signature, pixels by an independent decoder.
/// </summary>
internal static class ImageProbe
{
    public const string Jpeg = "JPEG";
    public const string Png = "PNG";
    public const string Webp = "WEBP";
    public const string Gif = "GIF";
    public const string Bmp = "BMP";
    public const string Tiff = "TIFF";
    public const string Tga = "TGA";
    public const string Pbm = "PBM";
    public const string Qoi = "QOI";

    public static string DetectFormat(byte[] content)
    {
        ReadOnlySpan<byte> b = content;

        return b switch
        {
            [0xFF, 0xD8, 0xFF, ..] => Jpeg,
            [0x89, (byte)'P', (byte)'N', (byte)'G', ..] => Png,
            [(byte)'R', (byte)'I', (byte)'F', (byte)'F', _, _, _, _, (byte)'W', (byte)'E', (byte)'B', (byte)'P', ..] => Webp,
            [(byte)'G', (byte)'I', (byte)'F', (byte)'8', ..] => Gif,
            [(byte)'B', (byte)'M', ..] => Bmp,
            [(byte)'I', (byte)'I', 0x2A, 0x00, ..] or [(byte)'M', (byte)'M', 0x00, 0x2A, ..] => Tiff,
            [(byte)'q', (byte)'o', (byte)'i', (byte)'f', ..] => Qoi,
            [(byte)'P', >= (byte)'1' and <= (byte)'6', ..] => Pbm,
            _ when IsTga(b) => Tga,
            _ => "Unknown",
        };
    }

    public static bool IsAnimatedWebp(byte[] content)
    {
        return DetectFormat(content) == Webp && content.AsSpan().IndexOf("ANIM"u8) >= 0;
    }

    /// <summary>
    /// Formats the independent decoder can read back.
    /// </summary>
    public static bool CanDecode(string format)
    {
        return format is Jpeg or Png or Webp or Gif or Bmp;
    }

    public static ProbedImage Decode(byte[] content)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(content))
            ?? throw new InvalidOperationException($"Not decodable: {DetectFormat(content)}");

        var info = codec.Info.WithColorType(SKColorType.Rgba8888).WithAlphaType(SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(info);

        var result = codec.GetPixels(info, bitmap.GetPixels());
        Assert.True(result is SKCodecResult.Success or SKCodecResult.IncompleteInput, $"Decode failed: {result}");

        return new ProbedImage(info.Width, info.Height, bitmap.GetPixelSpan().ToArray());
    }

    public static void AssertColor(Rgba expected, Rgba actual, int tolerance, string because)
    {
        // A fully transparent pixel has no meaningful color
        var matches = expected.A == 0
            ? actual.A <= tolerance
            : Math.Abs(expected.R - actual.R) <= tolerance
                && Math.Abs(expected.G - actual.G) <= tolerance
                && Math.Abs(expected.B - actual.B) <= tolerance
                && Math.Abs(expected.A - actual.A) <= tolerance;

        Assert.True(matches, $"{because}: expected {expected} ±{tolerance}, actual {actual}");
    }

    // TGA has no signature: accept the v2 footer, or a plausible 18-byte header
    // (color-map type 0/1, known image type, non-zero size)
    private static bool IsTga(ReadOnlySpan<byte> content)
    {
        if (content.EndsWith(Encoding.ASCII.GetBytes("TRUEVISION-XFILE.\0")))
        {
            return true;
        }

        return content.Length > 18
            && content[1] <= 1
            && content[2] is 1 or 2 or 3 or 9 or 10 or 11
            && BitConverter.ToUInt16(content[12..14]) > 0
            && BitConverter.ToUInt16(content[14..16]) > 0;
    }
}
