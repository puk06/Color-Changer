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

    private int _selectedEraserAreaCount = 0;

    /// <summary>
    /// 現在有効な消去エリアの数
    /// </summary>
    internal int EnabledEraserAreaCount => _selectedEraserAreaCount;

    private readonly List<SelectedArea> _selectedAreas = new List<SelectedArea>();

    /// <summary>
    /// 現在選択されているすべてのエリア
    /// </summary>
    internal BitArray SelectedArea
    {
        get
        {
            _selectedAreaCount = 0;
            _selectedEraserAreaCount = 0;
            BitArray selectedArea = BitArrayUtils.GetEmpty();

            for (int i = _selectedAreas.Count - 1; i >= 0; i--)
            {
                var item = _selectedAreas[i];

                if (item.Enabled)
                {
                    BitArrayUtils.Merge(ref selectedArea, item.SelectedPoints, item.IsEraser);

                    if (item.IsEraser)
                    {
                        _selectedEraserAreaCount++;
                    }
                    else
                    {
                        _selectedAreaCount++;
                    }
                }
            }

            return selectedArea;
        }
    }

    private static readonly ContextMenuStrip _contextMenu = new ContextMenuStrip();
    private int _rightClickedIndex = -1;
    private readonly InputDialogForm _inputDialogForm = new InputDialogForm("新しいレイヤー名を入力してください", "レイヤー名の変更");

    internal SelectedAreaListForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        _contextMenu.Items.Add("上へ移動").Click += MoveLayerUp;
        _contextMenu.Items.Add("下へ移動").Click += MoveLayerDown;
        _contextMenu.Items.Add("一番上へ移動").Click += MoveCheckedListBoxItemToTop;
        _contextMenu.Items.Add("一番下へ移動").Click += MoveCheckedListBoxItemToBottom;
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("選択用レイヤーに変換").Click += MakeSelectionLayer;
        _contextMenu.Items.Add("消去用レイヤーに変換").Click += MakeEraserLayer;
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("名前を変更").Click += RenameLayer;
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("レイヤーを削除").Click += RemoveLayer;

        selectedValuesList.ContextMenuStrip = _contextMenu;
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

        RefleshPriorityText();
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

        int index = 1;
        while (_selectedAreas.Any(selectedArea => selectedArea.Index == index))
        {
            index++;
        }

        SelectedArea selectedArea = new SelectedArea(index, true, values, isEraser);

        _selectedAreas.Add(selectedArea);
        selectedValuesList.Items.Add(selectedArea.ToString(), true);

        RefleshPriorityText();
    }

    private void RefleshPriorityText()
    {
        for (int i = 0; i < selectedValuesList.Items.Count; i++)
        {
            selectedValuesList.Items[i] = $"{i + 1}  |  {_selectedAreas[i]}";
        }
    }

    #region 右クリックメニュー処理
    private void MoveLayerUp(object? sender, EventArgs e)
    {
        if (_rightClickedIndex <= 0) return;

        SwapLayers(_rightClickedIndex, _rightClickedIndex - 1);
        _rightClickedIndex--;

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void MoveLayerDown(object? sender, EventArgs e)
    {
        if (_rightClickedIndex == -1 || _rightClickedIndex >= _selectedAreas.Count - 1) return;

        SwapLayers(_rightClickedIndex, _rightClickedIndex + 1);
        _rightClickedIndex++;

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void MoveCheckedListBoxItemToTop(object? sender, EventArgs e)
    {
        if (_rightClickedIndex <= 0) return;

        for (int i = _rightClickedIndex; i > 0; i--)
        {
            SwapLayers(i, i - 1);
        }

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void MoveCheckedListBoxItemToBottom(object? sender, EventArgs e)
    {
        int last = _selectedAreas.Count - 1;
        if (_rightClickedIndex == -1 || _rightClickedIndex >= last) return;

        for (int i = _rightClickedIndex; i < last; i++)
        {
            SwapLayers(i, i + 1);
        }

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void SwapLayers(int index1, int index2)
    {
        (_selectedAreas[index1], _selectedAreas[index2]) = (_selectedAreas[index2], _selectedAreas[index1]);
        (selectedValuesList.Items[index2], selectedValuesList.Items[index1]) = (selectedValuesList.Items[index1], selectedValuesList.Items[index2]);
    }

    private void MakeSelectionLayer(object? sender, EventArgs e)
    {
        if (_rightClickedIndex == -1) return;

        _selectedAreas[_rightClickedIndex].IsEraser = false;

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void MakeEraserLayer(object? sender, EventArgs e)
    {
        if (_rightClickedIndex == -1) return;

        _selectedAreas[_rightClickedIndex].IsEraser = true;

        RefleshPriorityText();
        TriggerCheckedChanged();
    }

    private void RenameLayer(object? sender, EventArgs e)
    {
        if (_rightClickedIndex == -1) return;

        SelectedArea area = _selectedAreas[_rightClickedIndex];

        string? newName = _inputDialogForm.ShowDialog(area.GetLayerName());

        if (!string.IsNullOrEmpty(newName))
        {
            area.CustomLayerName = newName;
        }

        RefleshPriorityText();
    }

    private void RemoveLayer(object? sender, EventArgs e)
    {
        if (_rightClickedIndex == -1) return;

        SelectedArea selectedArea = _selectedAreas[_rightClickedIndex];
        bool result = FormUtils.ShowConfirm($"このレイヤーを削除しますか？\n\nレイヤー名: {selectedArea.GetLayerName()}\nピクセル数: {selectedArea.Count:N0}");
        if (!result) return;

        _selectedAreas.RemoveAt(_rightClickedIndex);
        selectedValuesList.Items.RemoveAt(_rightClickedIndex);

        RefleshPriorityText();
        TriggerCheckedChanged();
    }
    #endregion

    #region イベントハンドラー
    private void SelectedValuesList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        bool isChecked = e.NewValue == CheckState.Checked;
        int index = e.Index;

        if (index < 0 || index >= _selectedAreas.Count) return;

        _selectedAreas[index].Enabled = isChecked;

        TriggerCheckedChanged();
    }

    private void SelectedValuesList_MouseDown(object sender, MouseEventArgs e)
    {
        int clickedIndex = selectedValuesList.IndexFromPoint(e.Location);

        if (e.Button == MouseButtons.Right)
        {
            _rightClickedIndex = clickedIndex;
        }
        else if (e.Button == MouseButtons.Left && clickedIndex != -1)
        {
            selectedValuesList.SetItemChecked(clickedIndex, !selectedValuesList.GetItemChecked(clickedIndex));
        }
    }

    private void SelectedValuesList_SelectedIndexChanged(object sender, EventArgs e)
        => selectedValuesList.SelectedIndex = -1;

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
