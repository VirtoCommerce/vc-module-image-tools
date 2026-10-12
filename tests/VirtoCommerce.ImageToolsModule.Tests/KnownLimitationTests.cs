using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

/// <summary>
/// How the module behaves for the image types listed as known limitations: unsupported formats are
/// refused up front, and if one reaches the generator anyway it is skipped without writing anything.
/// </summary>
public class KnownLimitationTests
{
    private const string SourceFolder = "https://blob.test/catalog/";

    [Theory]
    [InlineData("gif")]
    [InlineData("tif")]
    [InlineData("tiff")]
    [InlineData("tga")]
    [InlineData("ppm")]
    [InlineData("pbm")]
    [InlineData("pgm")]
    [InlineData("qoi")]
    public async Task CanHandle_UnsupportedFormat_ReturnsFalse(string extension)
    {
        var handler = ThumbnailTestHost.CreateRasterHandler(new InMemoryBlobStorage().Provider);

        Assert.False(await handler.CanHandleAsync($"{SourceFolder}image.{extension}"));
    }

    [Theory]
    [InlineData("gif.gif")]
    [InlineData("tiff.tif")]
    [InlineData("tga.tga")]
    [InlineData("pbm.ppm")]
    [InlineData("qoi.qoi")]
    public async Task GenerateThumbnails_UnsupportedFormat_ReturnsNullAndWritesNothing(string fixture)
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        storage.Add(SourceFolder + fixture, TestImages.Read(fixture));
        var option = new ThumbnailOption { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 40 };
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        var result = await generator.GenerateThumbnailsAsync(SourceFolder + fixture, SourceFolder, [option], CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Empty(storage.WrittenUrls);
    }

    [Fact]
    public async Task GenerateThumbnails_AnimatedWebp_WritesStaticFirstFrame()
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        const string sourceUrl = SourceFolder + "webp-animated.webp";
        storage.Add(sourceUrl, TestImages.Read("webp-animated.webp"));
        var option = new ThumbnailOption { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 40 };
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        await generator.GenerateThumbnailsAsync(sourceUrl, SourceFolder, [option], CancellationToken.None);

        // Assert: first frame is red|blue, the second (dropped) frame is all green
        var thumbnail = storage.GetWritten(sourceUrl.GenerateThumbnailName("sm"));
        Assert.Equal(ImageProbe.Webp, ImageProbe.DetectFormat(thumbnail));
        Assert.False(ImageProbe.IsAnimatedWebp(thumbnail));
        var image = ImageProbe.Decode(thumbnail);
        ImageProbe.AssertColor(Rgba.Red, image[10, 10], 8, "first frame, left half");
        ImageProbe.AssertColor(Rgba.Blue, image[30, 10], 8, "first frame, right half");
    }
}
