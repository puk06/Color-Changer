using System.Collections;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// ピクセル配列に対して色変換を適用する
/// </summary>
internal class ImageProcessor(ColorDifference colorDifference, BalanceModeConfiguration? balance, AdvancedColorConfiguration? advanced)
{
    private ColorPixel ProcessPixel(ColorPixel source)
    {
        if (source.IsTransparent) return source;

        if (balance != null)
        {
            ColorAdjuster.BalanceColorAdjustment(ref source, colorDifference, balance);
        }
        else
        {
            source += colorDifference;
        }

        if (advanced is { Enabled: true })
            ColorAdjuster.AdvancedColorAdjustment(ref source, advanced);

        return source;
    }

    internal void ProcessAllPixels(Span<ColorPixel> source, Span<ColorPixel> target)
    {
        for (var i = 0; i < source.Length; i++)
        {
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal void ProcessSelectedPixels(Span<ColorPixel> source, Span<ColorPixel> target, BitArray selectedPoints)
    {
        for (var i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal void ProcessTransparentAndInversePixels(Span<ColorPixel> source, BitArray selectedPoints)
    {
        ProcessAllPixels(source, source);

        for (var i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = ColorPixel.Transparent;
        }
    }

    internal static void ProcessInverseSelectedPixels(Span<ColorPixel> source, Span<ColorPixel> raw, BitArray selectedPoints)
    {
        for (var i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = raw[i];
        }
    }

    internal static void ProcessMaskImage(Span<ColorPixel> source, BitArray selectedPoints, bool inverse)
    {
        var selectedColor = inverse ? ColorPixel.White : ColorPixel.Black;
        var unselectedColor = inverse ? ColorPixel.Black : ColorPixel.White;

        for (var i = 0; i < selectedPoints.Length; i++)
        {
            source[i] = selectedPoints[i] ? selectedColor : unselectedColor;
        }
    }

    internal static void ChangeSelectedPixelsColor(Span<ColorPixel> target, BitArray selectedPoints, ColorPixel color)
    {
        for (var i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            target[i] = color;
        }
    }
}
