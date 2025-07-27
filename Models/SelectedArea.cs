using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Models;

internal class SelectedArea(int index, bool enabled, BitArray selectedPoints, bool isEraser)
{
    internal bool Enabled { get; set; } = enabled;
    internal int Index { get; private set; } = index;
    internal int Count { get; private set; } = BitArrayUtils.GetCount(selectedPoints);
    internal BitArray SelectedPoints { get; private set; } = selectedPoints;
    internal bool IsEraser { get; private set; } = isEraser;

    public override string ToString()
    {
        string label = IsEraser ? "消去エリア" : "選択エリア";
        return $"{label}{Index} : ピクセル数 {Count:N0}";
    }
}
