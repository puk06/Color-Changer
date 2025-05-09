using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.ImageProcessing;

internal class ImageProcessor
{
    private readonly int _width;
    private readonly int _height;
    private readonly bool _isBalanceMode;
    private readonly Color _previousColor;
    private readonly ColorDifference _colorDifference;
    private readonly BalanceModeConfiguration _balanceModeConfiguration;

    internal ImageProcessor(int width, int height, Color previousColor, ColorDifference colorDifference, bool isBalanceMode, BalanceModeConfiguration balanceModeConfiguration)
    {
        _width = width;
        _height = height;
        _previousColor = previousColor;
        _colorDifference = colorDifference;
        _isBalanceMode = isBalanceMode;
        _balanceModeConfiguration = balanceModeConfiguration;
    }

    internal void ProcessPixel(int x, int y, Span<ColorPixel> source, Span<ColorPixel> target, int customIndex = -1)
    {
        int index = MathUtils.GetPixelIndex(x, y, _width);
        if (source[index].IsTransparent) return;

        ColorPixel src = source[index];

        if (_isBalanceMode)
            src = ColorUtils.BalanceColorAdjustment(src, _previousColor, _colorDifference, _balanceModeConfiguration);
        else
            src += _colorDifference;

        if (customIndex == -1)
            target[index] = src;
        else
            target[customIndex] = src;
    }

    internal void ProcessInversePixel(int x, int y, Span<ColorPixel> source, Span<ColorPixel> raw)
    {
        int index = MathUtils.GetPixelIndex(x, y, _width);
        if (source[index].IsTransparent) return;

        source[index] = new ColorPixel(
            raw[index].R,
            raw[index].G,
            raw[index].B,
            source[index].A
        );
    }

    internal void ProcessTransPixel(int x, int y, Span<ColorPixel> source)
    {
        int index = MathUtils.GetPixelIndex(x, y, _width);
        source[index] = new ColorPixel(0, 0, 0, 0);
    }

    internal void ProcessAllPixels(Span<ColorPixel> source, Span<ColorPixel> target)
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                ProcessPixel(x, y, source, target);
    }

    internal void ProcessAllPreviewPixels(Span<ColorPixel> source, Span<ColorPixel> target, float ratioX, float ratioY, int previewWidth, int previewHeight)
    {
        for (int y = 0; y < previewHeight; y++)
        {
            for (int x = 0; x < previewWidth; x++)
            {
                int sourceX = (int)(x * ratioX);
                int sourceY = (int)(y * ratioY);

                int previewIndex = MathUtils.GetPixelIndex(x, y, previewWidth);
                ProcessPixel(sourceX, sourceY, source, target, previewIndex);
            }
        }
    }

    internal void ProcessSelectedPixels(Span<ColorPixel> source, Span<ColorPixel> target, (int x, int y)[][] selectedPointsArray)
    {
        foreach (var selectedPoints in selectedPointsArray)
            foreach (var (x, y) in selectedPoints)
                ProcessPixel(x, y, source, target);
    }

    internal void ProcessInverseSelectedPixels(Span<ColorPixel> source, Span<ColorPixel> raw, (int x, int y)[][] selectedPointsArray)
    {
        foreach (var selectedPoints in selectedPointsArray)
            foreach (var (x, y) in selectedPoints)
                ProcessInversePixel(x, y, source, raw);
    }

    internal void ProcessTransparentSelectedPixels(Span<ColorPixel> source, Span<ColorPixel> trans, (int x, int y)[][] selectedPointsArray)
    {
        foreach (var selectedPoints in selectedPointsArray)
            foreach (var (x, y) in selectedPoints)
                ProcessPixel(x, y, source, trans != default ? trans : source);
    }

    internal void ProcessTransparentInverse(Span<ColorPixel> source, (int x, int y)[][] selectedPointsArray)
    {
        ProcessAllPixels(source, source);
        foreach (var selectedPoints in selectedPointsArray)
            foreach (var (x, y) in selectedPoints)
                ProcessTransPixel(x, y, source);
    }

    internal static void ChangeSelectedPixelsColor(Span<ColorPixel> source, int previewWidth, (int x, int y)[][] selectedPointsArray, ColorPixel color)
    {
        foreach (var selectedPoints in selectedPointsArray)
        {
            foreach (var (x, y) in selectedPoints)
            {
                int index = MathUtils.GetPixelIndex(x, y, previewWidth);
                source[index] = color;
            }
        }
    }
}
