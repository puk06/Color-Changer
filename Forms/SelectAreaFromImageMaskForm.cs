using System.Collections;
using ColorChanger.Models;
using ColorChanger.Services;

namespace ColorChanger.Forms;

internal partial class SelectAreaFromImageMaskForm : HideOnCloseForm
{
    /// <summary>
    /// 選択エリアが確定したときに発生するイベント
    /// </summary>
    internal event EventHandler? AreaChanged;

    /// <summary>
    /// 現在選択されているエリア
    /// </summary>
    internal BitArray? SelectedArea { get; private set; }

    private ImageMaskSelectionType _selectionType;
    private Color _selectedColor = Color.Empty;

    private Bitmap? _source;
    private Bitmap? _preview;

    private readonly Throttle _updateThrottle = new Throttle(14);

    internal SelectAreaFromImageMaskForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        previewBox.BackColor = FormUtils.DefaultBackgroundColor;
        selectedColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        selectionType.SelectedIndex = 0;

        UpdateColorData();
    }

    private void SelectColor(object sender, MouseEventArgs e)
    {
        if (_selectionType != ImageMaskSelectionType.CustomColor) return;
        if (e.Button != MouseButtons.Left) return;
        if (_preview == null) return;

        if (!new Rectangle(Point.Empty, _preview.Size).Contains(e.Location)) return;
        if (!_updateThrottle.TryPass()) return;

        _selectedColor = _preview.GetPixel(e.X, e.Y);

        UpdateColorData();
    }

    private void LoadPictureFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("ファイルが存在しません。");
            return;
        }

        try
        {
            var source = ImageFileService.Load(path);
            var preview = PreviewService.CreateRawPreview(source, previewBox.Size);

            previewBox.Image = null;
            _source?.Dispose();
            _preview?.Dispose();

            _source = source;
            _preview = preview;
            previewBox.Image = _preview;
        }
        catch (Exception exception)
        {
            if (exception is ArgumentException)
            {
                FormUtils.ShowError("画像の読み込みに失敗しました。非対応のファイルです。");
            }
            else
            {
                FormUtils.ShowError("画像の読み込みに失敗しました。\n\nエラー: " + exception);
            }

            previewBox.Image = null;
            _source?.Dispose();
            _preview?.Dispose();
            _source = null;
            _preview = null;
        }
    }

    private void UpdateColorData()
    {
        selectedColorBox.BackColor = _selectedColor == Color.Empty
            ? FormUtils.DefaultBackgroundColor
            : _selectedColor;
    }

    #region イベントハンドラー
    private void SelectionType_SelectedIndexChanged(object sender, EventArgs e)
    {
        _selectionType = (ImageMaskSelectionType)selectionType.SelectedIndex;

        selectedColorLabel.Visible = _selectionType == ImageMaskSelectionType.CustomColor;
        selectedColorBox.Visible = _selectionType == ImageMaskSelectionType.CustomColor;
    }

    private void OpenFile_Click(object sender, EventArgs e)
    {
        var path = FormUtils.PickFile("画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;", "画像ファイルを選択してください");
        if (path != null) LoadPictureFile(path);
    }

    private void SelectArea_Click(object sender, EventArgs e)
    {
        if (_source == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        var selectedArea = SelectionService.SelectByMask(_source, _selectionType, _selectedColor);
        if (selectedArea.Length == 0)
        {
            FormUtils.ShowError("選択エリアがありません。");
            return;
        }

        var result = FormUtils.ShowConfirm($"マスク画像からの選択処理が完了しました\n\n総ピクセル数: {selectedArea.Length:N0}\n総選択ピクセル数: {BitArrayHelper.CountOf(selectedArea):N0}\n\n選択エリアとして追加しますか？");
        if (!result) return;

        SelectedArea = selectedArea;
        AreaChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SelectAreaFromImageMaskForm_DragDrop(object sender, DragEventArgs e)
    {
        var path = FormUtils.GetDroppedFile(e);
        if (path != null) LoadPictureFile(path);
    }

    private void SelectAreaFromImageMaskForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.All;
    #endregion
}
