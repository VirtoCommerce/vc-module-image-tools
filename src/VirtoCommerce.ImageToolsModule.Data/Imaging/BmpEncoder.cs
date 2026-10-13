using System;
using System.Buffers.Binary;

namespace VirtoCommerce.ImageToolsModule.Data.Imaging;

/// <summary>
/// Writes uncompressed 24-bit BMP files (BITMAPINFOHEADER, bottom-up rows). Alpha is not stored.
/// </summary>
public static class BmpEncoder
{
    private const int FileHeaderSize = 14;
    private const int InfoHeaderSize = 40;
    private const int PixelDataOffset = FileHeaderSize + InfoHeaderSize;

    /// <param name="rgba">Unpremultiplied RGBA pixels, top row first, 4 bytes per pixel.</param>
    public static byte[] Encode(ReadOnlySpan<byte> rgba, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if (rgba.Length < width * height * 4)
        {
            throw new ArgumentException($"Expected {width * height * 4} bytes of RGBA pixels, got {rgba.Length}.", nameof(rgba));
        }

        var stride = ((width * 3) + 3) & ~3;
        var bmp = new byte[PixelDataOffset + (stride * height)];
        var span = bmp.AsSpan();

        // BITMAPFILEHEADER
        span[0] = (byte)'B';
        span[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(span[2..], bmp.Length);
        BinaryPrimitives.WriteInt32LittleEndian(span[10..], PixelDataOffset);

        // BITMAPINFOHEADER: positive height means bottom-up rows, compression 0 = BI_RGB
        BinaryPrimitives.WriteInt32LittleEndian(span[14..], InfoHeaderSize);
        BinaryPrimitives.WriteInt32LittleEndian(span[18..], width);
        BinaryPrimitives.WriteInt32LittleEndian(span[22..], height);
        BinaryPrimitives.WriteInt16LittleEndian(span[26..], 1);
        BinaryPrimitives.WriteInt16LittleEndian(span[28..], 24);
        BinaryPrimitives.WriteInt32LittleEndian(span[34..], stride * height);

        for (var y = 0; y < height; y++)
        {
            var source = rgba.Slice(y * width * 4, width * 4);
            var target = span.Slice(PixelDataOffset + ((height - 1 - y) * stride), width * 3);

            for (var x = 0; x < width; x++)
            {
                target[(x * 3) + 0] = source[(x * 4) + 2];
                target[(x * 3) + 1] = source[(x * 4) + 1];
                target[(x * 3) + 2] = source[x * 4];
            }
        }

        return bmp;
    }
}
