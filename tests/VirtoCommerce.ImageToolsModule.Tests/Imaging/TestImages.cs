using System;
using System.IO;

namespace VirtoCommerce.ImageToolsModule.Tests.Imaging;

/// <summary>
/// Image fixtures under media/formats. Unless noted otherwise a fixture is 80x40 px,
/// left half red (#FF0000) and right half blue (#0000FF).
/// </summary>
internal static class TestImages
{
    public const int Width = 80;
    public const int Height = 40;

    public static byte[] Read(string fileName)
    {
        return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "media", "formats", fileName));
    }
}
