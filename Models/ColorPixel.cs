using System.Runtime.InteropServices;

namespace ColorChanger.Models;

[StructLayout(LayoutKind.Sequential)]
internal struct ColorPixel(byte r, byte g, byte b, byte a)
{
    internal static readonly ColorPixel Transparent = new ColorPixel(0, 0, 0, 0);
    internal static readonly ColorPixel Black = new ColorPixel(0, 0, 0, 255);
    internal static readonly ColorPixel White = new ColorPixel(255, 255, 255, 255);

    internal byte B = b;
    internal byte G = g;
    internal byte R = r;
    internal byte A = a;

    internal readonly bool IsTransparent => A == 0;

    internal readonly bool Equals(Color other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public static ColorPixel operator +(ColorPixel pixel, ColorDifference colorDiff)
    {
        pixel.R = ClampToByte(pixel.R + colorDiff.DiffR);
        pixel.G = ClampToByte(pixel.G + colorDiff.DiffG);
        pixel.B = ClampToByte(pixel.B + colorDiff.DiffB);

        return pixel;
    }

    internal static byte ClampToByte(int value)
        => (byte)Math.Clamp(value, 0, 255);
}
