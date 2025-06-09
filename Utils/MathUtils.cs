namespace ColorChanger.Utils;

internal class MathUtils
{
    /// <summary>
    /// 数値をパースして0〜255にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int ParseAndClamp(string value)
        => ClampColorValue(int.TryParse(value, out int result) ? result : 0);

    /// <summary>
    /// 数値を0〜255にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static byte ClampColorValue(int value)
        => (byte)Math.Clamp(value, 0, 255);

    /// <summary>
    /// 数値を0〜1にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static double ClampZeroToOne(double value)
        => Math.Clamp(value, 0, 1);

    /// <summary>
    /// 文字列をdoubleにパースし、失敗した場合はデフォルト値を返す
    /// </summary>
    /// <param name="input"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    internal static double ParseDoubleOrDefault(string input, double defaultValue = 0.0)
        => double.TryParse(input, out double result) ? result : defaultValue;

    /// <summary>
    /// doubleの値を小数点以下2桁まで比較する
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    internal static bool EqualDouble(double value1, double value2)
        => (int)(value1 * 100) == (int)(value2 * 100);
}
