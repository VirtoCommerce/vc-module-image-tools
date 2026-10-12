using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

/// <summary>
/// End-to-end behaviour of raster thumbnail generation per image type:
/// every supported source format produces a thumbnail in the same format with the expected size and colors.
/// </summary>
public class RasterFormatCharacterizationTests
{
    private const string SourceFolder = "https://blob.test/catalog/";

    public static IEnumerable<TheoryDataRow<string, string, string, string, int>> SourceFormats()
    {
        // fixture, expected output format, left-half color, right-half color, color tolerance
        yield return new("jpeg-baseline.jpg", ImageProbe.Jpeg, "#FF0000", "#0000FF", 40);
        yield return new("jpeg-baseline.jpeg", ImageProbe.Jpeg, "#FF0000", "#0000FF", 40);
        yield return new("jpeg-baseline.jfif", ImageProbe.Jpeg, "#FF0000", "#0000FF", 40);
        yield return new("jpeg-progressive.jpg", ImageProbe.Jpeg, "#FF0000", "#0000FF", 40);
        yield return new("jpeg-grayscale.jpg", ImageProbe.Jpeg, "#000000", "#FFFFFF", 40);
        yield return new("jpeg-cmyk.jpg", ImageProbe.Jpeg, "#FF0000", "#0000FF", 60);
        yield return new("png-rgb.png", ImageProbe.Png, "#FF0000", "#0000FF", 8);
        yield return new("png-indexed.png", ImageProbe.Png, "#FF0000", "#0000FF", 8);
        yield return new("png-16bit.png", ImageProbe.Png, "#FF0000", "#0000FF", 8);
        yield return new("png-grayscale.png", ImageProbe.Png, "#000000", "#FFFFFF", 8);
        yield return new("png-rgba.png", ImageProbe.Png, "#FF0000", "#00000000", 8);
        yield return new("webp-lossy.webp", ImageProbe.Webp, "#FF0000", "#0000FF", 40);
        yield return new("webp-lossless-alpha.webp", ImageProbe.Webp, "#FF0000", "#00000000", 8);
        yield return new("bmp-24.bmp", ImageProbe.Bmp, "#FF0000", "#0000FF", 8);
        yield return new("bmp-32.bmp", ImageProbe.Bmp, "#FF0000", "#0000FF", 8);
        yield return new("gif.gif", ImageProbe.Gif, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Gif };
        yield return new("tiff.tif", ImageProbe.Tiff, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Tiff };
        yield return new("tiff.tiff", ImageProbe.Tiff, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Tiff };
        yield return new("tga.tga", ImageProbe.Tga, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Tga };
        yield return new("pbm.ppm", ImageProbe.Pbm, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Pbm };
        yield return new("qoi.qoi", ImageProbe.Qoi, "#FF0000", "#0000FF", 8) { Skip = KnownLimitations.Qoi };
    }

    [Theory]
    [MemberData(nameof(SourceFormats))]
    public async Task GenerateThumbnails_SourceFormat_ThumbnailKeepsFormatSizeAndColors(string fixture, string expectedFormat, string leftColor, string rightColor, int tolerance)
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        var sourceUrl = SourceFolder + fixture;
        storage.Add(sourceUrl, TestImages.Read(fixture));
        var option = new ThumbnailOption { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 40, JpegQuality = JpegQuality.VeryHigh };
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        var result = await generator.GenerateThumbnailsAsync(sourceUrl, SourceFolder, [option], CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var thumbnailUrl = sourceUrl.GenerateThumbnailName("sm");
        Assert.Equal([thumbnailUrl], result.GeneratedThumbnails);

        var thumbnail = storage.GetWritten(thumbnailUrl);
        Assert.NotNull(thumbnail);
        Assert.Equal(expectedFormat, ImageProbe.DetectFormat(thumbnail));

        if (ImageProbe.CanDecode(expectedFormat))
        {
            var image = ImageProbe.Decode(thumbnail);
            Assert.Equal((40, 20), (image.Width, image.Height));
            ImageProbe.AssertColor(Rgba.Parse(leftColor), image[10, 10], tolerance, "left half");
            ImageProbe.AssertColor(Rgba.Parse(rightColor), image[30, 10], tolerance, "right half");
        }
    }

    [Fact]
    public async Task GenerateThumbnails_CorruptImage_ReturnsNullAndWritesNothing()
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        const string sourceUrl = SourceFolder + "corrupt.jpg";
        storage.Add(sourceUrl, TestImages.Read("corrupt.jpg"));
        var option = new ThumbnailOption { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 40 };
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        var result = await generator.GenerateThumbnailsAsync(sourceUrl, SourceFolder, [option], CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Empty(storage.WrittenUrls);
    }

    [Fact]
    public async Task GenerateThumbnails_MissingBlob_ReturnsNullAndWritesNothing()
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        var option = new ThumbnailOption { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 40 };
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        var result = await generator.GenerateThumbnailsAsync(SourceFolder + "missing.png", SourceFolder, [option], CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Empty(storage.WrittenUrls);
    }

    [Fact]
    public async Task GenerateThumbnails_SeveralOptions_OneThumbnailPerOption()
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        const string sourceUrl = SourceFolder + "png-rgb.png";
        storage.Add(sourceUrl, TestImages.Read("png-rgb.png"));
        ThumbnailOption[] options =
        [
            new() { FileSuffix = "sm", ResizeMethod = ResizeMethod.FixedWidth, Width = 20 },
            new() { FileSuffix = "md", ResizeMethod = ResizeMethod.FixedSize, Width = 30, Height = 30 },
        ];
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        var result = await generator.GenerateThumbnailsAsync(sourceUrl, SourceFolder, options, CancellationToken.None);

        // Assert
        Assert.Equal([SourceFolder + "png-rgb_sm.png", SourceFolder + "png-rgb_md.png"], result.GeneratedThumbnails);
        Assert.Equal((20, 10), Size(storage.GetWritten(SourceFolder + "png-rgb_sm.png")));
        Assert.Equal((30, 30), Size(storage.GetWritten(SourceFolder + "png-rgb_md.png")));
    }

    [Fact]
    public async Task GenerateThumbnails_JpegQuality_HigherQualityProducesLargerFile()
    {
        // Arrange
        var storage = new InMemoryBlobStorage();
        const string sourceUrl = SourceFolder + "jpeg-noise.jpg";
        storage.Add(sourceUrl, TestImages.Read("jpeg-noise.jpg"));
        ThumbnailOption[] options =
        [
            new() { FileSuffix = "low", ResizeMethod = ResizeMethod.FixedSize, Width = 128, Height = 128, JpegQuality = JpegQuality.Low },
            new() { FileSuffix = "high", ResizeMethod = ResizeMethod.FixedSize, Width = 128, Height = 128, JpegQuality = JpegQuality.VeryHigh },
        ];
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        // Act
        await generator.GenerateThumbnailsAsync(sourceUrl, SourceFolder, options, CancellationToken.None);

        // Assert
        var low = storage.GetWritten(SourceFolder + "jpeg-noise_low.jpg");
        var high = storage.GetWritten(SourceFolder + "jpeg-noise_high.jpg");
        Assert.Equal(ImageProbe.Jpeg, ImageProbe.DetectFormat(low));
        Assert.True(high.Length > low.Length * 1.3, $"VeryHigh ({high.Length} bytes) should be clearly larger than Low ({low.Length} bytes)");
    }

    private static (int Width, int Height) Size(byte[] content)
    {
        var image = ImageProbe.Decode(content);
        return (image.Width, image.Height);
    }
}
