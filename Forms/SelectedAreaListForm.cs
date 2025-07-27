using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Forms;

internal partial class SelectedAreaListForm : Form
{
    /// <summary>
    /// 選択エリアの有効／無効が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? OnCheckedChanged;

    private int _selectedAreaCount = 0;

    /// <summary>
    /// 現在有効な選択エリアの数
    /// </summary>
    internal int EnabledCount => _selectedAreaCount;

    private readonly List<SelectedArea> _selectedAreas = new List<SelectedArea>();

    /// <summary>
    /// 現在選択されているすべてのエリア
    /// </summary>
    internal BitArray SelectedArea
    {
        get
        {
            _selectedAreaCount = 0;
            BitArray selectedArea = BitArrayUtils.GetEmpty();

            foreach (var item in _selectedAreas)
            {
                if (item.Enabled)
                {
                    BitArrayUtils.Merge(ref selectedArea, item.SelectedPoints, item.IsEraser);
                    _selectedAreaCount++;
                }
            }

            return selectedArea;
        }
    }

    internal SelectedAreaListForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();
    }

    /// <summary>
    /// 選択エリアをクリアします。
    /// </summary>
    internal void Clear()
    {
        _selectedAreas.Clear();
        selectedValuesList.Items.Clear();
    }

    /// <summary>
    /// 最後に追加されたものを削除します。
    /// </summary>
    internal void RemoveLast()
    {
        if (_selectedAreas.Count == 0) return;

        int lastIndex = _selectedAreas.Count - 1;

        _selectedAreas.RemoveAt(lastIndex);
        selectedValuesList.Items.RemoveAt(lastIndex);

        TriggerCheckedChanged();
    }

    /// <summary>
    /// 渡されたBitArrayを選択エリアとして追加します。
    /// </summary>
    /// <param name="values"></param>
    /// <param name="isEraser"></param>
    internal void Add(BitArray values, bool isEraser = false)
    {
        for (int i = 0; i < _selectedAreas.Count; i++)
        {
            if (_selectedAreas[i].IsEraser == isEraser && BitArrayUtils.Equals(_selectedAreas[i].SelectedPoints, values))
            {
                selectedValuesList.SetItemChecked(i, true);
                return;
            }
        }

        int itemIndex = selectedValuesList.Items.Count + 1;
        SelectedArea selectedArea = new SelectedArea(itemIndex, true, values, isEraser);

        _selectedAreas.Add(selectedArea);
        string title = selectedArea.ToString();
        selectedValuesList.Items.Add(title, true);
    }

    #region イベントハンドラー
    private void SelectedValuesList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        bool isChecked = e.NewValue == CheckState.Checked;
        int index = e.Index;

        if (index < 0 || index >= _selectedAreas.Count) return;

        SelectedArea selectedArea = _selectedAreas[index];
        selectedArea.Enabled = isChecked;

        TriggerCheckedChanged();
    }

    private void TriggerCheckedChanged()
        => OnCheckedChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void SelectedAreaListForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
