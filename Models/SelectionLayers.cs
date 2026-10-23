using System.Collections;
using ColorChanger.Services;

namespace ColorChanger.Models;

/// <summary>
/// 選択エリア（レイヤー）の一覧を管理する。先頭ほど優先度が低い。
/// </summary>
internal class SelectionLayers
{
    private readonly List<SelectedArea> _items = new List<SelectedArea>();

    internal IReadOnlyList<SelectedArea> Items => _items;

    internal int Count => _items.Count;

    internal int EnabledSelectionCount => _items.Count(item => item.Enabled && !item.IsEraser);

    internal int EnabledEraserCount => _items.Count(item => item.Enabled && item.IsEraser);

    /// <summary>
    /// レイヤーを追加する。同じ内容のレイヤーが既にある場合は有効化する。
    /// </summary>
    /// <returns>追加、または有効化したレイヤーのインデックス</returns>
    internal int Add(BitArray values, bool isEraser)
    {
        int existing = _items.FindIndex(item => item.IsEraser == isEraser && BitArrayHelper.AreEqual(item.SelectedPoints, values));
        if (existing >= 0)
        {
            _items[existing].Enabled = true;
            return existing;
        }

        int index = 1;
        while (_items.Any(item => item.Index == index)) index++;

        _items.Add(new SelectedArea(index, true, values, isEraser));
        return _items.Count - 1;
    }

    internal void RemoveAt(int index)
        => _items.RemoveAt(index);

    internal void RemoveLast()
    {
        if (_items.Count == 0) return;
        _items.RemoveAt(_items.Count - 1);
    }

    internal void Move(int fromIndex, int toIndex)
    {
        SelectedArea item = _items[fromIndex];
        _items.RemoveAt(fromIndex);
        _items.Insert(toIndex, item);
    }

    internal void SetEnabled(int index, bool enabled)
        => _items[index].Enabled = enabled;

    internal void SetEraser(int index, bool isEraser)
        => _items[index].IsEraser = isEraser;

    internal void Rename(int index, string name)
        => _items[index].CustomLayerName = name;

    internal void Clear()
        => _items.Clear();

    /// <summary>
    /// 有効なレイヤーを下から順に合成した選択エリアを返す。
    /// </summary>
    internal BitArray Combine()
    {
        BitArray combined = BitArrayHelper.Empty();

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            SelectedArea item = _items[i];
            if (!item.Enabled) continue;

            combined = BitArrayHelper.Merge(combined, item.SelectedPoints, item.IsEraser);
        }

        return combined;
    }
}
