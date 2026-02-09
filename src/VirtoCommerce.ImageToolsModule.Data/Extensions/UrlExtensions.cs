using System;
using System.IO;

namespace VirtoCommerce.ImageToolsModule.Data.Extensions
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Gets the file extension from a URL without the leading dot.
        /// Returns empty string for null, empty, or extensionless URLs.
        /// </summary>
        public static string GetFileExtensionWithoutDot(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            // Strip query string and fragment before extracting extension
            var queryIndex = url.IndexOfAny(['?', '#']);
            var path = queryIndex >= 0 ? url[..queryIndex] : url;

            var extension = Path.GetExtension(path);

            return string.IsNullOrEmpty(extension)
                ? string.Empty
                : extension.TrimStart('.');
        }

        /// <summary>
        /// Checks whether the URL has the specified file extension (case-insensitive, without leading dot).
        /// </summary>
        public static bool HasExtension(string url, string extension)
        {
            return GetFileExtensionWithoutDot(url)
                .Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
