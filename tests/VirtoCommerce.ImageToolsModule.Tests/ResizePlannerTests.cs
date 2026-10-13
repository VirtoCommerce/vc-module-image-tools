using System;
using VirtoCommerce.ImageToolsModule.Core.Imaging;
using VirtoCommerce.ImageToolsModule.Core.Models;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class ResizePlannerTests
{
    private static readonly ImageSize Source = new(80, 40);

    [Theory]
    [InlineData(20, 40, 20, 10)] // fixed width
    [InlineData(80, 10, 20, 10)] // fixed height
    [InlineData(160, 40, 80, 40)] // wider box limited by the source height: no upscale
    [InlineData(160, 160, 160, 80)] // box larger in both dimensions: upscales
    [InlineData(33, 40, 33, 16)] // 16.5 rounds half to even
    [InlineData(25, 40, 25, 12)] // 12.5
    [InlineData(27, 40, 27, 14)] // 13.5
    [InlineData(1, 40, 1, 1)] // never collapses to zero
    public void FitWithin_KeepsAspectRatioInsideBox(int boxWidth, int boxHeight, int expectedWidth, int expectedHeight)
    {
        var plan = ResizePlanner.FitWithin(Source, new ImageSize(boxWidth, boxHeight));

        Assert.Equal(new ImageSize(expectedWidth, expectedHeight), plan.Canvas);
        Assert.Equal(new ImageRectangle(0, 0, expectedWidth, expectedHeight), plan.Destination);
    }

    [Theory]
    [InlineData(80, 40, 40, 40, 0, 10, 40, 20)] // wide source: bars top and bottom
    [InlineData(40, 80, 40, 40, 10, 0, 20, 40)] // tall source: bars left and right
    [InlineData(80, 40, 160, 160, 0, 40, 160, 80)] // upscales to fit
    [InlineData(80, 40, 80, 40, 0, 0, 80, 40)] // same aspect ratio: no bars
    [InlineData(80, 40, 41, 41, 0, 10, 41, 20)] // (41 - 20.5) / 2 = 10.25
    [InlineData(1920, 60, 16, 16, 0, 8, 16, 1)] // fitted height 0.5 is kept at one pixel
    [InlineData(60, 1920, 16, 16, 8, 0, 1, 16)] // fitted width 0.5 is kept at one pixel
    [InlineData(4000, 100, 20, 20, 0, 10, 20, 1)]
    [InlineData(80, 3, 40, 40, 0, 19, 40, 2)]
    public void Pad_FitsInsideTargetCentered(int sourceWidth, int sourceHeight, int width, int height, int x, int y, int destinationWidth, int destinationHeight)
    {
        var plan = ResizePlanner.Pad(new ImageSize(sourceWidth, sourceHeight), new ImageSize(width, height));

        Assert.Equal(new ImageSize(width, height), plan.Canvas);
        Assert.Equal(new ImageRectangle(x, y, destinationWidth, destinationHeight), plan.Destination);
    }

    [Theory]
    [InlineData(AnchorPosition.TopLeft, 0)]
    [InlineData(AnchorPosition.CenterLeft, 0)]
    [InlineData(AnchorPosition.BottomLeft, 0)]
    [InlineData(AnchorPosition.TopCenter, -40)]
    [InlineData(AnchorPosition.Center, -40)]
    [InlineData(AnchorPosition.BottomCenter, -40)]
    [InlineData(AnchorPosition.TopRight, -80)]
    [InlineData(AnchorPosition.CenterRight, -80)]
    [InlineData(AnchorPosition.BottomRight, -80)]
    public void Crop_WideSource_AnchorPicksHorizontalOffset(AnchorPosition anchor, int expectedX)
    {
        var plan = ResizePlanner.Crop(new ImageSize(120, 40), new ImageSize(40, 40), anchor);

        Assert.Equal(new ImageSize(40, 40), plan.Canvas);
        Assert.Equal(new ImageRectangle(expectedX, 0, 120, 40), plan.Destination);
    }

    [Theory]
    [InlineData(AnchorPosition.TopLeft, 0)]
    [InlineData(AnchorPosition.TopCenter, 0)]
    [InlineData(AnchorPosition.TopRight, 0)]
    [InlineData(AnchorPosition.CenterLeft, -40)]
    [InlineData(AnchorPosition.Center, -40)]
    [InlineData(AnchorPosition.CenterRight, -40)]
    [InlineData(AnchorPosition.BottomLeft, -80)]
    [InlineData(AnchorPosition.BottomCenter, -80)]
    [InlineData(AnchorPosition.BottomRight, -80)]
    public void Crop_TallSource_AnchorPicksVerticalOffset(AnchorPosition anchor, int expectedY)
    {
        var plan = ResizePlanner.Crop(new ImageSize(40, 120), new ImageSize(40, 40), anchor);

        Assert.Equal(new ImageSize(40, 40), plan.Canvas);
        Assert.Equal(new ImageRectangle(0, expectedY, 40, 120), plan.Destination);
    }

    [Fact]
    public void Crop_LargerThanSource_UpscalesToCover()
    {
        var plan = ResizePlanner.Crop(Source, new ImageSize(160, 160), AnchorPosition.Center);

        Assert.Equal(new ImageSize(160, 160), plan.Canvas);
        Assert.Equal(new ImageRectangle(-80, 0, 320, 160), plan.Destination);
    }

    [Fact]
    public void Crop_FractionalCover_RoundsCoveringSizeUp()
    {
        // 80x40 covering 30x30: scale 0.75 -> 60x30 exactly; 80x40 covering 31x31: 62x31
        var plan = ResizePlanner.Crop(Source, new ImageSize(31, 31), AnchorPosition.Center);

        Assert.Equal(new ImageRectangle(-16, 0, 62, 31), plan.Destination);
    }

    [Theory]
    [InlineData(10, 100, 20, 2, 20)]
    [InlineData(80, 40, 50, 40, 20)]
    [InlineData(80, 40, 1, 1, 1)] // 0.8 x 0.4 truncates to zero, kept at one pixel
    public void Scale_ByPercent_TruncatesSize(int width, int height, int percent, int expectedWidth, int expectedHeight)
    {
        var plan = ResizePlanner.Scale(new ImageSize(width, height), percent);

        Assert.Equal(new ImageSize(expectedWidth, expectedHeight), plan.Canvas);
        Assert.Equal(new ImageRectangle(0, 0, expectedWidth, expectedHeight), plan.Destination);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    public void Plans_NonPositiveTarget_Throw(int width, int height)
    {
        Assert.Equal("box", Assert.Throws<ArgumentOutOfRangeException>(() => ResizePlanner.FitWithin(Source, new ImageSize(width, height))).ParamName);
        Assert.Equal("target", Assert.Throws<ArgumentOutOfRangeException>(() => ResizePlanner.Pad(Source, new ImageSize(width, height))).ParamName);
        Assert.Equal("target", Assert.Throws<ArgumentOutOfRangeException>(() => ResizePlanner.Crop(Source, new ImageSize(width, height), AnchorPosition.Center)).ParamName);
    }
}
