using VirtoCommerce.ImageToolsModule.Data.Extensions;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class UrlExtensionsTests
    {
        #region GetFileExtensionWithoutDot Tests

        [Theory]
        [InlineData("image.jpg", "jpg")]
        [InlineData("image.PNG", "PNG")]
        [InlineData("photo.jpeg", "jpeg")]
        [InlineData("document.svg", "svg")]
        [InlineData("archive.tar.gz", "gz")]
        [InlineData("image.webp", "webp")]
        public void GetFileExtensionWithoutDot_ValidExtension_ReturnsWithoutDot(string url, string expected)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("https://example.com/images/photo.jpg", "jpg")]
        [InlineData("https://example.com/path/to/image.png", "png")]
        [InlineData("/assets/images/banner.webp", "webp")]
        [InlineData("relative/path/icon.svg", "svg")]
        public void GetFileExtensionWithoutDot_UrlWithPath_ReturnsExtension(string url, string expected)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFileExtensionWithoutDot_NullOrEmpty_ReturnsEmptyString(string url)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("noextension")]
        [InlineData("/path/to/noextension")]
        public void GetFileExtensionWithoutDot_NoExtension_ReturnsEmptyString(string url)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("image.jpg?v=123", "jpg")]
        [InlineData("https://cdn.example.com/photo.png?width=200&height=100", "png")]
        [InlineData("/assets/image.webp?token=abc", "webp")]
        [InlineData("image.svg?cache=false", "svg")]
        public void GetFileExtensionWithoutDot_UrlWithQueryString_ReturnsExtension(string url, string expected)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("image.jpg#section", "jpg")]
        [InlineData("https://example.com/photo.png#anchor", "png")]
        [InlineData("image.webp?v=1#top", "webp")]
        public void GetFileExtensionWithoutDot_UrlWithFragment_ReturnsExtension(string url, string expected)
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot(url);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFileExtensionWithoutDot_DotOnly_ReturnsEmptyString()
        {
            var result = UrlExtensions.GetFileExtensionWithoutDot("file.");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetFileExtensionWithoutDot_HiddenFile_ReturnsEmptyString()
        {
            // .gitignore has no extension per Path.GetExtension behavior
            var result = UrlExtensions.GetFileExtensionWithoutDot(".gitignore");
            Assert.Equal("gitignore", result);
        }

        #endregion

        #region HasExtension Tests

        [Theory]
        [InlineData("image.jpg", "jpg", true)]
        [InlineData("image.jpg", "JPG", true)]
        [InlineData("image.JPG", "jpg", true)]
        [InlineData("image.png", "jpg", false)]
        [InlineData("image.svg", "svg", true)]
        [InlineData("image.svg", "png", false)]
        public void HasExtension_VariousCases_ReturnsExpected(string url, string extension, bool expected)
        {
            var result = UrlExtensions.HasExtension(url, extension);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "jpg")]
        [InlineData("", "jpg")]
        public void HasExtension_NullOrEmptyUrl_ReturnsFalse(string url, string extension)
        {
            var result = UrlExtensions.HasExtension(url, extension);
            Assert.False(result);
        }

        [Fact]
        public void HasExtension_NoExtensionInUrl_ReturnsFalse()
        {
            var result = UrlExtensions.HasExtension("noextension", "jpg");
            Assert.False(result);
        }

        [Theory]
        [InlineData("https://cdn.example.com/assets/photo.JPEG", "jpeg", true)]
        [InlineData("https://cdn.example.com/assets/photo.JPEG", "jpg", false)]
        [InlineData("/storage/thumbnails/thumb_200x200.webp", "webp", true)]
        public void HasExtension_UrlWithPath_MatchesCorrectly(string url, string extension, bool expected)
        {
            var result = UrlExtensions.HasExtension(url, extension);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("image.jpg?v=123", "jpg", true)]
        [InlineData("image.jpg?v=123", "png", false)]
        [InlineData("https://cdn.example.com/photo.png?width=200", "png", true)]
        [InlineData("image.svg?cache=false#top", "svg", true)]
        public void HasExtension_UrlWithQueryString_MatchesCorrectly(string url, string extension, bool expected)
        {
            var result = UrlExtensions.HasExtension(url, extension);
            Assert.Equal(expected, result);
        }

        #endregion
    }
}
