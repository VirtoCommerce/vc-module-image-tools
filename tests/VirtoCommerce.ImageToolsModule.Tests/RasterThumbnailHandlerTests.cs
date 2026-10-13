using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class RasterThumbnailHandlerTests
{
    public static IEnumerable<TheoryDataRow<string>> HandledExtensions()
    {
        yield return new("jpg");
        yield return new("jpeg");
        yield return new("jfif");
        yield return new("png");
        yield return new("webp");
        yield return new("bmp");
        yield return new("gif") { Skip = KnownLimitations.Gif };
        yield return new("tif") { Skip = KnownLimitations.Tiff };
        yield return new("tiff") { Skip = KnownLimitations.Tiff };
        yield return new("tga") { Skip = KnownLimitations.Tga };
        yield return new("ppm") { Skip = KnownLimitations.Pbm };
        yield return new("pbm") { Skip = KnownLimitations.Pbm };
        yield return new("pgm") { Skip = KnownLimitations.Pbm };
        yield return new("qoi") { Skip = KnownLimitations.Qoi };
    }

    [Theory]
    [MemberData(nameof(HandledExtensions))]
    public async Task CanHandle_RasterExtension_ReturnsTrue(string extension)
    {
        var handler = ThumbnailTestHost.CreateRasterHandler(new InMemoryBlobStorage().Provider);

        Assert.True(await handler.CanHandleAsync($"catalog/image.{extension}"));
        Assert.True(await handler.CanHandleAsync($"catalog/IMAGE.{extension.ToUpperInvariant()}"));
    }

    [Theory]
    [InlineData("catalog/image.svg")]
    [InlineData("catalog/image.heic")]
    [InlineData("catalog/image.avif")]
    [InlineData("catalog/document.pdf")]
    [InlineData("catalog/readme.txt")]
    [InlineData("catalog/no-extension")]
    public async Task CanHandle_NonRasterUrl_ReturnsFalse(string url)
    {
        var handler = ThumbnailTestHost.CreateRasterHandler(new InMemoryBlobStorage().Provider);

        Assert.False(await handler.CanHandleAsync(url));
    }

    [Fact]
    public void AllowedImageFormats_DefaultValues_AreStable()
    {
        // These strings are persisted in settings; renaming them breaks existing installations
        Assert.Equal(
            ["JPEG", "PNG", "Webp", "SVG"],
            ModuleConstants.Settings.General.AllowedImageFormats.AllowedValues);
    }
}
