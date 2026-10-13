using System;
using System.Globalization;

namespace VirtoCommerce.ImageToolsModule.Core.Imaging;

/// <summary>
/// An unpremultiplied 8-bit RGBA color.
/// </summary>
public readonly record struct ImageColor(byte R, byte G, byte B, byte A)
{
    public static readonly ImageColor Transparent = new(0, 0, 0, 0);

    /// <summary>
    /// Parses a hexadecimal color: RGB, RGBA, RRGGBB or RRGGBBAA, with or without a leading '#'.
    /// </summary>
    /// <exception cref="ArgumentException">The value is not a hexadecimal color in one of those forms.</exception>
    public static ImageColor Parse(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);

        var digits = hex.StartsWith('#') ? hex[1..] : hex;
        var rgba = digits.Length switch
        {
            8 => digits,
            6 => digits + "FF",
            3 => $"{digits[0]}{digits[0]}{digits[1]}{digits[1]}{digits[2]}{digits[2]}FF",
            4 => $"{digits[0]}{digits[0]}{digits[1]}{digits[1]}{digits[2]}{digits[2]}{digits[3]}{digits[3]}",
            _ => null,
        };

        if (rgba is null || !uint.TryParse(rgba, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException($"'{hex}' is not a hexadecimal color (RGB, RGBA, RRGGBB or RRGGBBAA).", nameof(hex));
        }

        return new ImageColor((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    /// <summary>
    /// The same color with full opacity, as written to an image without an alpha channel.
    /// </summary>
    public ImageColor ToOpaque() => this with { A = 255 };

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";
}
