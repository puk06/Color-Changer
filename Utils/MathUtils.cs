namespace ColorChanger.Utils;

internal class MathUtils
{
    /// <summary>
    /// 数値をパースして0〜255にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int ParseAndClamp(string value)
        => Math.Min(255, Math.Max(0, int.TryParse(value, out int result) ? result : 0));

    /// <summary>
    /// 数値を0〜255にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int ClampColorValue(int value)
        => Math.Clamp(value, 0, 255);

    /// <summary>
    /// 文字列をdoubleにパースし、失敗した場合はデフォルト値を返す
    /// </summary>
    /// <param name="input"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    internal static double ParseDoubleOrDefault(string input, double defaultValue = 0.0)
        => double.TryParse(input, out double result) ? result : defaultValue;
}
