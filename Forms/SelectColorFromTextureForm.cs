using ColorChanger.Services;

namespace ColorChanger.Forms;

internal partial class SelectColorFromTextureForm : HideOnCloseForm
{
    /// <summary>
    /// 色が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ColorChanged;

    /// <summary>
    /// 現在選択されている色
    /// </summary>
    internal Color SelectedColor { get; private set; } = Color.Empty;

    private Bitmap? _preview;
    private Point _clickedPoint = Point.Empty;
    private readonly Throttle _updateThrottle = new Throttle(14);

    internal SelectColorFromTextureForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        previewBox.BackColor = FormUtils.DefaultBackgroundColor;
        selectedColorBox.BackColor = FormUtils.DefaultBackgroundColor;

        UpdateColorData();
    }

    private void SelectPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (_preview == null) return;

        if (!new Rectangle(Point.Empty, _preview.Size).Contains(e.Location)) return;
        if (!_updateThrottle.TryPass()) return;

        SelectedColor = _preview.GetPixel(e.X, e.Y);

        UpdateColorData();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
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
            using var source = ImageFileService.Load(path);
            var preview = PreviewService.CreateRawPreview(source, previewBox.Size);

            previewBox.Image = null;
            _preview?.Dispose();

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
            _preview?.Dispose();
            _preview = null;
        }
    }

    private void UpdateColorData()
    {
        if (SelectedColor == Color.Empty)
        {
            RGBText.Text = "未選択";
            ColorCodeText.Text = "未選択";
            selectedColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        }
        else
        {
            RGBText.Text = $"{SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B}";
            ColorCodeText.Text = ColorService.GetColorCodeFromColor(SelectedColor);
            selectedColorBox.BackColor = SelectedColor;
        }
    }

    #region イベントハンドラー
    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;
        if (sender is not PictureBox) return;

        FormUtils.DrawCrosshair(e.Graphics, _clickedPoint, SelectedColor);
    }

    private void OpenFile_Click(object sender, EventArgs e)
    {
        var path = FormUtils.PickFile("画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;", "画像ファイルを選択してください");
        if (path != null) LoadPictureFile(path);
    }

    private void SelectColorFromTextureForm_DragDrop(object sender, DragEventArgs e)
    {
        var path = FormUtils.GetDroppedFile(e);
        if (path != null) LoadPictureFile(path);
    }

    private void SelectColorFromTextureForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.All;

    private void ApplyColor_Click(object sender, EventArgs e)
    {
        if (SelectedColor == Color.Empty)
        {
            FormUtils.ShowError("色が選択されていません。");
            return;
        }

        ColorChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}
