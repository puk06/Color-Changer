namespace ColorChanger.Utils;

internal static class ParallelUtils
{
    /// <summary>
    /// 指定した 2D 範囲（x, y）に対して並列で処理を行います。
    /// </summary>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <param name="action">x, y を引数に取る処理</param>
    internal static void ForEach2D(int height, int width, Action<int, int> action)
    {
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                action(x, y);
            }
        });
    }
}
