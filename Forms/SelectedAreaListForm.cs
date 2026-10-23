using System.Collections;
using ColorChanger.Models;

namespace ColorChanger.Forms;

internal partial class SelectedAreaListForm : HideOnCloseForm
{
    /// <summary>
    /// 選択エリアの追加・削除・有効／無効の変更など、内容が変わったときに発生するイベント
    /// </summary>
    internal event EventHandler? LayersChanged;

    internal SelectionLayers Layers { get; } = new SelectionLayers();

    private readonly ContextMenuStrip _contextMenu = new ContextMenuStrip();
    private readonly InputDialogForm _inputDialogForm = new InputDialogForm("新しいレイヤー名を入力してください", "レイヤー名の変更");

    private int _rightClickedIndex = -1;
    private bool _suppressItemCheck = false;

    internal SelectedAreaListForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        _contextMenu.Items.Add("上へ移動").Click += (s, e) => MoveLayerUp();
        _contextMenu.Items.Add("下へ移動").Click += (s, e) => MoveLayerDown();
        _contextMenu.Items.Add("一番上へ移動").Click += (s, e) => MoveLayerToTop();
        _contextMenu.Items.Add("一番下へ移動").Click += (s, e) => MoveLayerToBottom();
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("選択用レイヤーに変換").Click += (s, e) => SetLayerEraser(false);
        _contextMenu.Items.Add("消去用レイヤーに変換").Click += (s, e) => SetLayerEraser(true);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("名前を変更").Click += (s, e) => RenameLayer();
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("レイヤーを削除").Click += (s, e) => RemoveLayer();

        selectedValuesList.ContextMenuStrip = _contextMenu;
    }

    /// <summary>
    /// 選択エリアを追加する。同じ内容のエリアがある場合は有効にする。
    /// </summary>
    internal void Add(BitArray values, bool isEraser = false)
    {
        Layers.Add(values, isEraser);
        Commit();
    }

    /// <summary>
    /// 最後に追加されたエリアを削除する。
    /// </summary>
    internal void RemoveLast()
    {
        if (Layers.Count == 0) return;

        Layers.RemoveLast();
        Commit();
    }

    /// <summary>
    /// 選択エリアをすべて削除する。
    /// </summary>
    internal void Clear()
    {
        Layers.Clear();
        RebuildList();
    }

    /// <summary>
    /// 一覧表示を更新し、変更を通知する
    /// </summary>
    private void Commit()
    {
        RebuildList();
        LayersChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// モデルの内容から一覧表示を作り直す
    /// </summary>
    private void RebuildList()
    {
        _suppressItemCheck = true;
        try
        {
            selectedValuesList.BeginUpdate();
            selectedValuesList.Items.Clear();

            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers.Items[i];
                selectedValuesList.Items.Add(FormatLayerText(layer, i + 1), layer.Enabled);
            }

            selectedValuesList.EndUpdate();
        }
        finally
        {
            _suppressItemCheck = false;
        }
    }

    private static string FormatLayerText(SelectedArea layer, int position)
        => $"{position}  |  {(layer.IsEraser ? "消去" : "選択")}  |  {layer.LayerName}  ({layer.Count:N0}px)";

    private bool IsValidRightClickedIndex
        => _rightClickedIndex >= 0 && _rightClickedIndex < Layers.Count;

    #region 右クリックメニュー処理
    private void MoveLayerUp()
    {
        if (_rightClickedIndex <= 0 || _rightClickedIndex >= Layers.Count) return;

        Layers.Move(_rightClickedIndex, _rightClickedIndex - 1);
        _rightClickedIndex--;
        Commit();
    }

    private void MoveLayerDown()
    {
        if (!IsValidRightClickedIndex || _rightClickedIndex >= Layers.Count - 1) return;

        Layers.Move(_rightClickedIndex, _rightClickedIndex + 1);
        _rightClickedIndex++;
        Commit();
    }

    private void MoveLayerToTop()
    {
        if (_rightClickedIndex <= 0 || _rightClickedIndex >= Layers.Count) return;

        Layers.Move(_rightClickedIndex, 0);
        _rightClickedIndex = 0;
        Commit();
    }

    private void MoveLayerToBottom()
    {
        if (!IsValidRightClickedIndex || _rightClickedIndex >= Layers.Count - 1) return;

        Layers.Move(_rightClickedIndex, Layers.Count - 1);
        _rightClickedIndex = Layers.Count - 1;
        Commit();
    }

    private void SetLayerEraser(bool isEraser)
    {
        if (!IsValidRightClickedIndex) return;

        Layers.SetEraser(_rightClickedIndex, isEraser);
        Commit();
    }

    private void RenameLayer()
    {
        if (!IsValidRightClickedIndex) return;

        var newName = _inputDialogForm.ShowDialog(Layers.Items[_rightClickedIndex].LayerName);
        if (!string.IsNullOrEmpty(newName))
        {
            Layers.Rename(_rightClickedIndex, newName);
        }

        RebuildList();
    }

    private void RemoveLayer()
    {
        if (!IsValidRightClickedIndex) return;

        var layer = Layers.Items[_rightClickedIndex];
        var result = FormUtils.ShowConfirm($"このレイヤーを削除しますか？\n\nレイヤー名: {layer.LayerName}\nピクセル数: {layer.Count:N0}");
        if (!result) return;

        Layers.RemoveAt(_rightClickedIndex);
        Commit();
    }
    #endregion

    #region イベントハンドラー
    private void SelectedValuesList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (_suppressItemCheck) return;
        if (e.Index < 0 || e.Index >= Layers.Count) return;

        Layers.SetEnabled(e.Index, e.NewValue == CheckState.Checked);
        LayersChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SelectedValuesList_MouseDown(object sender, MouseEventArgs e)
    {
        var clickedIndex = selectedValuesList.IndexFromPoint(e.Location);

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
    #endregion
}
