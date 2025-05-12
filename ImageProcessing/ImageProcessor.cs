using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.ImageProcessing;

internal class ImageProcessor
{
    private readonly int _width;
    private readonly int _height;
    private readonly ColorDifference _colorOffset;
    private BalanceModeConfiguration _balanceModeConfiguration = new();
    private bool _isBalanceMode = false;

    internal ImageProcessor(Size bitmapSize, ColorDifference colorDifference)
    {
        _width = bitmapSize.Width;
        _height = bitmapSize.Height;
        _colorOffset = colorDifference;
    }

    /// <summary>
    /// バランスモード用の設定を適用する
    /// </summary>
    /// <param name="balanceModeConfiguration"></param>
    internal void SetBalanceSettings(BalanceModeConfiguration balanceModeConfiguration)
    {
        _balanceModeConfiguration = balanceModeConfiguration;
        _isBalanceMode = true;
    }

    private ColorPixel ProcessPixel(ColorPixel source)
    {
        if (source.IsTransparent) return source;

        if (_isBalanceMode)
            return ColorUtils.BalanceColorAdjustment(source, _colorOffset, _balanceModeConfiguration);

        return source + _colorOffset;
    }

    internal void ProcessAllPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> target)
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                int index = PixelUtils.GetPixelIndex(x, y, _width);
                target[index] = ProcessPixel(source[index]);
            }
        }
    }

    internal void ProcessAllPreviewPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> target,
        (float ratioX, float ratioY) ratios,
        Size previewSize)
    {
        for (int y = 0; y < previewSize.Height; y++)
        {
            for (int x = 0; x < previewSize.Width; x++)
            {
                int sourceX = (int)(x * ratios.ratioX);
                int sourceY = (int)(y * ratios.ratioY);
                int sourceIndex = PixelUtils.GetPixelIndex(sourceX, sourceY, _width);
                int targetIndex = PixelUtils.GetPixelIndex(x, y, previewSize.Width);

                target[targetIndex] = ProcessPixel(source[sourceIndex]);
            }
        }
    }

    internal void ProcessSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> target,
        BitArray selectedPoints)
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal void ProcessInverseSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> raw,
        BitArray selectedPoints)
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = new ColorPixel(
                raw[i].R,
                raw[i].G,
                raw[i].B,
                source[i].A
            );
        }
    }

    internal void ProcessTransparentSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> trans,
        BitArray selectedPoints)
    {
        Span<ColorPixel> target = !trans.IsEmpty ? trans : source;

        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal void ProcessTransparentAndInversePixels(
        Span<ColorPixel> source,
        BitArray selectedPoints)
    {
        ProcessAllPixels(source, source);

        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = ColorUtils.TransparentPixel;
        }
    }

    internal static void ChangeSelectedPixelsColor(
        Span<ColorPixel> source,
        BitArray selectedPoints,
        ColorPixel color)
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = color;
        }
    }
}
