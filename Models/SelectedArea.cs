using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Models;

internal class SelectedArea(int index, bool enabled, BitArray selectedPoints, bool isEraser)
{
    internal bool Enabled { get; set; } = enabled;
    internal string CustomLayerName { get; set; } = string.Empty;
    internal int Index { get; private set; } = index;
    internal int Count { get; private set; } = BitArrayUtils.GetCount(selectedPoints);
    internal BitArray SelectedPoints { get; private set; } = selectedPoints;
    internal bool IsEraser { get; set; } = isEraser;

    internal string GetLayerName()
        => $"{(string.IsNullOrEmpty(CustomLayerName) ? $"レイヤー {Index}" : CustomLayerName)}";

    internal string ToString(int index)
        => $"{(index == -1 ? "N/A" : index)}  |  {(IsEraser ? "消去" : "選択")}  |  {GetLayerName()}  ({Count:N0}px)";
}
