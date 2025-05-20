using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Models;

internal class SelectedArea(int index, bool enabled, BitArray selectedPoints)
{
    internal bool Enabled { get; set; } = enabled;
    internal int Index { get; private set; } = index;
    internal int Count { get; private set; } = BitArrayUtils.GetCount(selectedPoints);
    internal BitArray SelectedPoints { get; private set; } = selectedPoints;

    public override string ToString()
        => $"選択エリア{Index} : ピクセル数 {Count:N0}";
}
