using ColorChanger.Models;
using System.Runtime.InteropServices;

namespace ColorChanger.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ColorPixel(byte r, byte g, byte b, byte a)
    {
        internal byte B = b;
        internal byte G = g;
        internal byte R = r;
        internal byte A = a;

        internal readonly bool IsTransparent => A == 0;

        public static ColorPixel operator +(ColorPixel pixel, ColorDifference color)
        {
            return new ColorPixel(
                (byte)MathUtils.ClampColorValue(pixel.R + color.DiffR),
                (byte)MathUtils.ClampColorValue(pixel.G + color.DiffG),
                (byte)MathUtils.ClampColorValue(pixel.B + color.DiffB),
                pixel.A
            );
        }
    }

    internal class ColorUtils
    {
        /// <summary>
        /// RGB値からカラーコードを取得する
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        internal static string GetColorCodeFromColor(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        /// <summary>
        /// カラーコードからRGB値を取得する
        /// </summary>
        /// <param name="colorCode"></param>
        /// <returns></returns>
        internal static Color GetColorFromColorCode(string colorCode)
        {
            try
            {
                if (colorCode.Length != 7) return Color.Empty;
                return ColorTranslator.FromHtml(colorCode);
            }
            catch
            {
                return Color.Empty;
            }
        }

        /// <summary>
        /// 2つの色の距離を計算する
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        internal static double GetColorDistance(Color color1, Color color2)
        {
            double r = Math.Pow(color1.R - color2.R, 2);
            double g = Math.Pow(color1.G - color2.G, 2);
            double b = Math.Pow(color1.B - color2.B, 2);

            return Math.Sqrt(r + g + b);
        }

        /// <summary>
        /// 2つの色の距離を計算する
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        internal static double GetColorDistance(ColorPixel color1, Color color2)
        {
            double r = Math.Pow(color1.R - color2.R, 2);
            double g = Math.Pow(color1.G - color2.G, 2);
            double b = Math.Pow(color1.B - color2.B, 2);

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
                    Color currentColor = bmp.GetPixel(x, y);
                    double distance = GetColorDistance(color, currentColor);
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
        /// <param name="previousColor"></param>
        /// <param name="diff"></param>
        /// <param name="balanceModeConfiguration"></param>
        /// <returns></returns>
        internal static ColorPixel BalanceColorAdjustment(
            ColorPixel pixel,
            Color previousColor,
            ColorDifference diff,
            BalanceModeConfiguration balanceModeConfiguration
        )
        {
            double distance = GetColorDistance(pixel, previousColor);

            double adjustmentFactor = 0.0;

            switch (balanceModeConfiguration.modeVersion)
            {
                case 1:
                    var (hasIntersection, intersectionDistance) = GetRGBIntersectionDistance(previousColor, pixel);

                    adjustmentFactor = CalculateColorChangeRate(
                        hasIntersection,
                        intersectionDistance,
                        distance,
                        balanceModeConfiguration.v1Weight,
                        balanceModeConfiguration.v1MinimumValue
                    );
                    break;

                case 2:
                    if (distance <= balanceModeConfiguration.v2Radius)
                    {
                        adjustmentFactor = CalculateColorChangeRate(
                            true,
                            balanceModeConfiguration.v2Radius,
                            distance,
                            balanceModeConfiguration.v2Weight,
                            balanceModeConfiguration.v2MinimumValue
                        );
                    }
                    else if (balanceModeConfiguration.v2IncludeOutside)
                    {
                        adjustmentFactor = balanceModeConfiguration.v1MinimumValue;
                    }
                    break;
            }

            int adjustedR = MathUtils.ClampColorValue(pixel.R + (int)(diff.DiffR * adjustmentFactor));
            int adjustedG = MathUtils.ClampColorValue(pixel.G + (int)(diff.DiffG * adjustmentFactor));
            int adjustedB = MathUtils.ClampColorValue(pixel.B + (int)(diff.DiffB * adjustmentFactor));

            return new ColorPixel((byte)adjustedR, (byte)adjustedG, (byte)adjustedB, pixel.A);
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
            List<double> t_values = new List<double>();

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
            foreach (var t in t_values)
            {
                if (t > 0)
                {
                    double x = base_r + t * dx;
                    double y = base_g + t * dy;
                    double z = base_b + t * dz;

                    // 点がRGB空間内にあるか（各成分が0〜255の間）
                    if (x >= 0 && x <= 255 && y >= 0 && y <= 255 && z >= 0 && z <= 255)
                    {
                        if (t < minPositiveT)
                            minPositiveT = t;
                    }
                }
            }

            // 最短距離 = ベクトルの長さ * t
            if (minPositiveT != double.MaxValue)
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
        /// <param name="IntersectionDistance"></param>
        /// <param name="distance"></param>
        /// <param name="graphWeight"></param>
        /// <returns></returns>
        internal static double CalculateColorChangeRate(bool hasIntersection, double IntersectionDistance, double distance, double graphWeight, double minValue)
        {
            if (!hasIntersection || IntersectionDistance == 0) return 1;
            var changeRate = Math.Pow(1 - (distance / IntersectionDistance), graphWeight);
            return Math.Max(minValue, changeRate);
        }

        /// <summary>
        /// 色を反転させる
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color InverseColor(Color color) => Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
    }
}
