using System.Diagnostics;

namespace ColorChanger.Forms;

/// <summary>
/// 一定時間内に連続して発生するイベントの処理を間引く
/// </summary>
internal sealed class Throttle(int intervalMs)
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <summary>
    /// 処理してよい場合は true を返し、次の間隔を開始する
    /// </summary>
    internal bool TryPass()
    {
        if (_stopwatch.ElapsedMilliseconds <= intervalMs) return false;

        _stopwatch.Restart();
        return true;
    }
}
