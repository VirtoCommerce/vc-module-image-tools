using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class RasterImageFormatTests
{
    [Fact]
    public void Supported_FormatNames_MatchPersistedSettingValues()
    {
        Assert.Equal(["JPEG", "PNG", "Webp", "BMP"], RasterImageFormat.Supported.Select(x => x.Name));
    }

    [Theory]
    [InlineData("jpg", "JPEG")]
    [InlineData("JPEG", "JPEG")]
    [InlineData("jfif", "JPEG")]
    [InlineData("png", "PNG")]
    [InlineData("apng", "PNG")]
    [InlineData("webp", "Webp")]
    [InlineData("bmp", "BMP")]
    [InlineData("bm", "BMP")]
    [InlineData("dip", "BMP")]
    public void FindByExtension_SupportedExtension_ReturnsFormat(string extension, string expectedName)
    {
        Assert.Equal(expectedName, RasterImageFormat.FindByExtension(extension)?.Name);
    }

    [Theory]
    [InlineData("gif")]
    [InlineData("tif")]
    [InlineData("tiff")]
    [InlineData("tga")]
    [InlineData("ppm")]
    [InlineData("qoi")]
    [InlineData("svg")]
    [InlineData("")]
    [InlineData(null)]
    public void FindByExtension_UnsupportedExtension_ReturnsNull(string extension)
    {
        Assert.Null(RasterImageFormat.FindByExtension(extension));
    }
}
