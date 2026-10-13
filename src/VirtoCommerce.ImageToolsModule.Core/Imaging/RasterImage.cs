using System;

namespace VirtoCommerce.ImageToolsModule.Core.Imaging;

/// <summary>
/// A decoded raster image. Implementations hold the pixels in the imaging library they are built on.
/// </summary>
public abstract class RasterImage : IDisposable
{
    protected RasterImage(RasterImageFormat format, bool isLossless)
    {
        Format = format ?? throw new ArgumentNullException(nameof(format));
        IsLossless = isLossless;
    }

    public abstract int Width { get; }

    public abstract int Height { get; }

    public ImageSize Size => new(Width, Height);

    /// <summary>
    /// False when every pixel is opaque by construction (no alpha channel); images derived from it keep that.
    /// </summary>
    public abstract bool HasAlpha { get; }

    /// <summary>
    /// The format the image was decoded from; thumbnails are written in the same format.
    /// </summary>
    public RasterImageFormat Format { get; }

    /// <summary>
    /// The source used lossless compression (lossless WebP); thumbnails keep it.
    /// </summary>
    public bool IsLossless { get; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);
}
