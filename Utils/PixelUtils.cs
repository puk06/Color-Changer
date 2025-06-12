namespace ColorChanger.Utils;

internal readonly struct PixelPoint(int x, int y)
{
    internal int X { get; } = x;
    internal int Y { get; } = y;

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}

internal static class PixelUtils
{
    /// <summary>
    /// 指定された座標からピクセルインデックスを取得する
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    internal static int GetPixelIndex(int x, int y, int width)
        => (y * width) + x;
}
