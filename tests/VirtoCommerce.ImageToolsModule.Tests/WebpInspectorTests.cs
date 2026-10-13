using VirtoCommerce.ImageToolsModule.Data.Imaging;
using VirtoCommerce.ImageToolsModule.Tests.Imaging;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests;

public class WebpInspectorTests
{
    [Theory]
    [InlineData("webp-lossless-alpha.webp", true)]
    [InlineData("webp-animated.webp", true)]
    [InlineData("webp-lossy.webp", false)]
    [InlineData("png-rgb.png", false)]
    [InlineData("corrupt.jpg", false)]
    public void IsLossless_DetectsLosslessBitstream(string fixture, bool expected)
    {
        Assert.Equal(expected, WebpInspector.IsLossless(TestImages.Read(fixture)));
    }

    [Fact]
    public void IsLossless_TruncatedHeader_ReturnsFalse()
    {
        Assert.False(WebpInspector.IsLossless("RIFF\0\0\0\0WEBPVP8"u8));
    }
}
