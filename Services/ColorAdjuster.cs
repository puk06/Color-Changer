using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 1ピクセルに対する色変換（バランスモード・色の追加設定）を行う
/// </summary>
internal static class ColorAdjuster
{
    /// <summary>
    /// バランスモードの計算式で色を変換する
    /// </summary>
    internal static void BalanceColorAdjustment(
        ref ColorPixel pixel,
        ColorDifference diff,
        BalanceModeConfiguration balanceModeConfiguration
    )
    {
        var distance = ColorService.GetColorDistance(pixel, diff.PreviousColor);

        var adjustmentFactor = 0.0;

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

                var grayScale = (
                    (grayScaleWeightR * (pixel.R / 255.0)) +
                    (grayScaleWeightG * (pixel.G / 255.0)) +
                    (grayScaleWeightB * (pixel.B / 255.0))
                );

                var gradientStart = balanceModeConfiguration.V3GradientStart / 100.0;
                var gradientEnd = balanceModeConfiguration.V3GradientEnd / 100.0;

                pixel.R = ColorService.CalculateGradientValue(diff.NewColor.R, balanceModeConfiguration.V3GradientColor.R, gradientStart, gradientEnd, grayScale);
                pixel.G = ColorService.CalculateGradientValue(diff.NewColor.G, balanceModeConfiguration.V3GradientColor.G, gradientStart, gradientEnd, grayScale);
                pixel.B = ColorService.CalculateGradientValue(diff.NewColor.B, balanceModeConfiguration.V3GradientColor.B, gradientStart, gradientEnd, grayScale);
                return;
        }

        pixel.R = MathHelper.ClampColorValue(pixel.R + (int)(diff.DiffR * adjustmentFactor));
        pixel.G = MathHelper.ClampColorValue(pixel.G + (int)(diff.DiffG * adjustmentFactor));
        pixel.B = MathHelper.ClampColorValue(pixel.B + (int)(diff.DiffB * adjustmentFactor));
    }

    /// <summary>
    /// RGB空間の壁に交差する点までの距離を計算する
    /// </summary>
    internal static (bool hasIntersection, double IntersectionDistance) GetRGBIntersectionDistance(Color baseColor, ColorPixel targetColor)
    {
        var baseR = baseColor.R;
        var baseG = baseColor.G;
        var baseB = baseColor.B;

        var dx = targetColor.R - baseR;
        var dy = targetColor.G - baseG;
        var dz = targetColor.B - baseB;

        // 無限線分を RGB 空間の壁に交差する点まで伸ばす（各軸で）
        var tValues = new List<double>();

        // 各チャンネルの0と255の壁について、t値（ベクトル方向に進むスカラー量）を求める
        if (dx != 0)
        {
            tValues.Add((0 - baseR) / (double)dx);
            tValues.Add((255 - baseR) / (double)dx);
        }

        if (dy != 0)
        {
            tValues.Add((0 - baseG) / (double)dy);
            tValues.Add((255 - baseG) / (double)dy);
        }

        if (dz != 0)
        {
            tValues.Add((0 - baseB) / (double)dz);
            tValues.Add((255 - baseB) / (double)dz);
        }

        // 最小正の t を探す（延長線上、前方方向）
        var minPositiveT = double.MaxValue;
        foreach (var t in tValues.Where(t => t > 0))
        {
            var x = baseR + (t * dx);
            var y = baseG + (t * dy);
            var z = baseB + (t * dz);

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
        if (Math.Abs(minPositiveT - double.MaxValue) > MathHelper.EPSILON)
        {
            var length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            return (true, minPositiveT * length);
        }

        return (false, -1);
    }

    /// <summary>
    /// 重みから色の変化率を計算する
    /// </summary>
    internal static double CalculateColorChangeRate(bool hasIntersection, double intersectionDistance, double distance, double graphWeight, double minValue)
    {
        if (!hasIntersection || Math.Abs(intersectionDistance) < MathHelper.EPSILON) return 1;
        var changeRate = Math.Pow(1 - (distance / intersectionDistance), graphWeight);
        return Math.Max(minValue, changeRate);
    }

    /// <summary>
    /// 色の追加設定（明度・コントラスト・ガンマ・露出・透明度）を適用する
    /// </summary>
    internal static void AdvancedColorAdjustment(ref ColorPixel pixel, AdvancedColorConfiguration advancedColorConfiguration)
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

    private static void ApplyBrightness(ref ColorPixel pixel, double brightness)
    {
        pixel.R = MathHelper.ClampColorValue((int)(pixel.R * brightness));
        pixel.G = MathHelper.ClampColorValue((int)(pixel.G * brightness));
        pixel.B = MathHelper.ClampColorValue((int)(pixel.B * brightness));
    }

    private static void ApplyContrast(ref ColorPixel pixel, double contrast)
    {
        pixel.R = MathHelper.ClampColorValue((int)(((pixel.R - 128) * contrast) + 128));
        pixel.G = MathHelper.ClampColorValue((int)(((pixel.G - 128) * contrast) + 128));
        pixel.B = MathHelper.ClampColorValue((int)(((pixel.B - 128) * contrast) + 128));
    }

    private static void ApplyGamma(ref ColorPixel pixel, double gamma)
    {
        pixel.R = MathHelper.ClampColorValue((int)(Math.Pow(pixel.R / 255.0, gamma) * 255));
        pixel.G = MathHelper.ClampColorValue((int)(Math.Pow(pixel.G / 255.0, gamma) * 255));
        pixel.B = MathHelper.ClampColorValue((int)(Math.Pow(pixel.B / 255.0, gamma) * 255));
    }

    private static void ApplyExposure(ref ColorPixel pixel, double exposure)
    {
        pixel.R = MathHelper.ClampColorValue((int)(pixel.R * Math.Pow(2, exposure)));
        pixel.G = MathHelper.ClampColorValue((int)(pixel.G * Math.Pow(2, exposure)));
        pixel.B = MathHelper.ClampColorValue((int)(pixel.B * Math.Pow(2, exposure)));
    }

    private static void ApplyTransparency(ref ColorPixel pixel, double transparency)
    {
        pixel.A = MathHelper.ClampColorValue((int)(pixel.A * (1 - transparency)));
    }
}
