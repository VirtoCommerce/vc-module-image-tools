using System;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class ImageColorTests
{
    [Theory]
    [InlineData("#00FF00", 0, 255, 0, 255)]
    [InlineData("00FF00", 0, 255, 0, 255)]
    [InlineData("#00ff00", 0, 255, 0, 255)]
    [InlineData("#0F0", 0, 255, 0, 255)]
    [InlineData("0F0", 0, 255, 0, 255)]
    [InlineData("#0F08", 0, 255, 0, 136)]
    [InlineData("#00FF0080", 0, 255, 0, 128)]
    [InlineData("12345678", 0x12, 0x34, 0x56, 0x78)]
    public void Parse_SupportedHexForms_ReturnsColor(string hex, int r, int g, int b, int a)
    {
        var color = ImageColor.Parse(hex);

        Assert.Equal(new ImageColor((byte)r, (byte)g, (byte)b, (byte)a), color);
    }

    [Theory]
    [InlineData("not-a-color")]
    [InlineData("#12345")]
    [InlineData("#12")]
    [InlineData("#GGGGGG")]
    [InlineData("#")]
    [InlineData("")]
    public void Parse_InvalidHex_ThrowsArgumentException(string hex)
    {
        Assert.ThrowsAny<ArgumentException>(() => ImageColor.Parse(hex));
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => ImageColor.Parse(null));
    }

    [Fact]
    public void Transparent_HasZeroAlpha()
    {
        Assert.Equal(new ImageColor(0, 0, 0, 0), ImageColor.Transparent);
    }

    [Fact]
    public void ToOpaque_KeepsColorChannelsAndDropsAlpha()
    {
        Assert.Equal(new ImageColor(0, 255, 0, 255), new ImageColor(0, 255, 0, 128).ToOpaque());
        Assert.Equal(new ImageColor(0, 0, 0, 255), ImageColor.Transparent.ToOpaque());
    }
}
