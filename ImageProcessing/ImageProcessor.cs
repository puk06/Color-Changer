using ColorChanger.Models;
using ColorChanger.Utils;

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
        (int x, int y)[][] selectedPointsArray)
    {
        foreach (var (x, y) in selectedPointsArray.SelectMany(p => p))
        {
            int index = PixelUtils.GetPixelIndex(x, y, _width);

            target[index] = ProcessPixel(source[index]);
        }
    }

    internal void ProcessInverseSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> raw,
        (int x, int y)[][] selectedPointsArray)
    {
        foreach (var (x, y) in selectedPointsArray.SelectMany(p => p))
        {
            int index = PixelUtils.GetPixelIndex(x, y, _width);

            source[index] = new ColorPixel(
                raw[index].R,
                raw[index].G,
                raw[index].B,
                source[index].A
            );
        }
    }

    internal void ProcessTransparentSelectedPixels(
        Span<ColorPixel> source,
        Span<ColorPixel> trans,
        (int x, int y)[][] selectedPointsArray)
    {
        Span<ColorPixel> target = !trans.IsEmpty ? trans : source;

        foreach (var (x, y) in selectedPointsArray.SelectMany(p => p))
        {
            int index = PixelUtils.GetPixelIndex(x, y, _width);

            target[index] = ProcessPixel(source[index]);
        }
    }

    internal void ProcessTransparentAndInversePixels(
        Span<ColorPixel> source,
        (int x, int y)[][] selectedPointsArray)
    {
        ProcessAllPixels(source, source);

        foreach (var (x, y) in selectedPointsArray.SelectMany(p => p))
        {
            int index = PixelUtils.GetPixelIndex(x, y, _width);

            source[index] = ColorUtils.TransparentPixel;
        }
    }

    internal static void ChangeSelectedPixelsColor(
        Span<ColorPixel> source,
        int previewWidth,
        (int x, int y)[][] selectedPointsArray,
        ColorPixel color)
    {
        foreach (var (x, y) in selectedPointsArray.SelectMany(p => p))
        {
            int index = PixelUtils.GetPixelIndex(x, y, previewWidth);

            source[index] = color;
        }
    }
}
