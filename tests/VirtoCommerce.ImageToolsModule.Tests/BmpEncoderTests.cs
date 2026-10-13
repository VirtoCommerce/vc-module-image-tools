using System;
using VirtoCommerce.ImageToolsModule.Data.Imaging;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class BmpEncoderTests
{
    // 3x2 RGBA pixels, row by row from the top: red, green, blue / white, black, gray
    private static readonly byte[] Pixels =
    [
        255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255,
        255, 255, 255, 255, 0, 0, 0, 255, 128, 128, 128, 255,
    ];

    [Fact]
    public void Encode_WritesUncompressed24BitHeader()
    {
        var bmp = BmpEncoder.Encode(Pixels, 3, 2);

        // Rows of 3 * 3 = 9 bytes are padded to 12
        Assert.Equal(54 + (12 * 2), bmp.Length);
        Assert.Equal("BM"u8.ToArray(), bmp[0..2]);
        Assert.Equal(bmp.Length, BitConverter.ToInt32(bmp, 2));
        Assert.Equal(54, BitConverter.ToInt32(bmp, 10));
        Assert.Equal(40, BitConverter.ToInt32(bmp, 14));
        Assert.Equal(3, BitConverter.ToInt32(bmp, 18));
        Assert.Equal(2, BitConverter.ToInt32(bmp, 22));
        Assert.Equal(24, BitConverter.ToInt16(bmp, 28));
        Assert.Equal(0, BitConverter.ToInt32(bmp, 30));
    }

    [Fact]
    public void Encode_RoundTripsPixels()
    {
        var image = ImageProbe.Decode(BmpEncoder.Encode(Pixels, 3, 2));

        Assert.Equal((3, 2), (image.Width, image.Height));
        Assert.Equal(Rgba.Red, image[0, 0]);
        Assert.Equal(Rgba.Green, image[1, 0]);
        Assert.Equal(Rgba.Blue, image[2, 0]);
        Assert.Equal(Rgba.White, image[0, 1]);
        Assert.Equal(Rgba.Black, image[1, 1]);
        Assert.Equal(new Rgba(128, 128, 128, 255), image[2, 1]);
    }

    [Fact]
    public void Encode_PixelBufferTooSmall_Throws()
    {
        Assert.Throws<ArgumentException>(() => BmpEncoder.Encode(new byte[4], 3, 2));
    }
}
