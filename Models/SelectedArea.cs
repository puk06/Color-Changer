using System.Collections;
using ColorChanger.Services;

namespace ColorChanger.Models;

internal class SelectedArea(int index, bool enabled, BitArray selectedPoints, bool isEraser)
{
    internal int Index { get; } = index;
    internal bool Enabled { get; set; } = enabled;
    internal bool IsEraser { get; set; } = isEraser;
    internal string CustomLayerName { get; set; } = string.Empty;
    internal BitArray SelectedPoints { get; } = selectedPoints;
    internal int Count { get; } = BitArrayHelper.CountOf(selectedPoints);

    internal string LayerName
        => string.IsNullOrEmpty(CustomLayerName) ? $"レイヤー {Index}" : CustomLayerName;
}
