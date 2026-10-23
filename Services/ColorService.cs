using System.Drawing.Imaging;
using ColorChanger.Models;

namespace ColorChanger.Services;

internal static class ColorService
{
    /// <summary>
    /// RGB値からカラーコードを取得する
    /// </summary>
    internal static string GetColorCodeFromColor(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>
    /// カラーコードからRGB値を取得する
    /// </summary>
    internal static Color GetColorFromColorCode(string colorCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(colorCode))
                return Color.Empty;

            if (!colorCode.StartsWith('#'))
                colorCode = "#" + colorCode;

            if (colorCode.Length != 7)
                return Color.Empty;

            return ColorTranslator.FromHtml(colorCode);
        }
        catch
        {
            return Color.Empty;
        }
    }

    /// <summary>
    /// グラデーションのプレビュー画像を生成する
    /// </summary>
    internal static Bitmap GenerateGradientPreview(Color startColor, Color endColor, int start, int end, Size size)
    {
        var gradientStart = start / 100.0;
        var gradientEnd = end / 100.0;

        var bmp = new Bitmap(size.Width, size.Height);
        using var graphics = Graphics.FromImage(bmp);

        for (var x = 0; x < size.Width; x++)
        {
            var grayRatio = Math.Clamp((double)x / size.Width, 0, 1);

            var r = CalculateGradientValue(startColor.R, endColor.R, gradientStart, gradientEnd, grayRatio);
            var g = CalculateGradientValue(startColor.G, endColor.G, gradientStart, gradientEnd, grayRatio);
            var b = CalculateGradientValue(startColor.B, endColor.B, gradientStart, gradientEnd, grayRatio);

            using var brush = new SolidBrush(Color.FromArgb(r, g, b));
            graphics.FillRectangle(brush, x, 0, 1, size.Height);
        }

        return bmp;
    }

    /// <summary>
    /// グラデーションの位置に応じた値を計算する
    /// </summary>
    internal static byte CalculateGradientValue(
        byte startColorValue, byte endColorValue,
        double gradientStart, double gradientEnd,
        double grayRatio
    )
    {
        if (grayRatio <= gradientStart) return startColorValue;
        if (grayRatio >= gradientEnd) return endColorValue;

        var interpolationFactor = (grayRatio - gradientStart) / (gradientEnd - gradientStart);
        var interpolatedValue = startColorValue + ((endColorValue - startColorValue) * interpolationFactor);

        return MathHelper.ClampColorValue((int)Math.Round(interpolatedValue));
    }

    /// <summary>
    /// 2つの色の距離を計算する
    /// </summary>
    internal static double GetColorDistance(Color color1, Color color2)
        => GetColorDistanceInternal((color1.R, color1.G, color1.B), (color2.R, color2.G, color2.B));

    /// <summary>
    /// 2つの色の距離を計算する
    /// </summary>
    internal static double GetColorDistance(ColorPixel color1, Color color2)
        => GetColorDistanceInternal((color1.R, color1.G, color1.B), (color2.R, color2.G, color2.B));

    private static double GetColorDistanceInternal((int R, int G, int B) c1, (int R, int G, int B) c2)
    {
        var r = Math.Pow(c1.R - c2.R, 2);
        var g = Math.Pow(c1.G - c2.G, 2);
        var b = Math.Pow(c1.B - c2.B, 2);

        return Math.Sqrt(r + g + b);
    }

    /// <summary>
    /// 指定した色に最も近いピクセルの座標を取得する
    /// </summary>
    internal static Point FindClosestPoint(Bitmap bitmap, Color color)
    {
        var minDistance = double.MaxValue;
        var bestPoint = Point.Empty;

        using var pixelLock = new PixelLock(bitmap, ImageLockMode.ReadOnly);
        var pixels = pixelLock.Pixels;

        for (var i = 0; i < pixels.Length; i++)
        {
            var distance = GetColorDistance(pixels[i], color);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestPoint = new Point(i % bitmap.Width, i / bitmap.Width);
            }
        }

        return bestPoint;
    }

    /// <summary>
    /// 色を反転させる
    /// </summary>
    internal static Color InverseColor(Color color)
        => Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);

    /// <summary>
    /// HSVからRGBのColorを返します。
    /// </summary>
    internal static Color ColorFromHSV(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = (hue / 60) - Math.Floor(hue / 60);

        value *= 255;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - (f * saturation)));
        var t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q),
        };
    }

    /// <summary>
    /// RGBをHSVに変換します。
    /// </summary>
    internal static void RGBtoHSV(Color color, out double hue, out double saturation, out double value)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        if (delta == 0)
        {
            hue = 0;
        }
        else if (max == r)
        {
            hue = 60 * (((g - b) / delta) % 6);
        }
        else if (max == g)
        {
            hue = 60 * (((b - r) / delta) + 2);
        }
        else
        {
            hue = 60 * (((r - g) / delta) + 4);
        }

        if (hue < 0) hue += 360;

        saturation = (max == 0) ? 0 : delta / max;

        value = max;
    }

    /// <summary>
    /// 指定した色を元に、彩度と明度を調整した色を返します。
    /// </summary>
    internal static Color InterpolateColor(Color target, float saturation, float value)
    {
        var r = (int)(255 + ((target.R - 255) * saturation));
        var g = (int)(255 + ((target.G - 255) * saturation));
        var b = (int)(255 + ((target.B - 255) * saturation));

        r = MathHelper.ClampColorValue((int)(r * value));
        g = MathHelper.ClampColorValue((int)(g * value));
        b = MathHelper.ClampColorValue((int)(b * value));

        return Color.FromArgb(r, g, b);
    }
}
