using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace ColorChanger.Forms;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.12";
    private static readonly string FORM_TITLE = $"Color Changer For Texture {CURRENT_VERSION}";
    private static readonly string ITEM_URL = "https://pukorufu.booth.pm/items/6519471";
    private static readonly Point VERSION_LABEL_POSITION = new Point(275, 54);

    private readonly ProcessStartInfo _processStartInfo = new ProcessStartInfo()
    {
        FileName = ITEM_URL,
        UseShellExecute = true
    };

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Color _backgroundColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;

    private Bitmap? _bmp;
    private string? _imageFilePath;

    private BitArray _selectedPointsForPreview = BitArrayUtils.GetEmpty();
    private Bitmap? _previewBitmap;

    private readonly ColorPickerForm _colorPickerForm = new ColorPickerForm();
    private readonly BalanceModeSettingsForm _balanceModeSettingsForm = new BalanceModeSettingsForm();
    private readonly SelectedAreaListForm _selectedAreaListForm = new SelectedAreaListForm();
    private readonly HelpForm _helpForm = new HelpForm();
    private readonly AdvancedColorSettingsForm _advancedColorSettingsForm = new AdvancedColorSettingsForm();

    private readonly ColorDifference _colorDifference = new ColorDifference(Color.Empty, Color.Empty);
    private ColorDifference ColorDifference
    {
        get
        {
            _colorDifference.Set(_previousColor, _newColor);
            return _colorDifference;
        }
    }

    public MainForm()
    {
        InitializeComponent();

        Text = FORM_TITLE;

        versionLabel.Text = CURRENT_VERSION;
        FormUtils.AlignTextRight(versionLabel, VERSION_LABEL_POSITION);

        previousColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        newColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        previewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        coloredPreviewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        backgroundColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

        SetupEventHandlers();
    }

    #region 色選択関連
    /// <summary>
    /// 変更前の色を選択する。
    /// </summary>
    private void SelectPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;
        if (_bmp == null || _previewBitmap == null) return;

        if (!BitmapUtils.IsValidCoordinate(e.Location, _previewBitmap.Size)) return;

        Color selectedColor = _previewBitmap.GetPixel(e.X, e.Y);

        if (selectMode.Checked)
        {
            Point originalCoordinates = BitmapUtils.ConvertToOriginalCoordinates(e, previewBox, _bmp);
            HandleSelectionMode(e, selectedColor, originalCoordinates);
            return;
        }

        previousColorBox.BackColor = selectedColor;
        _previousColor = selectedColor;
        _colorPickerForm.SetInitialColor(selectedColor);

        previousRGBLabel.Text = $"{selectedColor.R}, {selectedColor.G}, {selectedColor.B}";
        UpdateCalculatedRGBValue();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
        coloredPreviewBox.Invalidate();
    }

    /// <summary>
    /// 計算後のRGB値を更新する
    /// </summary>
    private void UpdateCalculatedRGBValue()
    {
        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = string.Empty;
            return;
        }

        calculatedRGBLabel.Text = ColorDifference.ToString();
    }
    #endregion

    #region 処理関連
    /// <summary>
    /// 色を変更する
    /// </summary>
    /// <param name="filePath"></param>
    private void ApplyColorChange(string filePath)
    {
        try
        {
            if (_bmp == null) return;

            Rectangle rect = BitmapUtils.GetRectangle(_bmp);

            Bitmap bitMap = BitmapUtils.CopyBitmap(_bmp);
            BitmapData? data = BitmapUtils.LockBitmap(bitMap, rect, ImageLockMode.ReadWrite);
            if (data == null)
            {
                bitMap.Dispose();
                return;
            }

            Bitmap? rawBitmap = BitmapUtils.CreateInverseBitmap(_bmp, inverseMode.Checked);
            BitmapData? rawData = BitmapUtils.LockBitmap(rawBitmap, rect, ImageLockMode.ReadOnly);

            Bitmap? transBitmap = BitmapUtils.CreateTransparentBitmap(_bmp, transMode.Checked, inverseMode.Checked);
            BitmapData? transData = BitmapUtils.LockBitmap(transBitmap, rect, ImageLockMode.ReadWrite);

            bool skipped = false;

            try
            {
                unsafe
                {
                    Span<ColorPixel> sourcePixels = BitmapUtils.GetPixelSpan(data);
                    Span<ColorPixel> rawPixels = BitmapUtils.GetPixelSpan(rawData);
                    Span<ColorPixel> transPixels = BitmapUtils.GetPixelSpan(transData);

                    var imageProcessor = new ImageProcessor(bitMap.Size, ColorDifference);
                    imageProcessor.SetColorSettings(_advancedColorSettingsForm.Configuration);

                    if (balanceMode.Checked)
                        imageProcessor.SetBalanceSettings(_balanceModeSettingsForm.Configuration);

                    skipped = ProcessImage(sourcePixels, rawPixels, transPixels, imageProcessor);
                }
            }
            finally
            {
                bitMap.UnlockBits(data);
                if (rawData != null && rawBitmap != null) rawBitmap.UnlockBits(rawData);
                if (transData != null && transBitmap != null) transBitmap.UnlockBits(transData);
            }

            rawBitmap?.Dispose();

            if (!skipped && transMode.Checked && !inverseMode.Checked)
            {
                if (transBitmap != null)
                {
                    transBitmap.Save(filePath);
                    transBitmap.Dispose();
                }
                else
                {
                    FormUtils.ShowError("透過用画像が作成できませんでした。");
                }
            }
            else
            {
                bitMap.Save(filePath);
            }

            bitMap.Dispose();
            FormUtils.ShowInfo("テクスチャ画像の作成が完了しました。");
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"テクスチャ画像作成中にエラーが発生しました。\n{ex}");
        }
        finally
        {
            makeButton.Enabled = true;
            makeButton.Text = "作成";
        }
    }

    /// <summary>
    /// 画像を処理する
    /// </summary>
    /// <param name="sourcePixels"></param>
    /// <param name="rawPixels"></param>
    /// <param name="transPixels"></param>
    /// <param name="processor"></param>
    /// <returns></returns>
    private bool ProcessImage(
        Span<ColorPixel> sourcePixels,
        Span<ColorPixel> rawPixels,
        Span<ColorPixel> transPixels,
        ImageProcessor processor
    )
    {
        bool skipped = false;

        BitArray selectedPoints = _selectedAreaListForm.SelectedArea;

        if (selectedPoints.Length == 0)
        {
            if (transMode.Checked)
            {
                FormUtils.ShowInfo("選択エリアがなかったため、透過モードはスキップされます。");
                skipped = true;
            }

            processor.ProcessAllPixels(sourcePixels, sourcePixels);
        }
        else if (transMode.Checked)
        {
            if (inverseMode.Checked)
            {
                processor.ProcessTransparentAndInversePixels(sourcePixels, selectedPoints);
            }
            else
            {
                if (transPixels.IsEmpty)
                    FormUtils.ShowError("透過画像用データの取得に失敗しました。デフォルトの画像が使用されます。");

                processor.ProcessTransparentSelectedPixels(sourcePixels, transPixels, selectedPoints);
            }
        }
        else if (inverseMode.Checked)
        {
            processor.ProcessAllPixels(sourcePixels, sourcePixels);

            if (rawPixels.IsEmpty)
            {
                FormUtils.ShowError("元画像のデータの取得に失敗しました。選択反転モードの結果は作成されません。");
            }
            else
            {
                ImageProcessor.ProcessInverseSelectedPixels(sourcePixels, rawPixels, selectedPoints);
            }
        }
        else
        {
            processor.ProcessSelectedPixels(sourcePixels, sourcePixels, selectedPoints);
        }

        return skipped;
    }

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
            return ImageProcessorService.GeneratePreview(
                sourceBitmap,
                ColorDifference,
                balanceMode.Checked, _balanceModeSettingsForm.Configuration,
                _advancedColorSettingsForm.Configuration,
                _selectedPointsForPreview,
                coloredPreviewBox.Size,
                rawMode
            );
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

            _imageFilePath = path;
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

            _imageFilePath = null;
        }
        finally
        {
            BitmapUtils.ResetImage(coloredPreviewBox);

            _previousColor = Color.Empty;
            _newColor = Color.Empty;
            _backgroundColor = Color.Empty;

            previousColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            newColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            backgroundColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

            _clickedPoint = Point.Empty;

            previousRGBLabel.Text = string.Empty;
            newRGBLabel.Text = string.Empty;
            calculatedRGBLabel.Text = string.Empty;

            estimatedMemoryUsageBase.Text = string.Empty;
            estimatedMemoryUsageCount.Text = string.Empty;
            estimatedMemoryUsageTotal.Text = string.Empty;

            _selectedPointsForPreview = BitArrayUtils.GetEmpty();
            _selectedAreaListForm.Clear();

            Text = FORM_TITLE;
            selectMode.Checked = false;
            UpdateEstimatedMemoryUsage();
        }
    }
    #endregion

    #region 選択モード関連
    /// <summary>
    /// 選択モードの処理
    /// </summary>
    private void HandleSelectionMode(MouseEventArgs e, Color color, Point originalCoordinates)
    {
        if (_bmp == null) return;
        if (e.Button == MouseButtons.Right)
        {
            _backgroundColor = color;
            backgroundColorBox.BackColor = color;
            return;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            FormUtils.ShowError("色が選択されていません。（プレビューが作成できません）");
            return;
        }

        if (_backgroundColor == Color.Empty)
        {
            FormUtils.ShowError("背景色が選択されていません。");
            return;
        }

        string previousFormTitle = Text;
        Text = FORM_TITLE + " - 選択処理中...";

        BitArray? values = BitmapUtils.GetSelectedArea(
            originalCoordinates,
            _bmp,
            _backgroundColor
        );

        Text = previousFormTitle;

        if (values == null || values.Length == 0)
        {
            FormUtils.ShowError("選択エリアがありません。");
            return;
        }

        _selectedAreaListForm.Add(values);
    }
    #endregion

    #region イベントハンドラー
    /// <summary>
    /// イベントハンドラのセットアップ
    /// </summary>
    private void SetupEventHandlers()
    {
        _balanceModeSettingsForm.ConfigurationChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        _colorPickerForm.ColorChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty) return;

            Color color = _colorPickerForm.SelectedColor;

            _newColor = color;
            newColorBox.BackColor = color;
            newRGBLabel.Text = $"{color.R}, {color.G}, {color.B}";

            UpdateCalculatedRGBValue();

            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        _selectedAreaListForm.OnCheckedChanged += (s, e)
            => UpdateSelectedArea();

        _advancedColorSettingsForm.ConfigurationChanged += (s, e) =>
        {
            _advancedColorSettingsForm.Configuration.UpdateComponentActivationStatus();
            advancedColorConfigStatus.Text = _advancedColorSettingsForm.Configuration.Enabled ? "有効" : "無効";
            
            if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };
    }

    /// <summary>
    /// 選択エリアを更新する
    /// </summary>
    private void UpdateSelectedArea()
    {
        if (_bmp == null || _previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitArray allSelectedAreas = _selectedAreaListForm.SelectedArea;
        int enabledCount = _selectedAreaListForm.EnabledCount;

        if (allSelectedAreas.Length == 0)
        {
            _selectedPointsForPreview = BitArrayUtils.GetEmpty();

            Text = FORM_TITLE;
        }
        else
        {
            Text = FORM_TITLE + " - プレビュー用の選択エリア作成中...";
            _selectedPointsForPreview = BitmapUtils.ConvertSelectedAreaToPreviewBox(allSelectedAreas, _bmp, previewBox, inverseMode.Checked);

            int totalSelectedPoints = BitArrayUtils.GetCount(allSelectedAreas);
            Text = FORM_TITLE + $" - {enabledCount} 個の選択エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
        }

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// 推定メモリ使用量を更新する
    /// </summary>
    private void UpdateEstimatedMemoryUsage()
    {
        if (_bmp == null) return;

        int bitmapCount = 1;

        if (inverseMode.Checked)
            bitmapCount += 1;

        if (transMode.Checked)
            bitmapCount += 1;

        double totalMB = BitmapUtils.CalculateEstimatedMemoryUsage(_bmp);
        estimatedMemoryUsageBase.Text = $"{totalMB:F2} MB";
        estimatedMemoryUsageCount.Text = bitmapCount.ToString();
        estimatedMemoryUsageTotal.Text = $"{totalMB * bitmapCount:F2} MB";
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;

        if (sender is PictureBox pictureBox)
        {
            Color color = pictureBox.Name == "previewBox" ? _previousColor : _newColor;
            Color inverseColor = ColorUtils.InverseColor(color);

            using Pen pen = new Pen(inverseColor, 2);
            e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
            e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
        }
    }

    private void PreviewBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (_previewBitmap == null || _newColor == Color.Empty || _previousColor == Color.Empty) return;

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (_bmp == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        if (_previousColor == Color.Empty)
        {
            FormUtils.ShowError("変更前の色が選択されていません。(プレビューが作成できません)");
            return;
        }

        _colorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);
        _colorPickerForm.Show();
    }

    private void SelectMode_CheckedChanged(object sender, EventArgs e)
    {
        if (selectMode.Checked && _bmp == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && (_previousColor == Color.Empty || _newColor == Color.Empty))
        {
            FormUtils.ShowError("色が選択されていません。（プレビューが作成できません）");
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && _backgroundColor == Color.Empty)
        {
            FormUtils.ShowInfo("選択モードが有効になりました。はじめに背景色を右クリックで設定してください。", "選択モード");
        }

        selectModePanel.Enabled = selectMode.Checked;
    }

    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (inverseMode.Checked)
            FormUtils.ShowInfo("選択反転モードがオンになりました。\n\n- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n- 透過画像作成モードでは、透過する部分が選択部分と逆になります。", "選択反転モード");

        UpdateSelectedArea();
        UpdateEstimatedMemoryUsage();
    }

    private void TransMode_CheckedChanged(object sender, EventArgs e)
        => UpdateEstimatedMemoryUsage();

    private void SelectedAreaListButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.Show();

    private void AdvancedColorSettingsButton_Click(object sender, EventArgs e)
        => _advancedColorSettingsForm.Show();

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

    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (_bmp == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            FormUtils.ShowError("色が選択されていません。");
            return;
        }

        bool result = FormUtils.ShowConfirm("画像を作成しますか？");
        if (!result) return;

        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNGファイル|*.png;",
            Title = "新規テクスチャ画像の保存先を選択してください",
            FileName = FileUtils.GetNewFileName(_imageFilePath),
            InitialDirectory = FileUtils.GetInitialDirectory(_imageFilePath)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        string newFilePath = dialog.FileName;

        if (newFilePath == string.Empty)
        {
            FormUtils.ShowError("ファイルの保存先が選択されていません。");
            return;
        }

        makeButton.Text = "作成中...";
        makeButton.Enabled = false;
        ApplyColorChange(newFilePath);
    }

    private void HelpUseButton_Click(object sender, EventArgs e)
        => _helpForm.Show();

    private void UndoButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.RemoveLast();

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "柔軟なテクスチャ色変換ツール\n指定した色の差分をもとに、テクスチャの色を簡単に変更できます。\n\n";
        message += "ツール情報:\n制作者: ぷこるふ\nTwitter: @pukorufu\nGithub: https://github.com/puk06/Color-Changer\n\n";
        message += "このソフトウェアは、個人の趣味で作成されたものです。\nもしこのソフトウェアが役に立ったと感じたら、ぜひ支援をお願いします！\n\n";
        message += "ライセンス:\nこのソフトウェアは、MITライセンスのもとで配布されています。";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + CURRENT_VERSION);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
        => _balanceModeSettingsForm.Show();

    private void DonationButton_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm(
            "支援していただける場合は、以下のリンクを開きます。\n\n" +
            "支援先: https://pukorufu.booth.pm/items/6519471\n\n" +
            "支援は任意です。無理のない範囲でお願いします。"
        );

        if (!result) return;

        try
        {
            Process.Start(_processStartInfo);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"リンクを開くことができませんでした。\n{ex.Message}");
        }
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = _colorPickerForm.Visible ? DragDropEffects.None : DragDropEffects.All;
    #endregion
}
