using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

/// <summary>
/// End-to-end behaviour of each resize method (PNG in, PNG out, so pixels are exact).
/// Source images: png-rgb.png is 80x40 red|blue; stripes-vertical.png is 120x40 red|green|blue side by side;
/// stripes-horizontal.png is 40x120 red/green/blue stacked.
/// </summary>
public class RasterResizeCharacterizationTests
{
    private const int Tolerance = 8;

    [Fact]
    public async Task FixedWidth_KeepsAspectRatio()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedWidth, Width = 20 });

        Assert.Equal((20, 10), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Red, image[4, 5], Tolerance, "left half");
        ImageProbe.AssertColor(Rgba.Blue, image[15, 5], Tolerance, "right half");
    }

    [Fact]
    public async Task FixedHeight_KeepsAspectRatio()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedHeight, Height = 10 });

        Assert.Equal((20, 10), (image.Width, image.Height));
    }

    [Fact]
    public async Task FixedWidth_LargerThanSource_DoesNotUpscale()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedWidth, Width = 160 });

        Assert.Equal((80, 40), (image.Width, image.Height));
    }

    [Fact]
    public async Task FixedHeight_LargerThanSource_DoesNotUpscale()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedHeight, Height = 80 });

        Assert.Equal((80, 40), (image.Width, image.Height));
    }

    [Theory]
    [InlineData(33, 16)] // 16.5
    [InlineData(25, 12)] // 12.5
    [InlineData(27, 14)] // 13.5
    [InlineData(3, 2)] // 1.5
    [InlineData(1, 1)] // 0.5 never collapses to 0
    [InlineData(50, 25)]
    public async Task FixedWidth_OddTargetWidth_RoundsHeightHalfToEven(int width, int expectedHeight)
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedWidth, Width = width });

        Assert.Equal((width, expectedHeight), (image.Width, image.Height));
    }

    [Fact]
    public async Task FixedSize_DifferentAspectRatio_PadsWithBackgroundColor()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40, BackgroundColor = "#00FF00" });

        Assert.Equal((40, 40), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Green, image[20, 2], Tolerance, "top padding");
        ImageProbe.AssertColor(Rgba.Green, image[20, 37], Tolerance, "bottom padding");
        ImageProbe.AssertColor(Rgba.Red, image[8, 20], Tolerance, "image left half");
        ImageProbe.AssertColor(Rgba.Blue, image[32, 20], Tolerance, "image right half");
    }

    [Fact]
    public async Task FixedSize_SourceWithAlphaAndNoBackgroundColor_PadsTransparent()
    {
        var image = await GenerateAsync("png-rgba.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40 });

        ImageProbe.AssertColor(Rgba.Transparent, image[20, 2], Tolerance, "top padding");
        ImageProbe.AssertColor(Rgba.Red, image[8, 20], Tolerance, "image left half");
    }

    [Fact]
    public async Task FixedSize_OpaquePngWithoutBackgroundColor_PadsBlack()
    {
        // The thumbnail keeps the source's color type: an RGB PNG has no alpha channel, so transparent padding turns black
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40 });

        ImageProbe.AssertColor(Rgba.Black, image[20, 2], Tolerance, "top padding");
        ImageProbe.AssertColor(Rgba.Red, image[8, 20], Tolerance, "image left half");
    }

    [Fact]
    public async Task FixedSize_OpaquePngWithTranslucentBackgroundColor_DropsAlpha()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40, BackgroundColor = "#00FF0080" });

        ImageProbe.AssertColor(Rgba.Green, image[20, 2], Tolerance, "top padding");
    }

    [Fact]
    public async Task FixedSize_JpegWithoutBackgroundColor_PadsBlack()
    {
        var image = await GenerateAsync("jpeg-baseline.jpg", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40, JpegQuality = JpegQuality.VeryHigh });

        ImageProbe.AssertColor(Rgba.Black, image[20, 2], 40, "top padding");
        ImageProbe.AssertColor(Rgba.Red, image[8, 20], 40, "image left half");
    }

    [Fact]
    public async Task FixedSize_LargerThanSource_UpscalesToFit()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 160, Height = 160, BackgroundColor = "#00FF00" });

        // 80x40 scaled x2 to 160x80, centered vertically (rows 40..119)
        Assert.Equal((160, 160), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Green, image[80, 20], Tolerance, "top padding");
        ImageProbe.AssertColor(Rgba.Red, image[40, 80], Tolerance, "image left half");
        ImageProbe.AssertColor(Rgba.Blue, image[120, 80], Tolerance, "image right half");
    }

    [Fact]
    public async Task FixedSize_NoWidthOrHeight_UsesSourceSize()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize });

        Assert.Equal((80, 40), (image.Width, image.Height));
    }

    [Theory]
    [InlineData("#00FF00", "#00FF00FF")]
    [InlineData("00FF00", "#00FF00FF")]
    [InlineData("#0F0", "#00FF00FF")]
    [InlineData("#00FF0080", "#00FF0080")]
    [InlineData("", "#00000000")]
    public async Task FixedSize_BackgroundColorFormats_AreParsed(string backgroundColor, string expectedPadding)
    {
        var image = await GenerateAsync("png-rgba.png",new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40, BackgroundColor = backgroundColor });

        ImageProbe.AssertColor(Rgba.Parse(expectedPadding), image[20, 2], Tolerance, "padding");
    }

    [Fact]
    public async Task FixedSize_InvalidBackgroundColor_Throws()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 40, Height = 40, BackgroundColor = "not-a-color" }));
    }

    [Theory]
    [InlineData(AnchorPosition.TopLeft, "#FF0000")]
    [InlineData(AnchorPosition.CenterLeft, "#FF0000")]
    [InlineData(AnchorPosition.BottomLeft, "#FF0000")]
    [InlineData(AnchorPosition.TopCenter, "#00FF00")]
    [InlineData(AnchorPosition.Center, "#00FF00")]
    [InlineData(AnchorPosition.BottomCenter, "#00FF00")]
    [InlineData(AnchorPosition.TopRight, "#0000FF")]
    [InlineData(AnchorPosition.CenterRight, "#0000FF")]
    [InlineData(AnchorPosition.BottomRight, "#0000FF")]
    public async Task Crop_WideSource_HorizontalAnchorSelectsStripe(AnchorPosition anchor, string expectedColor)
    {
        var image = await GenerateAsync("stripes-vertical.png", new ThumbnailOption { ResizeMethod = ResizeMethod.Crop, Width = 40, Height = 40, AnchorPosition = anchor });

        Assert.Equal((40, 40), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Parse(expectedColor), image[20, 20], Tolerance, $"center of crop anchored {anchor}");
        ImageProbe.AssertColor(Rgba.Parse(expectedColor), image[3, 3], Tolerance, $"corner of crop anchored {anchor}");
    }

    [Theory]
    [InlineData(AnchorPosition.TopLeft, "#FF0000")]
    [InlineData(AnchorPosition.TopCenter, "#FF0000")]
    [InlineData(AnchorPosition.TopRight, "#FF0000")]
    [InlineData(AnchorPosition.CenterLeft, "#00FF00")]
    [InlineData(AnchorPosition.Center, "#00FF00")]
    [InlineData(AnchorPosition.CenterRight, "#00FF00")]
    [InlineData(AnchorPosition.BottomLeft, "#0000FF")]
    [InlineData(AnchorPosition.BottomCenter, "#0000FF")]
    [InlineData(AnchorPosition.BottomRight, "#0000FF")]
    public async Task Crop_TallSource_VerticalAnchorSelectsStripe(AnchorPosition anchor, string expectedColor)
    {
        var image = await GenerateAsync("stripes-horizontal.png", new ThumbnailOption { ResizeMethod = ResizeMethod.Crop, Width = 40, Height = 40, AnchorPosition = anchor });

        Assert.Equal((40, 40), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Parse(expectedColor), image[20, 20], Tolerance, $"center of crop anchored {anchor}");
        ImageProbe.AssertColor(Rgba.Parse(expectedColor), image[3, 3], Tolerance, $"corner of crop anchored {anchor}");
    }

    [Fact]
    public async Task Crop_LargerThanSource_UpscalesToCover()
    {
        var image = await GenerateAsync("png-rgb.png", new ThumbnailOption { ResizeMethod = ResizeMethod.Crop, Width = 160, Height = 160, AnchorPosition = AnchorPosition.Center });

        // 80x40 scaled x4 to 320x160, centered horizontally: output shows source columns 20..59
        Assert.Equal((160, 160), (image.Width, image.Height));
        ImageProbe.AssertColor(Rgba.Red, image[40, 80], Tolerance, "left of center");
        ImageProbe.AssertColor(Rgba.Blue, image[120, 80], Tolerance, "right of center");
    }

    [Fact]
    public async Task Downscale_FineDetail_IsAveragedNotAliased()
    {
        // 512x512 one-pixel black/white checkerboard down to 16x16 must come out uniformly mid-gray
        var image = await GenerateAsync("checkerboard-512.png", new ThumbnailOption { ResizeMethod = ResizeMethod.FixedSize, Width = 16, Height = 16 });

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                ImageProbe.AssertColor(new Rgba(128, 128, 128, 255), image[x, y], 40, $"pixel ({x},{y})");
            }
        }
    }

    private static async Task<ProbedImage> GenerateAsync(string fixture, ThumbnailOption option)
    {
        var storage = new InMemoryBlobStorage();
        var sourceUrl = "https://blob.test/catalog/" + fixture;
        storage.Add(sourceUrl, TestImages.Read(fixture));
        option.FileSuffix = "thumb";
        var generator = ThumbnailTestHost.CreateGenerator(storage.Provider);

        var result = await generator.GenerateThumbnailsAsync(sourceUrl, "https://blob.test/catalog/", [option], CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        var thumbnail = storage.GetWritten(sourceUrl.GenerateThumbnailName("thumb"));
        Assert.NotNull(thumbnail);

        return ImageProbe.Decode(thumbnail);
    }
}
