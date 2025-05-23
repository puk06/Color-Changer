using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.ImageProcessing;

internal class ImageProcessor
{
    private readonly int _width;
    private readonly ColorDifference _colorOffset;

    private bool _isBalanceMode = false;
    private BalanceModeConfiguration _balanceModeConfiguration = new();

    private bool _isAdvancedColorMode = false;
    private AdvancedColorConfiguration _advancedColorConfiguration = new();

    internal ImageProcessor(Size bitmapSize, ColorDifference colorDifference)
    {
        _width = bitmapSize.Width;
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

    /// <summary>
    /// 色の追加設定を適用する
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal void SetColorSettings(AdvancedColorConfiguration advancedColorConfiguration)
    {
        _advancedColorConfiguration = advancedColorConfiguration;
        _isAdvancedColorMode = advancedColorConfiguration.Enabled;
    }

    private ColorPixel ProcessPixel(ColorPixel source)
    {
        if (source.IsTransparent) return source;

        if (_isBalanceMode)
        {
            ColorUtils.BalanceColorAdjustment(ref source, _colorOffset, _balanceModeConfiguration);
        }
        else
        {
            source += _colorOffset;
        }

        if (_isAdvancedColorMode)
            ColorUtils.AdvancedColorAdjustment(ref source, _advancedColorConfiguration);

        return source;
    }

    internal void ProcessAllPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> target
    )
    {
        for (int i = 0; i < source.Length; i++)
        {
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal void ProcessAllPreviewPixelsWithoutAdjustment(
        Span<ColorPixel> source,
        Span<ColorPixel> target,
        (float ratioX, float ratioY) ratios,
        Size previewSize
    )
    {
        int targetWidth = previewSize.Width;
        int targetHeight = previewSize.Height;

        float ratioX = ratios.ratioX;
        float ratioY = ratios.ratioY;

        int sourceSize = source.Length;
        int targetSize = target.Length;

        for (int y = 0; y < targetHeight; y++)
        {
            int sourceY = (int)(y * ratioY);
            int targetRowOffset = y * targetWidth;

            for (int x = 0; x < targetWidth; x++)
            {
                int sourceX = (int)(x * ratioX);
                int sourceIndex = PixelUtils.GetPixelIndex(sourceX, sourceY, _width);
                int targetIndex = targetRowOffset + x;

                if (sourceIndex < 0 || sourceIndex >= sourceSize) continue;
                if (targetIndex < 0 || targetIndex >= targetSize) continue;

                target[targetIndex] = source[sourceIndex];
            }
        }
    }

    internal void ProcessSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> target,
        BitArray selectedPoints
    )
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            target[i] = ProcessPixel(source[i]);
        }
    }

    internal static void ProcessInverseSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> raw,
        BitArray selectedPoints
    )
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = raw[i];
        }
    }

    internal void ProcessTransparentSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> trans,
        BitArray selectedPoints
    )
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
        BitArray selectedPoints
    )
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
        ColorPixel color
    )
    {
        for (int i = 0; i < selectedPoints.Length; i++)
        {
            if (!selectedPoints[i]) continue;
            source[i] = color;
        }
    }
}
