using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.Json;

namespace ColorChanger.Forms;

internal partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.18";
    private static readonly string FORM_TITLE = $"Color Changer For Texture {CURRENT_VERSION}";
    private static readonly Point VERSION_LABEL_POSITION = new Point(275, 54);
    private const int COLOR_UPDATE_DEBOUNCE_MS = 14;

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Color _backgroundColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;
    private readonly Stopwatch _updateDebounceStopwatch = Stopwatch.StartNew();

    private Bitmap? _bmp;
    private string? _imageFilePath;

    private BitArray _selectedPointsForPreview = BitArrayUtils.GetEmpty();
    private Bitmap? _previewBitmap;

    private bool _savedColorSettings = false;

    internal readonly ColorPickerForm ColorPickerForm = new ColorPickerForm();
    private readonly BalanceModeSettingsForm _balanceModeSettingsForm;
    private readonly SelectedAreaListForm _selectedAreaListForm = new SelectedAreaListForm();
    private readonly HelpForm _helpForm = new HelpForm();
    private readonly AdvancedColorSettingsForm _advancedColorSettingsForm = new AdvancedColorSettingsForm();
    private readonly SelectColorFromTextureForm _selectColorFromTextureForm = new SelectColorFromTextureForm();
    private readonly SelectionPenSettingsForm _selectionPenSettingsForm = new SelectionPenSettingsForm();
    private readonly PreviewZoomForm _previewZoomForm = new PreviewZoomForm();

    private readonly ColorDifference _colorDifference = new ColorDifference(Color.Empty, Color.Empty);
    private ColorDifference ColorDifference
    {
        get
        {
            _colorDifference.Set(_previousColor, _newColor);
            return _colorDifference;
        }
    }

    /// <summary>
    /// 変更後の色が選択されているかどうかを取得します。
    /// </summary>
    internal bool NewColorSelected
        => _newColor != Color.Empty;

    internal MainForm()
    {
        _balanceModeSettingsForm = new BalanceModeSettingsForm(this);

        InitializeComponent();
        RegisterPreviewMouseEvents();
        Icon = FormUtils.GetSoftwareIcon();
        Text = FORM_TITLE;

        versionLabel.Text = CURRENT_VERSION;
        FormUtils.AlignTextRight(versionLabel, VERSION_LABEL_POSITION);

        previousColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        newColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
        previewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        coloredPreviewBox.BackColor = ColorUtils.DefaultBackgroundColor;
        backgroundColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

        SetupEventHandlers();
        UpdateTextureData();
        UpdateColorData();
        UpdateColorConfigulation();
    }

    private void RegisterPreviewMouseEvents()
    {
        previewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        previewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
        coloredPreviewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        coloredPreviewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
    }

    #region 色選択関連
    /// <summary>
    /// 変更前の色を選択する。
    /// </summary>
    /// <param name="isMouseMoving"></param>
    /// <param name="e"></param>
    private void SelectPreviousColor(bool isMouseMoving, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;
        if (_bmp == null || _previewBitmap == null) return;

        if (!BitmapUtils.IsValidCoordinate(e.Location, _previewBitmap.Size)) return;

        if (_updateDebounceStopwatch.ElapsedMilliseconds <= COLOR_UPDATE_DEBOUNCE_MS) return;
        _updateDebounceStopwatch.Restart();

        Color selectedColor = _previewBitmap.GetPixel(e.X, e.Y);

        if (selectMode.Checked)
        {
            Point originalCoordinates = BitmapUtils.ConvertToOriginalCoordinates(e, previewBox, _bmp);
            HandleSelectionMode(e, selectedColor, originalCoordinates, isMouseMoving);
            return;
        }

        previousColorBox.BackColor = selectedColor;
        _previousColor = selectedColor;
        ColorPickerForm.SetInitialColor(selectedColor);
        if (_newColor == Color.Empty) ColorPickerForm.SetColor(selectedColor);

        UpdateColorData();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
        coloredPreviewBox.Invalidate();
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

            Bitmap? transBitmap = BitmapUtils.CreateTransparentBitmap(_bmp, transparentMode.Checked, inverseMode.Checked);
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

            if (!skipped && transparentMode.Checked && !inverseMode.Checked)
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

            FileSystemUtils.OpenFilePath(filePath);
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
            if (transparentMode.Checked)
            {
                FormUtils.ShowInfo("選択エリアがなかったため、透過モードはスキップされます。");
                skipped = true;
            }

            processor.ProcessAllPixels(sourcePixels, sourcePixels);
        }
        else if (transparentMode.Checked)
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
            PreviewConfiguration previewConfig = new PreviewConfiguration
            {
                SourceBitmap = sourceBitmap,
                ColorDifference = ColorDifference,
                BalanceMode = balanceMode.Checked,
                BalanceModeConfiguration = _balanceModeSettingsForm.Configuration,
                AdvancedColorConfiguration = _advancedColorSettingsForm.Configuration,
                SelectedPoints = _selectedPointsForPreview,
                PreviewBoxSize = coloredPreviewBox.Size,
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

        if (_bmp != null && _previousColor != Color.Empty && _newColor != Color.Empty && !_savedColorSettings)
        {
            var result = FormUtils.ShowConfirm("現在の色設定を保存せずに画像を変更しようとしています。\n画像変更前に保存しますか？", "画像変更");
            if (result) ExportColorSettings_Click(null, null);
        }

        try
        {
            BitmapUtils.DisposeBitmap(ref _bmp);
            BitmapUtils.DisposeBitmap(ref _previewBitmap);

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _bmp = new Bitmap(stream);
            _previewBitmap = GenerateColoredPreview(_bmp, rawMode: true);

            BitmapUtils.SetImage(previewBox, _previewBitmap, disposeImage: false);

            _previewZoomForm.SetGraphics(null);
            _previewZoomForm.SetImage(_previewBitmap, false);

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

            _previewZoomForm.SetGraphics(null);
            _previewZoomForm.ResetImage();

            _imageFilePath = null;
        }
        finally
        {
            BitmapUtils.ResetImage(coloredPreviewBox);

            _previousColor = Color.Empty;
            _newColor = Color.Empty;
            _backgroundColor = Color.Empty;

            ColorPickerForm.SetInitialColor(Color.Empty);
            ColorPickerForm.SetColor(Color.Empty);

            previousColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            newColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            backgroundColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

            _clickedPoint = Point.Empty;

            _selectedPointsForPreview = BitArrayUtils.GetEmpty();
            _selectedAreaListForm.Clear();

            _advancedColorSettingsForm.Reset();

            _balanceModeSettingsForm.LoadSettings(BalanceModeUtils.GetEmpty());
            balanceMode.Checked = false;

            transparentMode.Checked = false;

            _selectionPenSettingsForm.Reset();

            Text = FORM_TITLE;
            selectMode.Checked = false;

            UpdateTextureData();
            UpdateColorData();

            ColorPickerForm.Hide();
            _balanceModeSettingsForm.ResetGradientPreviewImage();

            _savedColorSettings = false;
        }
    }

    /// <summary>
    /// 色情報設定ファイルを読み込む
    /// </summary>
    /// <param name="path"></param>
    private void LoadColorSettingsFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("ファイルが存在しません。");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            ColorSettings? colorSettings = JsonSerializer.Deserialize<ColorSettings>(json) ?? throw new Exception("色情報設定ファイルが読み込めませんでした。");
            SetColorSettingsValues(colorSettings);

            FormUtils.ShowInfo("色情報設定ファイルの読み込みに成功しました！");
        }
        catch
        {
            FormUtils.ShowError("色情報設定ファイルの読み込みに失敗しました。");
        }
    }

    /// <summary>
    /// 色情報設定ファイルを出力する
    /// </summary>
    /// <param name="path"></param>
    private void ExportColorSettingsFile(string path)
    {
        try
        {
            ColorSettings colorSettings = new ColorSettings()
            {
                PreviousColor = new SerializableColor(_previousColor),
                NewColor = new SerializableColor(_newColor),
                BalanceModeEnabled = balanceMode.Checked,
                TransparentModeEnabled = transparentMode.Checked,
                BalanceModeConfiguration = _balanceModeSettingsForm.Configuration,
                AdvancedColorConfiguration = _advancedColorSettingsForm.Configuration
            };

            string json = JsonSerializer.Serialize(colorSettings);
            File.WriteAllText(path, json);

            FormUtils.ShowInfo("色情報設定ファイルの出力に成功しました！");
            FileSystemUtils.OpenFilePath(path);
            _savedColorSettings = true;
        }
        catch
        {
            FormUtils.ShowError("色情報設定ファイルの出力に失敗しました。");
        }
    }

    /// <summary>
    /// 与えられたColorSettingsの値を元にFormの値を設定します
    /// </summary>
    /// <param name="colorSettings"></param>
    private void SetColorSettingsValues(ColorSettings colorSettings)
    {
        _previousColor = colorSettings.PreviousColor.ToColor();
        previousColorBox.BackColor = colorSettings.PreviousColor.ToColor();

        _newColor = colorSettings.NewColor.ToColor();
        newColorBox.BackColor = colorSettings.NewColor.ToColor();
        ColorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);

        _balanceModeSettingsForm.LoadSettings(colorSettings.BalanceModeConfiguration);
        _advancedColorSettingsForm.LoadSettings(colorSettings.AdvancedColorConfiguration);

        balanceMode.Checked = colorSettings.BalanceModeEnabled;
        transparentMode.Checked = colorSettings.TransparentModeEnabled;

        UpdateColorData();

        if (_previewBitmap == null) return;
        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// 選択エリアを更新する
    /// </summary>
    private void UpdateSelectedArea()
    {
        if (_bmp == null || _previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitArray allSelectedAreas = _selectedAreaListForm.SelectedArea;

        int enabledCount = _selectedAreaListForm.EnabledCount;
        int enabledEraserCount = _selectedAreaListForm.EnabledEraserAreaCount;

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
            Text = FORM_TITLE + $" - {enabledCount} 個の選択エリア / {enabledEraserCount} 個の消去エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
        }

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// 色の追加設定を更新する
    /// </summary>
    private void UpdateColorConfigulation()
    {
        _advancedColorSettingsForm.Configuration.UpdateComponentActivationStatus();

        if (_advancedColorSettingsForm.Configuration.Enabled)
        {
            advancedColorConfigStatus.Text = "有効";
            advancedColorConfigStatus.ForeColor = ColorUtils.DefaultForeColor;
        }
        else
        {
            advancedColorConfigStatus.Text = "無効";
            advancedColorConfigStatus.ForeColor = ColorUtils.DefaultUnselectedColor;
        }

        if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// テクスチャデータを更新する
    /// </summary>
    private void UpdateTextureData()
    {
        Color foreColor = _bmp != null ? ColorUtils.DefaultForeColor : ColorUtils.DefaultUnselectedColor;

        if (_bmp == null)
        {
            textureResolution.Text = "N/A";
            memoryUsage.Text = "N/A";
            textureType.Text = "N/A";
        }
        else
        {
            textureResolution.Text = $"{_bmp.Width} x {_bmp.Height} px";
            memoryUsage.Text = BitmapUtils.CalculateEstimatedMemoryUsage(_bmp).ToString("F2") + " MB";
            textureType.Text = _bmp.PixelFormat.ToString().Replace("Format", "");
        }

        textureResolution.ForeColor = foreColor;
        memoryUsage.ForeColor = foreColor;
        textureType.ForeColor = foreColor;
    }

    /// <summary>
    /// 色情報を更新する
    /// </summary>
    private void UpdateColorData()
    {
        if (_previousColor == Color.Empty)
        {
            previousRGBLabel.Text = "未選択";
            previousRGBLabel.ForeColor = ColorUtils.DefaultUnselectedColor;
        }
        else
        {
            previousRGBLabel.Text = $"{_previousColor.R}, {_previousColor.G}, {_previousColor.B}";
            previousRGBLabel.ForeColor = ColorUtils.DefaultForeColor;
        }

        if (_newColor == Color.Empty)
        {
            newRGBLabel.Text = "未選択";
            newRGBLabel.ForeColor = ColorUtils.DefaultUnselectedColor;
        }
        else
        {
            newRGBLabel.Text = $"{_newColor.R}, {_newColor.G}, {_newColor.B}";
            newRGBLabel.ForeColor = ColorUtils.DefaultForeColor;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = "N/A";
            calculatedRGBLabel.ForeColor = ColorUtils.DefaultUnselectedColor;
        }
        else
        {
            calculatedRGBLabel.Text = ColorDifference.ToString();
            calculatedRGBLabel.ForeColor = ColorUtils.DefaultForeColor;
        }
    }
    #endregion

    #region 選択モード関連
    /// <summary>
    /// 選択モードの処理
    /// </summary>
    private void HandleSelectionMode(MouseEventArgs e, Color color, Point originalCoordinates, bool isMouseMoving)
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

        if (_selectionPenSettingsForm.PenEnaled)
        {
            if (!_selectionPenSettingsForm.Initialized) _selectionPenSettingsForm.Initialize(_bmp.Size);
            _selectionPenSettingsForm.SetSelectionArea(originalCoordinates);
            previewBox.Invalidate();
            return;
        }

        if (isMouseMoving) return;

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

    private void OnSelectionEnd(bool isEraserLayer)
    {
        if (!_selectionPenSettingsForm.Initialized) return;
        _selectedAreaListForm.Add(_selectionPenSettingsForm.GetTotalSelectedArea, isEraserLayer);
    }
    #endregion

    #region イベントハンドラー
    /// <summary>
    /// イベントハンドラのセットアップ
    /// </summary>
    private void SetupEventHandlers()
    {
        SetupColorChangeHandlers();
        SetupFormInteractionHandlers();
        SetupPreviewZoomHandlers();
    }

    private void SetupColorChangeHandlers()
    {
        _balanceModeSettingsForm.ConfigurationChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        ColorPickerForm.ColorChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty) return;

            _newColor = ColorPickerForm.SelectedColor;
            newColorBox.BackColor = _newColor;

            UpdateColorData();
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        _selectColorFromTextureForm.ColorChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty) return;

            _newColor = _selectColorFromTextureForm.SelectedColor;
            newColorBox.BackColor = _newColor;
            ColorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);

            UpdateColorData();
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };
    }

    private void SetupFormInteractionHandlers()
    {
        _selectedAreaListForm.OnCheckedChanged += (s, e) => UpdateSelectedArea();
        _advancedColorSettingsForm.ConfigurationChanged += (s, e) => UpdateColorConfigulation();

        _selectionPenSettingsForm.SelectionConfirmed += (s, e) =>
        {
            if (s is bool isEraserLayer)
                OnSelectionEnd(isEraserLayer);

            _selectionPenSettingsForm.Reset();
            previewBox.Invalidate();
        };

        _selectionPenSettingsForm.SelectionReverted += (s, e)
            => previewBox.Invalidate();
    }

    private void SetupPreviewZoomHandlers()
    {
        _previewZoomForm.PreviewMouseDown += (s, e)
            => HandlePreviewMouse(s as Point?, e, triggeredByMouseMove: false);

        _previewZoomForm.PreviewMouseMoved += (s, e)
            => HandlePreviewMouse(s as Point?, e, triggeredByMouseMove: true);

        _previewZoomForm.PreviewMouseUp += PreviewBox_MouseUp;

        _previewZoomForm.RequestImageUpdate += (s, e) =>
        {
            if (_previewBitmap == null || s is not int selectedIndex) return;

            switch (selectedIndex)
            {
                case 0:
                    _previewZoomForm.SetImage(_previewBitmap, false);
                    break;
                case 1:
                    _previewZoomForm.SetImage(GenerateColoredPreview(_previewBitmap), true);
                    break;
            }
        };
    }

    private void HandlePreviewMouse(Point? pointLocation, MouseEventArgs e, bool triggeredByMouseMove)
    {
        if (pointLocation is not Point point) return;

        var mouseEvent = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
        SelectPreviousColor(triggeredByMouseMove, mouseEvent);
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;
        if (sender is not PictureBox pictureBox) return;

        Color color = pictureBox.Name == "previewBox" ? _previousColor : _newColor;
        Color inverseColor = ColorUtils.InverseColor(color);

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
        e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);

        if (pictureBox.Name != "previewBox") return;
        if (selectMode.Checked && _selectionPenSettingsForm.PenEnaled)
        {
            bool[,] previewMap = _selectionPenSettingsForm.GeneratePreviewSelectionMap(previewBox.Size);

            SelectionUtils.SetSelectionPreviewMap(e.Graphics, previewMap);
            _previewZoomForm.SetGraphics(previewMap);
        }
        else
        {
            _previewZoomForm.SetGraphics(null);
        }
    }

    private void PreviewBox_MouseUp(object? sender, MouseEventArgs e)
    {
        if (selectMode.Checked && _selectionPenSettingsForm.PenEnaled) _selectionPenSettingsForm.EndSelection();
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

        ColorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);
        ColorPickerForm.Show();
        ColorPickerForm.BringToFront();
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
            FormUtils.ShowInfo("選択モードが有効になりました。\nペンツール以外でオブジェクト選択する場合は、はじめに背景色を右クリックで設定してください。", "選択モード");
        }

        selectModePanel.Enabled = selectMode.Checked;
    }

    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (balanceMode.Checked)
        {
            int currentBalanceMode = _balanceModeSettingsForm.Configuration.ModeVersion;
            var result = FormUtils.ShowConfirm(
                "バランスモードが有効化されました！\n\n" +
                "色を綺麗に改変するためには、以下の設定項目の調整が推奨されます。\n\n" +
                "推奨設定項目：\n" +
                "- V1: 重りの調整\n" +
                "- V2: 半径の長さの調整\n" +
                "- V3: 変更後の色、およびグラデーション位置の調整\n\n" +
                $"おすすめバージョン: V3 (現在のバージョン: V{currentBalanceMode})\n\n" +
                "設定画面を開きますか？",
                "バランスモード"
            );

            if (result)
            {
                _balanceModeSettingsForm.Show();
                _balanceModeSettingsForm.BringToFront();
            }
        }

        if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (inverseMode.Checked)
            FormUtils.ShowInfo("選択反転モードがオンになりました。\n\n- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n- 透過画像作成モードでは、透過する部分が選択部分と逆になります。", "選択反転モード");

        UpdateSelectedArea();
    }

    private void SelectedAreaListButton_Click(object sender, EventArgs e)
    {
        _selectedAreaListForm.Show();
        _selectedAreaListForm.BringToFront();
    }

    private void AdvancedColorSettingsButton_Click(object sender, EventArgs e)
    {
        _advancedColorSettingsForm.Show();
        _advancedColorSettingsForm.BringToFront();
    }

    private void OpenFile_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog()
        {
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.ico|すべてのファイル|*.*",
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
    {
        _helpForm.Show();
        _helpForm.BringToFront();
    }

    private void SelectColorFromTexture_Click(object sender, EventArgs e)
    {
        _selectColorFromTextureForm.Show();
        _selectColorFromTextureForm.BringToFront();
    }

    private void SelectionPenTool_Click(object sender, EventArgs e)
    {
        _selectionPenSettingsForm.Show();
        _selectionPenSettingsForm.BringToFront();
    }

    private void PreviewZoomTool_Click(object sender, EventArgs e)
    {
        _previewZoomForm.Show();
        _previewZoomForm.BringToFront();
    }

    private void UndoButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.RemoveLast();

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "柔軟なテクスチャ色変換ツール\n指定した色の差分をもとに、テクスチャの色を簡単に変更できます。\n\n";
        message += $"ツール情報:\n制作者: ぷこるふ\nTwitter: @pukorufu\nGithub: {ItemUtils.GithubURL}\n\n";
        message += "このソフトウェアは、個人の趣味で作成されたものです。\nもしこのソフトウェアが役に立ったと感じたら、ぜひ支援をお願いします！\n\n";
        message += "ライセンス:\nこのソフトウェアは、MITライセンスのもとで配布されています。";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + CURRENT_VERSION);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
    {
        _balanceModeSettingsForm.Show();
        _balanceModeSettingsForm.BringToFront();
    }
    private void UpdateCheck_Click(object sender, EventArgs e)
        => UpdateUtils.CheckUpdate(CURRENT_VERSION);

    private void DonationButton_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm(
            "支援していただける場合は、以下のリンクを開きます。\n\n" +
            $"支援先: {ItemUtils.ItemURL}\n\n" +
            "支援は任意です。無理のない範囲でお願いします。"
        );

        if (!result) return;

        ItemUtils.OpenItemURL();
    }

    private void ImportColorSettings_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog()
        {
            Filter = "色情報設定ファイル|*.ccs",
            Title = "色情報設定ファイルを選択してください"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadColorSettingsFile(dialog.FileName);
    }

    private void ExportColorSettings_Click(object? sender, EventArgs? e)
    {
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "色情報設定ファイル|*.ccs",
            Title = "色情報設定ファイルの保存先を選択してください",
            FileName = $"MyColorTheme_{DateTime.Now:MM-dd_HH-mm-ss}.ccs",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        ExportColorSettingsFile(dialog.FileName);
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        string file = files[0];

        if (Path.GetExtension(file) == ".ccs")
        {
            LoadColorSettingsFile(file);
        }
        else
        {
            LoadPictureFile(file);
        }
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.All;

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.O)
        {
            ImportColorSettings_Click(sender, e);
        }

        if (e.Control && e.KeyCode == Keys.S)
        {
            ExportColorSettings_Click(sender, e);
        }
    }
    #endregion
}
