using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.ImageToolsModule.Core.Imaging;

/// <summary>
/// A raster image format the module can read and write thumbnails for.
/// GIF, TIFF, TGA, PBM and QOI are not supported.
/// </summary>
public sealed class RasterImageFormat
{
    // Names are persisted in the AllowedImageFormats setting; do not rename
    public static readonly RasterImageFormat Jpeg = new("JPEG", "image/jpeg", ["jpg", "jpeg", "jfif"]);
    public static readonly RasterImageFormat Png = new("PNG", "image/png", ["png", "apng"]);
    public static readonly RasterImageFormat Webp = new("Webp", "image/webp", ["webp"]);
    public static readonly RasterImageFormat Bmp = new("BMP", "image/bmp", ["bmp", "bm", "dip"]);

    public static IReadOnlyList<RasterImageFormat> Supported { get; } = [Jpeg, Png, Webp, Bmp];

    private RasterImageFormat(string name, string mimeType, IReadOnlyList<string> fileExtensions)
    {
        Name = name;
        MimeType = mimeType;
        FileExtensions = fileExtensions;
    }

    public string Name { get; }

    public string MimeType { get; }

    /// <summary>
    /// File extensions without the leading dot, lower case.
    /// </summary>
    public IReadOnlyList<string> FileExtensions { get; }

    /// <summary>
    /// Finds the supported format for a file extension (without the dot, any case); null when unsupported.
    /// </summary>
    public static RasterImageFormat FindByExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return Supported.FirstOrDefault(format => format.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
    }

    public override string ToString() => Name;
}
