using System.Collections;

namespace ColorChanger.Models;

/// <summary>
/// ペンツールで描いている選択状態と、その履歴を管理する。
/// </summary>
internal class PenSelection
{
    private readonly List<BitArray> _history = new List<BitArray>();
    private BitArray _selection = new BitArray(0);
    private bool _strokeEnded = false;

    internal bool IsInitialized { get; private set; } = false;

    internal int Width { get; private set; }

    internal int Height { get; private set; }

    internal int HistoryCount => _history.Count;

    /// <summary>
    /// 現在の選択状態（参照を返すため、保持する場合はコピーすること）
    /// </summary>
    internal BitArray Selection => _selection;

    internal void Initialize(Size imageSize)
    {
        Width = imageSize.Width;
        Height = imageSize.Height;

        _history.Clear();
        _selection = new BitArray(Width * Height);
        _strokeEnded = false;
        IsInitialized = true;
    }

    internal void Reset()
    {
        Width = 0;
        Height = 0;

        _history.Clear();
        _selection = new BitArray(0);
        _strokeEnded = false;
        IsInitialized = false;
    }

    /// <summary>
    /// 現在のストロークを終了する。次の描画時に履歴が積まれる。
    /// </summary>
    internal void EndStroke()
        => _strokeEnded = true;

    /// <summary>
    /// 中心点を基準に円形の範囲を選択、または解除する。
    /// </summary>
    internal void Paint(Point center, int diameter, bool select)
    {
        if (!IsInitialized || diameter <= 0) return;

        if (_strokeEnded || _history.Count == 0)
        {
            _history.Add(new BitArray(_selection));
            _strokeEnded = false;
        }

        int radius = diameter / 2;
        int radiusSquared = radius * radius;

        int minX = Math.Max(center.X - radius, 0);
        int maxX = Math.Min(center.X + radius, Width - 1);
        int minY = Math.Max(center.Y - radius, 0);
        int maxY = Math.Min(center.Y + radius, Height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - center.Y;
            int dy2 = dy * dy;
            int rowStart = y * Width;

            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - center.X;
                if ((dx * dx) + dy2 <= radiusSquared)
                {
                    _selection[rowStart + x] = select;
                }
            }
        }
    }

    /// <summary>
    /// 直前の履歴に戻す。戻せた場合は true を返す。
    /// </summary>
    internal bool Undo()
    {
        if (_history.Count == 0) return false;

        BitArray last = _history[^1];
        for (int i = 0; i < last.Count; i++)
        {
            _selection[i] = last[i];
        }

        _history.RemoveAt(_history.Count - 1);
        return true;
    }
}
