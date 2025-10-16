using ColorChanger.Models;
using System.Runtime.InteropServices;

namespace ColorChanger.Utils;

[StructLayout(LayoutKind.Sequential)]
internal struct ColorPixel(byte r, byte g, byte b, byte a)
{
    internal byte B = b;
    internal byte G = g;
    internal byte R = r;
    internal byte A = a;

    internal readonly bool IsTransparent => A == 0;

    internal readonly bool Equals(Color other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public static ColorPixel operator +(ColorPixel pixel, ColorDifference colorDiff)
    {
        pixel.R = MathUtils.ClampColorValue(pixel.R + colorDiff.DiffR);
        pixel.G = MathUtils.ClampColorValue(pixel.G + colorDiff.DiffG);
        pixel.B = MathUtils.ClampColorValue(pixel.B + colorDiff.DiffB);

        return pixel;
    }
}

internal static class ColorUtils
{
    private static ColorPixel TransparentColorPixel = new ColorPixel(0, 0, 0, 0);

    /// <summary>
    /// RGB値からカラーコードを取得する
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    internal static string GetColorCodeFromColor(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>
    /// カラーコードからRGB値を取得する
    /// </summary>
    /// <param name="colorCode"></param>
    /// <returns></returns>
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
    /// グラデーションのプレビューを生成する
    /// </summary>
    /// <param name="startColor"></param>
    /// <param name="endColor"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    internal static Bitmap GenerateGradientPreview(
        Color startColor, Color endColor,
        int start, int end,
        Size size
    )
    {
        double gradientStart = start / 100.0;
        double gradientEnd = end / 100.0;

        Bitmap bmp = new Bitmap(size.Width, size.Height);
        using (Graphics graphics = Graphics.FromImage(bmp))
        {
            for (int x = 0; x < size.Width; x++)
            {
                double grayRatio = MathUtils.ClampZeroToOne((double)x / size.Width);

                int r = CalculateGradientValue(
                    startColor.R, endColor.R,
                    gradientStart, gradientEnd,
                    grayRatio
                );

                int g = CalculateGradientValue(
                    startColor.G, endColor.G,
                    gradientStart, gradientEnd,
                    grayRatio
                );

                int b = CalculateGradientValue(
                    startColor.B, endColor.B,
                    gradientStart, gradientEnd,
                    grayRatio
                );

                Color color = Color.FromArgb(r, g, b);

                using Brush brush = new SolidBrush(color);
                graphics.FillRectangle(brush, x, 0, 1, size.Height);
            }
        }

        return bmp;
    }

    /// <summary>
    /// 2つの色の距離を計算する
    /// </summary>
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns></returns>
    internal static double GetColorDistance(Color color1, Color color2)
        => GetColorDistanceInternal((color1.R, color1.G, color1.B), (color2.R, color2.G, color2.B));

    /// <summary>
    /// 2つの色の距離を計算する
    /// </summary>
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns></returns>
    internal static double GetColorDistance(ColorPixel color1, Color color2)
        => GetColorDistanceInternal((color1.R, color1.G, color1.B), (color2.R, color2.G, color2.B));

    private static double GetColorDistanceInternal((int R, int G, int B) c1, (int R, int G, int B) c2)
    {
        double r = Math.Pow(c1.R - c2.R, 2);
        double g = Math.Pow(c1.G - c2.G, 2);
        double b = Math.Pow(c1.B - c2.B, 2);

        return Math.Sqrt(r + g + b);
    }

