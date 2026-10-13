namespace VirtoCommerce.ImageToolsModule.Core.Imaging;

public readonly record struct ImageSize(int Width, int Height);

public readonly record struct ImageRectangle(int X, int Y, int Width, int Height);

/// <summary>
/// How to render a resized image: a canvas of <see cref="Canvas"/> size, with the whole source image
/// scaled into <see cref="Destination"/>. The destination may extend past the canvas (cropping)
/// or leave parts of it uncovered (padding).
/// </summary>
public readonly record struct ResizePlan(ImageSize Canvas, ImageRectangle Destination);
