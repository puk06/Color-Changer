using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Diagnostics;

namespace ColorChanger.Forms;

public partial class SelectAreaFromImageMaskForm : Form
{
    private const int COLOR_UPDATE_DEBOUNCE_MS = 14;

    /// <summary>
    /// 選択エリアが変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? AreaChanged;

    /// <summary>
    /// 現在選択されているエリア
    /// </summary>
    internal BitArray? SelectedArea { get; private set; }

    private ImageMaskSelectionType _imageMaskSelectionType;
    private Color _selectedColor = Color.Empty;

    private Bitmap? _bmp;
    private Bitmap? _previewBitmap;

    private readonly Stopwatch _updateDebounceStopwatch = Stopwatch.StartNew();

    public SelectAreaFromImageMaskForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        previewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        selectedColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        selectionType.SelectedIndex = 0;

        UpdateColorData();
    }

    #region 色選択関連
    /// <summary>
    /// 変更前の色を選択する。
    /// </summary>
    private void SelectColor(object sender, MouseEventArgs e)
    {
        if (_imageMaskSelectionType != ImageMaskSelectionType.CustomColor) return;
        if (e.Button != MouseButtons.Left) return;
        if (_previewBitmap == null) return;

        if (!BitmapUtils.IsValidCoordinate(e.Location, _previewBitmap.Size)) return;

        if (_updateDebounceStopwatch.ElapsedMilliseconds <= COLOR_UPDATE_DEBOUNCE_MS) return;
        _updateDebounceStopwatch.Restart();

        Color selectedColor = _previewBitmap.GetPixel(e.X, e.Y);

        _selectedColor = selectedColor;

        UpdateColorData();
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
            BitmapUtils.DisposeBitmap(ref _bmp);
            BitmapUtils.DisposeBitmap(ref _previewBitmap);

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _bmp = new Bitmap(stream);
            _previewBitmap = GenerateColoredPreview(_bmp, rawMode: true);

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

            BitmapUtils.DisposeBitmap(ref _bmp);
            BitmapUtils.DisposeBitmap(ref _previewBitmap);
            BitmapUtils.ResetImage(previewBox);
        }
    }

    /// <summary>
    /// 色情報を更新する
    /// </summary>
    private void UpdateColorData()
    {
        if (_selectedColor == Color.Empty)
        {
            selectedColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        }
        else
        {
            selectedColorBox.BackColor = _selectedColor;
        }
    }
    #endregion

    #region イベントハンドラー
    private void SelectionType_SelectedIndexChanged(object sender, EventArgs e)
    {
        _imageMaskSelectionType = (ImageMaskSelectionType)selectionType.SelectedIndex;

        selectedColorLabel.Visible = _imageMaskSelectionType == ImageMaskSelectionType.CustomColor;
        selectedColorBox.Visible = _imageMaskSelectionType == ImageMaskSelectionType.CustomColor;
    }

    private void NotifyAreaChanged()
        => AreaChanged?.Invoke(this, EventArgs.Empty);

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

    private void SelectArea_Click(object sender, EventArgs e)
    {
        if (_bmp == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        BitArray? selectedArea = BitmapUtils.GetSelectedArea(_bmp, _imageMaskSelectionType, _selectedColor);
        if (selectedArea == null || selectedArea.Length == 0)
        {
            FormUtils.ShowError("選択エリアがありません。");
            return;
        }

        bool result = FormUtils.ShowConfirm($"マスク画像からの選択処理が完了しました\n\n総ピクセル数: {selectedArea.Length:N0}\n総選択ピクセル数: {BitArrayUtils.GetCount(selectedArea, true):N0}\n\n選択エリアとして追加しますか？");
        if (result)
        {
            SelectedArea = selectedArea;
            NotifyAreaChanged();
        }
    }

    private void SelectAreaFromImageMaskForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
    }

    private void SelectAreaFromImageMaskForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.All;
    #endregion

    #region フォーム関連
    private void ImageMaskForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
