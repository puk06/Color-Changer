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
    => string.IsNullOrEmpty(CustomLayerName) ? $"{(IsEraser ? "消去レイヤー" : "選択レイヤー")} {Index}" : CustomLayerName;


    public override string ToString()
        => $"{GetLayerName()}  /  ピクセル数: {Count:N0}";
}
