namespace ColorChanger.Services;

internal static class MathHelper
{
    /// <summary>
    /// 浮動小数点の比較に使用する許容誤差
    /// </summary>
    internal const double EPSILON = 1e-6;

    /// <summary>
    /// 数値をパースして0〜255にクランプする
    /// </summary>
    internal static int ParseAndClamp(string value)
        => ClampColorValue(int.TryParse(value, out int result) ? result : 0);

    /// <summary>
    /// 数値を0〜255にクランプする
    /// </summary>
    internal static byte ClampColorValue(int value)
        => (byte)Math.Clamp(value, 0, 255);

    /// <summary>
    /// 文字列をdoubleにパースし、失敗した場合はデフォルト値を返す
    /// </summary>
    internal static double ParseDoubleOrDefault(string input, double defaultValue = 0.0)
        => double.TryParse(input, out double result) ? result : defaultValue;
}
