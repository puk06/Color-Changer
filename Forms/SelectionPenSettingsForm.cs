using System.Collections;
using ColorChanger.Models;
using ColorChanger.Services;

namespace ColorChanger.Forms;

internal partial class SelectionPenSettingsForm : HideOnCloseForm
{
    private const string UNDO_BUTTON_TEXT = "元に戻す";

    private readonly PenSelection _pen = new PenSelection();

    /// <summary>
    /// ペンでの選択が確定し、レイヤーとして追加されるときに発生するイベント
    /// </summary>
    internal event Action<BitArray, bool>? LayerSubmitted;

    /// <summary>
    /// 選択状態が変わったとき（確定・取り消し・元に戻す）に発生するイベント
    /// </summary>
    internal event Action? SelectionChanged;

    /// <summary>
    /// ペンが有効で、画面上でなぞれる状態かどうか
    /// </summary>
    internal bool PenEnabled => Visible && enablePen.Checked;

    internal bool IsInitialized => _pen.IsInitialized;

    private int PenWidth => penWidth.Value;

    internal SelectionPenSettingsForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        RefreshPenWidthLabel();
        RefreshUndoButton();
    }

    /// <summary>
    /// 選択エリアを初期化します。
    /// </summary>
    internal void Initialize(Size imageSize)
    {
        _pen.Initialize(imageSize);
        RefreshUndoButton();
    }

    /// <summary>
    /// 選択エリアをリセットします。リセット後はInitializeの実行が必要になります。
    /// </summary>
    internal void Reset()
    {
        _pen.Reset();
        RefreshUndoButton();
    }

    /// <summary>
    /// 画像座標の中心点にペンを当てて、選択エリアを更新する
    /// </summary>
    internal void PaintAt(Point center)
    {
        _pen.Paint(center, PenWidth, !eraserMode.Checked);
        RefreshUndoButton();
    }

    /// <summary>
    /// ストロークを終了する
    /// </summary>
    internal void EndStroke()
        => _pen.EndStroke();

    /// <summary>
    /// 現在の選択状態を、表示サイズに縮小したマップとして取得する
    /// </summary>
    internal bool[,] BuildPreviewMap(Size previewSize)
        => SelectionService.BuildPreviewMap(_pen.Selection, _pen.Width, _pen.Height, previewSize);

    private void Finish(bool? isEraserLayer)
    {
        if (isEraserLayer is bool isEraser && _pen.IsInitialized)
        {
            LayerSubmitted?.Invoke(new BitArray(_pen.Selection), isEraser);
        }

        Reset();
        SelectionChanged?.Invoke();
    }

    private void UndoSelection()
    {
        if (!_pen.Undo()) return;

        RefreshUndoButton();
        SelectionChanged?.Invoke();
    }

    private void RefreshPenWidthLabel()
        => penWidthLabel.Text = $"- {penWidth.Value}px";

    private void RefreshUndoButton()
    {
        undo.Text = $"{UNDO_BUTTON_TEXT} - {_pen.HistoryCount}個の履歴";
        undo.Enabled = _pen.HistoryCount != 0;
    }

    #region イベントハンドラー
    private void PenWidth_ValueChanged(object sender, EventArgs e)
        => RefreshPenWidthLabel();

    private void Undo_Click(object sender, EventArgs e)
        => UndoSelection();

    private void SelectionPenSettingsForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            UndoSelection();
        }
    }

    private void AddLayer_Click(object sender, EventArgs e)
        => Finish(false);

    private void AddEraserLayer_Click(object sender, EventArgs e)
        => Finish(true);

    private void CancelSelection_Click(object sender, EventArgs e)
    {
        var result = FormUtils.ShowConfirm("現在のペンの選択状況を全て無視して、取り消しますか？");
        if (!result) return;

        Finish(null);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        Finish(null);
    }
    #endregion
}
