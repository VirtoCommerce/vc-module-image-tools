using System;
using System.Runtime.CompilerServices;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Imaging;

/// <summary>
/// Library-independent resize geometry. Single-precision math with half-to-even rounding,
/// so thumbnail sizes match those produced by earlier versions of the module.
/// </summary>
public static class ResizePlanner
{
    /// <summary>
    /// Scales the source to fit inside <paramref name="box"/>, keeping its aspect ratio.
    /// </summary>
    public static ResizePlan FitWithin(ImageSize source, ImageSize box)
    {
        EnsurePositive(box);

        var width = box.Width;
        var height = box.Height;
        var widthRatio = box.Width / (float)source.Width;
        var heightRatio = box.Height / (float)source.Height;

        if (source.Height / (float)source.Width < box.Height / (float)box.Width)
        {
            height = Round(source.Height * widthRatio);
        }
        else
        {
            width = Round(source.Width * heightRatio);
        }

        var size = new ImageSize(AtLeastOne(width), AtLeastOne(height));

        return new ResizePlan(size, new ImageRectangle(0, 0, size.Width, size.Height));
    }

    /// <summary>
    /// Scales the source to fit inside <paramref name="target"/> and centers it on a canvas of that size.
    /// </summary>
    public static ResizePlan Pad(ImageSize source, ImageSize target)
    {
        EnsurePositive(target);

        var widthRatio = target.Width / (float)source.Width;
        var heightRatio = target.Height / (float)source.Height;
        ImageRectangle destination;

        if (heightRatio < widthRatio)
        {
            var width = Round(source.Width * heightRatio);
            var x = Round((target.Width - (source.Width * heightRatio)) / 2f);
            destination = new ImageRectangle(x, 0, width, target.Height);
        }
        else
        {
            var height = Round(source.Height * widthRatio);
            var y = Round((target.Height - (source.Height * widthRatio)) / 2f);
            destination = new ImageRectangle(0, y, target.Width, height);
        }

        return new ResizePlan(target, destination);
    }

    /// <summary>
    /// Scales the source to cover <paramref name="target"/> and trims the excess, keeping the part at <paramref name="anchor"/>.
    /// </summary>
    public static ResizePlan Crop(ImageSize source, ImageSize target, AnchorPosition anchor)
    {
        EnsurePositive(target);

        var widthRatio = target.Width / (float)source.Width;
        var heightRatio = target.Height / (float)source.Height;
        ImageRectangle destination;

        if (heightRatio < widthRatio)
        {
            var overflow = target.Height - (source.Height * widthRatio);
            var y = anchor switch
            {
                AnchorPosition.TopLeft or AnchorPosition.TopCenter or AnchorPosition.TopRight => 0,
                AnchorPosition.BottomLeft or AnchorPosition.BottomCenter or AnchorPosition.BottomRight => Round(overflow),
                _ => Round(overflow / 2f),
            };
            destination = new ImageRectangle(0, y, target.Width, AtLeastOne((int)MathF.Ceiling(source.Height * widthRatio)));
        }
        else
        {
            var overflow = target.Width - (source.Width * heightRatio);
            var x = anchor switch
            {
                AnchorPosition.TopLeft or AnchorPosition.CenterLeft or AnchorPosition.BottomLeft => 0,
                AnchorPosition.TopRight or AnchorPosition.CenterRight or AnchorPosition.BottomRight => Round(overflow),
                _ => Round(overflow / 2f),
            };
            destination = new ImageRectangle(x, 0, AtLeastOne((int)MathF.Ceiling(source.Width * heightRatio)), target.Height);
        }

        return new ResizePlan(target, destination);
    }

    /// <summary>
    /// Scales the source by <paramref name="percent"/>, truncating to whole pixels.
    /// </summary>
    public static ResizePlan Scale(ImageSize source, int percent)
    {
        var factor = percent / 100f;
        var size = new ImageSize(AtLeastOne((int)(source.Width * factor)), AtLeastOne((int)(source.Height * factor)));

        return new ResizePlan(size, new ImageRectangle(0, 0, size.Width, size.Height));
    }

    private static int Round(float value) => (int)MathF.Round(value);

    private static int AtLeastOne(int value) => Math.Max(1, value);

    private static void EnsurePositive(ImageSize size, [CallerArgumentExpression(nameof(size))] string paramName = null)
    {
        if (size.Width <= 0 || size.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, size, "Width and height must be positive.");
        }
    }
}