    /// <summary>
    /// 指定した色に最も近い色の座標を取得する
    /// </summary>
    /// <param name="color"></param>
    /// <param name="bmp"></param>
    /// <returns></returns>
    internal static Point GetClosestColorPoint(Color color, Bitmap bmp)
    {
        Point closestPoint = Point.Empty;
        double closestDistance = double.MaxValue;

        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                Color currentPixelColor = bmp.GetPixel(x, y);
                double distance = GetColorDistance(color, currentPixelColor);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = new Point(x, y);
                }
            }
        }

        return closestPoint;
    }

    /// <summary>
    /// 色のバランス調整を行う
    /// </summary>
    /// <param name="pixel"></param>
    /// <param name="diff"></param>
    /// <param name="balanceModeConfiguration"></param>
    /// <returns></returns>
    internal static void BalanceColorAdjustment(
        ref ColorPixel pixel,
        ColorDifference diff,
        BalanceModeConfiguration balanceModeConfiguration
    )
    {
        double distance = GetColorDistance(pixel, diff.PreviousColor);

        double adjustmentFactor = 0.0;

        switch (balanceModeConfiguration.ModeVersion)
        {
            case 1:
                var (hasIntersection, intersectionDistance) = GetRGBIntersectionDistance(diff.PreviousColor, pixel);

                adjustmentFactor = CalculateColorChangeRate(
                    hasIntersection,
                    intersectionDistance,
                    distance,
                    balanceModeConfiguration.V1Weight,
                    balanceModeConfiguration.V1MinimumValue
                );
                break;

            case 2:
                if (distance <= balanceModeConfiguration.V2Radius)
                {
                    adjustmentFactor = CalculateColorChangeRate(
                        true,
                        balanceModeConfiguration.V2Radius,
                        distance,
                        balanceModeConfiguration.V2Weight,
                        balanceModeConfiguration.V2MinimumValue
                    );
                }
                else if (balanceModeConfiguration.V2IncludeOutside)
                {
                    adjustmentFactor = balanceModeConfiguration.V2MinimumValue;
                }
                break;

            case 3:
                // 赤は0.299、緑は0.587、青は0.114の重みを使ってグレースケール値を計算
                // この値は、人間の視覚における色の感度を考慮した輝度法に基づいています
                // コード内で使用しているグレースケールの値に関する詳細はこちらから: https://en.wikipedia.org/wiki/Grayscale
                const double grayScaleWeightR = 0.299;
                const double grayScaleWeightG = 0.587;
                const double grayScaleWeightB = 0.114;

                double grayScale = (
                    (grayScaleWeightR * (pixel.R / 255.0)) +
                    (grayScaleWeightG * (pixel.G / 255.0)) +
                    (grayScaleWeightB * (pixel.B / 255.0))
                );

                double gradientStart = balanceModeConfiguration.V3GradientStart / 100.0;
                double gradientEnd = balanceModeConfiguration.V3GradientEnd / 100.0;

                pixel.R = CalculateGradientValue(
                    diff.NewColor.R,
                    balanceModeConfiguration.V3GradientColor.R,
                    gradientStart,
                    gradientEnd,
                    grayScale
                );

                pixel.G = CalculateGradientValue(
                    diff.NewColor.G,
                    balanceModeConfiguration.V3GradientColor.G,
                    gradientStart,
                    gradientEnd,
                    grayScale
                );

                pixel.B = CalculateGradientValue(
                    diff.NewColor.B,
                    balanceModeConfiguration.V3GradientColor.B,
                    gradientStart,
                    gradientEnd,
                    grayScale
                );
                return;
        }

        pixel.R = MathUtils.ClampColorValue(pixel.R + (int)(diff.DiffR * adjustmentFactor));
        pixel.G = MathUtils.ClampColorValue(pixel.G + (int)(diff.DiffG * adjustmentFactor));
        pixel.B = MathUtils.ClampColorValue(pixel.B + (int)(diff.DiffB * adjustmentFactor));
    }

    private static byte CalculateGradientValue(
        byte startColorValue, byte endColorValue,
        double gradientStart, double gradientEnd,
        double grayRatio
    )
    {
        if (grayRatio <= gradientStart) return startColorValue;
        if (grayRatio >= gradientEnd) return endColorValue;

        double interpolationFactor = (grayRatio - gradientStart) / (gradientEnd - gradientStart);
        double interpolatedValue = startColorValue + ((endColorValue - startColorValue) * interpolationFactor);

        return MathUtils.ClampColorValue((int)Math.Round(interpolatedValue));
    }

    /// <summary>
    /// RGB空間の壁に交差する点までの距離を計算する
    /// </summary>
    /// <param name="baseColor"></param>
    /// <param name="targetColor"></param>
    /// <returns></returns>
    internal static (bool hasIntersection, double IntersectionDistance) GetRGBIntersectionDistance(Color baseColor, ColorPixel targetColor)
    {
        // 基準色
        int base_r = baseColor.R;
        int base_g = baseColor.G;
        int base_b = baseColor.B;

        // 目標色
        int target_r = targetColor.R;
        int target_g = targetColor.G;
        int target_b = targetColor.B;

        // 方向ベクトル
        int dx = target_r - base_r;
        int dy = target_g - base_g;
        int dz = target_b - base_b;

        // 無限線分を RGB 空間の壁に交差する点まで伸ばす（各軸で）
        List<double> t_values = new();

        // 各チャンネルの0と255の壁について、t値（ベクトル方向に進むスカラー量）を求める
        if (dx != 0)
        {
            t_values.Add((0 - base_r) / (double)dx);
            t_values.Add((255 - base_r) / (double)dx);
        }

        if (dy != 0)
        {
            t_values.Add((0 - base_g) / (double)dy);
            t_values.Add((255 - base_g) / (double)dy);
        }

        if (dz != 0)
        {
            t_values.Add((0 - base_b) / (double)dz);
            t_values.Add((255 - base_b) / (double)dz);
        }

        // 最小正の t を探す（延長線上、前方方向）
        double minPositiveT = double.MaxValue;
        foreach (double t in t_values.Where(t => t > 0))
        {
            double x = base_r + (t * dx);
            double y = base_g + (t * dy);
            double z = base_b + (t * dz);

            // 点がRGB空間内にあるか（各成分が0〜255の間）
            if (
                x >= 0 && x <= 255 &&
                y >= 0 && y <= 255 &&
                z >= 0 && z <= 255 &&
                t < minPositiveT
            )
            {
                minPositiveT = t;
            }
        }

        // 最短距離 = ベクトルの長さ * t
        if (Math.Abs(minPositiveT - double.MaxValue) > MathUtils.EPSILON)
        {
            double length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            return (true, minPositiveT * length);
        }

        // 交差しない場合
        return (false, -1);
    }

    /// <summary>
    /// 重みから色の変化率を計算する
    /// </summary>
    /// <param name="hasIntersection"></param>
    /// <param name="intersectionDistance"></param>
    /// <param name="distance"></param>
    /// <param name="graphWeight"></param>
    /// <param name="minValue"></param>
    /// <returns></returns>
    internal static double CalculateColorChangeRate(bool hasIntersection, double intersectionDistance, double distance, double graphWeight, double minValue)
    {
        if (!hasIntersection || Math.Abs(intersectionDistance) < MathUtils.EPSILON) return 1;
        double changeRate = Math.Pow(1 - (distance / intersectionDistance), graphWeight);
        return Math.Max(minValue, changeRate);
    }

    /// <summary>
    /// 色の追加設定を行う
    /// </summary>
    /// <param name="pixel"></param>
    /// <param name="advancedColorConfiguration"></param>
    /// <returns></returns>
    internal static void AdvancedColorAdjustment(
        ref ColorPixel pixel,
        AdvancedColorConfiguration advancedColorConfiguration
    )
    {
        if (advancedColorConfiguration.BrightnessEnabled)
            ApplyBrightness(ref pixel, advancedColorConfiguration.Brightness);

        if (advancedColorConfiguration.ContrastEnabled)
            ApplyContrast(ref pixel, advancedColorConfiguration.Contrast);

        if (advancedColorConfiguration.GammaEnabled)
            ApplyGamma(ref pixel, advancedColorConfiguration.Gamma);

        if (advancedColorConfiguration.ExposureEnabled)
            ApplyExposure(ref pixel, advancedColorConfiguration.Exposure);

        if (advancedColorConfiguration.TransparencyEnabled)
            ApplyTransparency(ref pixel, advancedColorConfiguration.Transparency);
    }

    #region 色の追加設定用メソッド
    private static void ApplyBrightness(ref ColorPixel pixel, double brightness)
    {
        pixel.R = MathUtils.ClampColorValue((int)(pixel.R * brightness));
        pixel.G = MathUtils.ClampColorValue((int)(pixel.G * brightness));
        pixel.B = MathUtils.ClampColorValue((int)(pixel.B * brightness));
    }
    private static void ApplyContrast(ref ColorPixel pixel, double contrast)
    {
        pixel.R = MathUtils.ClampColorValue((int)(((pixel.R - 128) * contrast) + 128));
        pixel.G = MathUtils.ClampColorValue((int)(((pixel.G - 128) * contrast) + 128));
        pixel.B = MathUtils.ClampColorValue((int)(((pixel.B - 128) * contrast) + 128));
    }
    private static void ApplyGamma(ref ColorPixel pixel, double gamma)
    {
        pixel.R = MathUtils.ClampColorValue((int)(Math.Pow(pixel.R / 255.0, gamma) * 255));
        pixel.G = MathUtils.ClampColorValue((int)(Math.Pow(pixel.G / 255.0, gamma) * 255));
        pixel.B = MathUtils.ClampColorValue((int)(Math.Pow(pixel.B / 255.0, gamma) * 255));
    }
    private static void ApplyExposure(ref ColorPixel pixel, double exposure)
    {
        pixel.R = MathUtils.ClampColorValue((int)(pixel.R * Math.Pow(2, exposure)));
        pixel.G = MathUtils.ClampColorValue((int)(pixel.G * Math.Pow(2, exposure)));
        pixel.B = MathUtils.ClampColorValue((int)(pixel.B * Math.Pow(2, exposure)));
    }
    private static void ApplyTransparency(ref ColorPixel pixel, double transparency)
    {
        pixel.A = MathUtils.ClampColorValue((int)(pixel.A * (1 - transparency)));
    }
    #endregion

    /// <summary>
    /// 色を反転させる
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    internal static Color InverseColor(Color color)
        => Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);

    /// <summary>
    /// HSVからRGBのColorを返します。
    /// </summary>
    /// <param name="hue"></param>
    /// <param name="saturation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = (hue / 60) - Math.Floor(hue / 60);

        value *= 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - (f * saturation)));
        int t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

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
    /// <param name="color"></param>
    /// <param name="hue"></param>
    /// <param name="saturation"></param>
    /// <param name="value"></param>
    internal static void RGBtoHSV(Color color, out double hue, out double saturation, out double value)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

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
    /// 指定した色を元に、彩度と明度を調整した物を返します。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="saturation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static Color InterpolateColor(Color target, float saturation, float value)
    {
        int r = (int)(255 + ((target.R - 255) * saturation));
        int g = (int)(255 + ((target.G - 255) * saturation));
        int b = (int)(255 + ((target.B - 255) * saturation));

        r = MathUtils.ClampColorValue((int)(r * value));
        g = MathUtils.ClampColorValue((int)(g * value));
        b = MathUtils.ClampColorValue((int)(b * value));

        return Color.FromArgb(r, g, b);
    }

    /// <summary>
    /// 透明なピクセルを取得する
    /// </summary>
    internal static ColorPixel TransparentPixel
        => TransparentColorPixel;

    /// <summary>
    /// デフォルトの背景色を取得する
    /// </summary>
    internal static Color DefaultBackgroundColor
        => Color.LightGray;

    /// <summary>
    /// デフォルトの色を取得する
    /// </summary>
    internal static Color DefaultForeColor
        => Color.LightGreen;

    /// <summary>
    /// デフォルトの未選択時の色を取得する
    /// </summary>
    internal static Color DefaultUnselectedColor
        => Color.LightCoral;
}
