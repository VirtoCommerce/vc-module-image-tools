namespace VirtoCommerce.ImageToolsModule.Tests.Imaging;

/// <summary>
/// Skip reasons for behaviour that ended with the move from ImageSharp to SkiaSharp.
/// Each limitation is also listed in README.md and pinned by <see cref="KnownLimitationTests"/>.
/// </summary>
internal static class KnownLimitations
{
    public const string Gif = "Known limitation: GIF is not supported (SkiaSharp cannot encode GIF)";
    public const string Tiff = "Known limitation: TIFF is not supported (SkiaSharp cannot decode or encode TIFF)";
    public const string Tga = "Known limitation: TGA is not supported (SkiaSharp cannot decode or encode TGA)";
    public const string Pbm = "Known limitation: PBM/PGM/PPM is not supported (SkiaSharp cannot decode or encode it)";
    public const string Qoi = "Known limitation: QOI is not supported (SkiaSharp cannot decode or encode QOI)";
}
