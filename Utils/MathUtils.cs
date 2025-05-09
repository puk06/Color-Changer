namespace ColorChanger.Utils
{
    class MathUtils
    {
        /// <summary>
        /// 数値をパースして0〜255にクランプする
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ParseAndClamp(string value)
        {
            return Math.Min(255, Math.Max(0, int.TryParse(value, out int result) ? result : 0));
        }

        /// <summary>
        /// 数値を0〜255にクランプする
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampColorValue(int value)
        {
            return Math.Clamp(value, 0, 255);
        }

        /// <summary>
        /// 文字列をdoubleにパースし、失敗した場合はデフォルト値を返す
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static double ParseDoubleOrDefault(string input, double defaultValue = 0.0)
        {
            return double.TryParse(input, out double result) ? result : defaultValue;
        }

        /// <summary>
        /// 指定された座標からピクセルインデックスを取得する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        internal static int GetPixelIndex(int x, int y, int width)
        {
            return (y * width) + x;
        }
    }
}
