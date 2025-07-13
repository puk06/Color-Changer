using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Diagnostics;

namespace ColorChanger.Forms;

public partial class SelectColorFromTextureForm : Form
{
    private const int COLOR_UPDATE_DEBOUNCE_MS = 14;

    /// <summary>
    /// 色が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ColorChanged;

    /// <summary>
    /// 現在選択されている色
    /// </summary>
    internal Color SelectedColor { get; private set; } = Color.Empty;

    private Bitmap? _previewBitmap;
    private Point _clickedPoint = Point.Empty;
    private readonly Stopwatch _updateDebounceStopwatch = Stopwatch.StartNew();

    public SelectColorFromTextureForm()
    {
        InitializeComponent();

        previewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        selectedColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

        UpdateColorData();
    }

    #region 色選択関連
    /// <summary>
    /// 変更前の色を選択する。
    /// </summary>
    private void SelectPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (_previewBitmap == null) return;

        if (!BitmapUtils.IsValidCoordinate(e.Location, _previewBitmap.Size)) return;

        if (_updateDebounceStopwatch.ElapsedMilliseconds <= COLOR_UPDATE_DEBOUNCE_MS) return;
        _updateDebounceStopwatch.Restart();

        Color selectedColor = _previewBitmap.GetPixel(e.X, e.Y);

        SelectedColor = selectedColor;

        UpdateColorData();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
    }
    #endregion

    #region 処理関連
    /// <summary>
    /// プレビュー用のビットマップを生成する
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <param name="rawMode"></param>
    /// <returns></returns>
    private Bitmap GenerateColoredPreview(Bitmap sourceBitmap, bool rawMode = false)
    {
        try
        {
            PreviewConfiguration previewConfig = new PreviewConfiguration
            {
                SourceBitmap = sourceBitmap,
                PreviewBoxSize = previewBox.Size,
                RawMode = rawMode
            };

            return ImageProcessorService.GeneratePreview(previewConfig);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"プレビュー画像の生成に失敗しました。\n{ex.Message}");
            return sourceBitmap;
        }
    }

    /// <summary>
    /// 画像ファイルを読み込む
    /// </summary>
    /// <param name="path"></param>
    private void LoadPictureFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("ファイルが存在しません。");
            return;
        }

        try
        {
            BitmapUtils.DisposeBitmap(ref _previewBitmap);

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var rawBitmap = new Bitmap(stream);
            _previewBitmap = GenerateColoredPreview(rawBitmap, rawMode: true);

            BitmapUtils.DisposeBitmap(ref rawBitmap);
            BitmapUtils.SetImage(previewBox, _previewBitmap, disposeImage: false);
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

            BitmapUtils.DisposeBitmap(ref _previewBitmap);
            BitmapUtils.ResetImage(previewBox);
        }
    }

    /// <summary>
    /// 色情報を更新する
    /// </summary>
    private void UpdateColorData()
    {
        if (SelectedColor == Color.Empty)
        {
            RGBText.Text = "未選択";
            ColorCodeText.Text = "未選択";
            selectedColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        }
        else
        {
            RGBText.Text = $"{SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B}";
            ColorCodeText.Text = ColorUtils.GetColorCodeFromColor(SelectedColor);
            selectedColorBox.BackColor = SelectedColor;
        }
    }
    #endregion

    #region イベントハンドラー
    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;
        if (sender is not PictureBox) return;

        Color inverseColor = ColorUtils.InverseColor(SelectedColor);

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
        e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
    }

    private void OpenFile_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog()
        {
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;",
            Title = "画像ファイルを選択してください"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadPictureFile(dialog.FileName);
    }

    private void SelectColorFromTextureForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
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

        NotifyColorChanged();
    }

    private void NotifyColorChanged()
        => ColorChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void SelectColorFromTextureForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
