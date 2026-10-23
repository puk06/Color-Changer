using ColorChanger.Models;
using ColorChanger.Services;
using System.Collections;

namespace ColorChanger.Forms;

internal partial class MainForm : Form
{
    private static readonly Point VERSION_LABEL_POSITION = new Point(275, 54);

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Color _backgroundColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;
    private readonly Throttle _colorPickThrottle = new Throttle(14);

    private Bitmap? _sourceBitmap;
    private Bitmap? _rawPreview;
    private string? _imageFilePath;

    private BitArray _selectedPointsForPreview = BitArrayHelper.Empty();
    private bool _savedColorSettings = false;

    private readonly ColorPickerForm _colorPickerForm = new ColorPickerForm();
    private readonly BalanceModeSettingsForm _balanceModeSettingsForm = new BalanceModeSettingsForm();
    private readonly SelectedAreaListForm _selectedAreaListForm = new SelectedAreaListForm();
    private readonly HelpForm _helpForm = new HelpForm();
    private readonly AdvancedColorSettingsForm _advancedColorSettingsForm = new AdvancedColorSettingsForm();
    private readonly SelectColorFromTextureForm _selectColorFromTextureForm = new SelectColorFromTextureForm();
    private readonly SelectionPenSettingsForm _selectionPenSettingsForm = new SelectionPenSettingsForm();
    private readonly PreviewZoomForm _previewZoomForm = new PreviewZoomForm();
    private readonly SelectAreaFromImageMaskForm _selectAreaFromImageMaskForm = new SelectAreaFromImageMaskForm();

    internal MainForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();
        Text = AppInfo.Title;

        versionLabel.Text = AppInfo.Version;
        FormUtils.AlignTextRight(versionLabel, VERSION_LABEL_POSITION);

        previousColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        newColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        previewBox.BackColor = FormUtils.DefaultBackgroundColor;
        coloredPreviewBox.BackColor = FormUtils.DefaultBackgroundColor;
        backgroundColorBox.BackColor = FormUtils.DefaultBackgroundColor;

        RegisterPreviewMouseEvents();
        SetupEventHandlers();

        UpdateTextureData();
        UpdateColorData();
        UpdateColorConfiguration();

        _ = CheckUpdateAsync(silent: true);
    }

    private void RegisterPreviewMouseEvents()
    {
        previewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        previewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
        coloredPreviewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        coloredPreviewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
    }

    private void SetupEventHandlers()
    {
        _balanceModeSettingsForm.ConfigurationChanged += (s, e) => RenderColoredPreview();
        _balanceModeSettingsForm.NewColorPickerRequested += ShowColorPicker;

        _colorPickerForm.ColorChanged += (s, e) => OnColorPickerChanged();
        _selectColorFromTextureForm.ColorChanged += (s, e) => OnTextureColorSelected();

        _selectedAreaListForm.LayersChanged += (s, e) => UpdateSelectedArea();
        _advancedColorSettingsForm.ConfigurationChanged += (s, e) => UpdateColorConfiguration();

        _selectionPenSettingsForm.LayerSubmitted += (selection, isEraser) => _selectedAreaListForm.Add(selection, isEraser);
        _selectionPenSettingsForm.SelectionChanged += () => previewBox.Invalidate();

        _previewZoomForm.PreviewMouseDown += (point, e) => SelectPreviousColorAtZoom(point, e, isMouseMoving: false);
        _previewZoomForm.PreviewMouseMoved += (point, e) => SelectPreviousColorAtZoom(point, e, isMouseMoving: true);
        _previewZoomForm.PreviewMouseUp += (point, e) => PreviewBox_MouseUp(null, e);
        _previewZoomForm.RequestImageUpdate += ShowZoomImage;

        _selectAreaFromImageMaskForm.AreaChanged += (s, e) => OnMaskAreaChanged();
    }

    private async Task CheckUpdateAsync(bool silent)
    {
        try
        {
            VersionData? versionData = await UpdateService.FetchLatestVersionAsync();
            if (versionData == null)
            {
                if (!silent) FormUtils.ShowError("アップデート情報の取得中にエラーが発生しました");
                return;
            }

            bool hasUpdate = versionData.LatestVersion != AppInfo.Version;
            string changeLog = string.Join("\n", versionData.ChangeLog.Select(log => $"・{log}"));

            if (silent)
            {
                if (hasUpdate) updateCheck.BackColor = Color.LightGreen;
                return;
            }

            if (hasUpdate)
            {
                bool result = FormUtils.ShowConfirm(
                    "新しいColor Changerのバージョンが利用可能です！\n\n" +
                    "「はい」をクリックすると商品ページを開きます。\n\n" +
                    $"現在のバージョン: {AppInfo.Version}\n" +
                    $"最新のバージョン: {versionData.LatestVersion}\n\n" +
                    $"以下は最新版（{versionData.LatestVersion}）の変更内容です。\n\n" +
                    changeLog
                );

                if (result) OpenUrl(AppInfo.ItemUrl);
            }
            else
            {
                FormUtils.ShowInfo(
                    "ご利用中のColor Changerは最新版です！\n\n" +
                    "いつもご利用いただき、ありがとうございます。\n" +
                    "今後のアップデートもぜひお楽しみに！\n\n" +
                    $"以下は最新版（{versionData.LatestVersion}）の変更内容です。\n\n" +
                    changeLog
                );
            }
        }
        catch
        {
            if (!silent)
            {
                FormUtils.ShowError(
                    "アップデート情報の取得中にエラーが発生しました。\n" +
                    "通信環境やサーバーの状況が原因の可能性があります。"
                );
            }
        }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            ShellService.OpenUrl(url);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"リンクを開くことができませんでした。\n{ex.Message}");
        }
    }

    private static void ShowWindow(Form form)
    {
        form.Show();
        form.BringToFront();
    }

    #region 色選択関連
    /// <summary>
    /// 変更前の色を選択する。
    /// </summary>
    private void SelectPreviousColor(bool isMouseMoving, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;
        if (_sourceBitmap == null || _rawPreview == null) return;

        if (!new Rectangle(Point.Empty, _rawPreview.Size).Contains(e.Location)) return;
        if (!_colorPickThrottle.TryPass()) return;

        Color selectedColor = _rawPreview.GetPixel(e.X, e.Y);

        if (selectMode.Checked)
        {
            Point sourcePoint = SelectionService.ToSourceCoordinates(e.Location, previewBox.Size, _sourceBitmap.Size);
            HandleSelectionMode(e, selectedColor, sourcePoint, isMouseMoving);
            return;
        }

        previousColorBox.BackColor = selectedColor;
        _previousColor = selectedColor;
        _colorPickerForm.SetInitialColor(selectedColor);
        if (_newColor == Color.Empty) _colorPickerForm.SetColor(selectedColor);

        UpdateColorData();
        RenderColoredPreview();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
        coloredPreviewBox.Invalidate();
    }

    private void SelectPreviousColorAtZoom(Point point, MouseEventArgs e, bool isMouseMoving)
    {
        var mouseEvent = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
        SelectPreviousColor(isMouseMoving, mouseEvent);
    }

    private void OnColorPickerChanged()
    {
        if (_rawPreview == null || _previousColor == Color.Empty) return;

        SetNewColor(_colorPickerForm.SelectedColor);
        RenderColoredPreview();
    }

    private void OnTextureColorSelected()
    {
        if (_rawPreview == null || _previousColor == Color.Empty) return;

        SetNewColor(_selectColorFromTextureForm.SelectedColor);
        _colorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);

        RenderColoredPreview();
    }

    /// <summary>
    /// 変更後の色を設定し、関連する表示を更新する
    /// </summary>
    private void SetNewColor(Color color)
    {
        _newColor = color;
        newColorBox.BackColor = color == Color.Empty ? FormUtils.DefaultBackgroundColor : color;
        _balanceModeSettingsForm.SetSourceColor(color);

        UpdateColorData();
    }

    private void ShowColorPicker()
    {
        _colorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);
        ShowWindow(_colorPickerForm);
    }
    #endregion

    #region 処理関連
    private ConversionSettings BuildConversionSettings()
        => new ConversionSettings
        {
            ColorDifference = new ColorDifference(_previousColor, _newColor),
            AdvancedColor = _advancedColorSettingsForm.Configuration,
            Balance = balanceMode.Checked ? _balanceModeSettingsForm.Configuration : null,
            TransparentMode = transparentMode.Checked,
            InverseMode = inverseMode.Checked,
            MaskMode = maskMode.Checked
        };

    /// <summary>
    /// 色変換後のテクスチャ画像を作成して保存する
    /// </summary>
    private void CreateTexture(string filePath)
    {
        try
        {
            if (_sourceBitmap == null) return;

            BitArray selection = _selectedAreaListForm.Layers.Combine();
            TextureConverter.ConversionResult result = TextureConverter.Convert(_sourceBitmap, BuildConversionSettings(), selection);

            using (result.Image)
            {
                foreach (string notice in result.Notices)
                {
                    FormUtils.ShowInfo(notice);
                }

                result.Image.Save(filePath);
            }

            FormUtils.ShowInfo("テクスチャ画像の作成が完了しました。");
            FormUtils.RevealFileWithConfirm(filePath);
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
    /// プレビュー用の色変換済み画像を生成する
    /// </summary>
    private Bitmap? GenerateColoredPreview(Bitmap rawPreview)
    {
        try
        {
            return PreviewService.Generate(rawPreview, coloredPreviewBox.Size, BuildConversionSettings(), _selectedPointsForPreview);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"プレビュー画像の生成に失敗しました。\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 色変換済みプレビューを再生成して表示する。色が未選択の場合は何もしない。
    /// </summary>
    private void RenderColoredPreview()
    {
        if (_rawPreview == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        Bitmap? preview = GenerateColoredPreview(_rawPreview);
        if (preview != null) FormUtils.SetOwnedImage(coloredPreviewBox, preview);
    }

    /// <summary>
    /// 画像ファイルを読み込み、関連する状態を初期化する
    /// </summary>
    private void LoadPictureFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("ファイルが存在しません。");
            return;
        }

        if (_sourceBitmap != null && _previousColor != Color.Empty && _newColor != Color.Empty && !_savedColorSettings)
        {
            bool result = FormUtils.ShowConfirm("現在の色設定を保存せずに画像を変更しようとしています。\n画像変更前に保存しますか？", "画像変更");
            if (result) SaveColorSettingsDialog();
        }

        ClearImages();

        try
        {
            Bitmap source = ImageFileService.Load(path);
            Bitmap rawPreview = PreviewService.CreateRawPreview(source, previewBox.Size);

            _sourceBitmap = source;
            _rawPreview = rawPreview;
            previewBox.Image = _rawPreview;

            _previewZoomForm.SetGraphics(null);
            _previewZoomForm.SetImage(_rawPreview, ownsImage: false);

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

            ClearImages();
            _imageFilePath = null;
        }

        ResetEditingState();
    }

    private void ClearImages()
    {
        previewBox.Image = null;
        _previewZoomForm.SetGraphics(null);
        _previewZoomForm.ResetImage();

        _rawPreview?.Dispose();
        _rawPreview = null;

        _sourceBitmap?.Dispose();
        _sourceBitmap = null;
    }

    /// <summary>
    /// 画像読み込み時に、色や選択、各種設定を初期状態に戻す
    /// </summary>
    private void ResetEditingState()
    {
        _previousColor = Color.Empty;
        _backgroundColor = Color.Empty;
        _clickedPoint = Point.Empty;
        _selectedPointsForPreview = BitArrayHelper.Empty();

        previousColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        backgroundColorBox.BackColor = FormUtils.DefaultBackgroundColor;
        SetNewColor(Color.Empty);

        _colorPickerForm.SetInitialColor(Color.Empty);
        _colorPickerForm.SetColor(Color.Empty);

        _selectedAreaListForm.Clear();
        _advancedColorSettingsForm.Reset();
        _balanceModeSettingsForm.LoadSettings(new BalanceModeConfiguration());
        balanceMode.Checked = false;
        transparentMode.Checked = false;
        _selectionPenSettingsForm.Reset();

        Text = AppInfo.Title;
        selectMode.Checked = false;

        FormUtils.SetOwnedImage(coloredPreviewBox, null);

        UpdateTextureData();
        UpdateColorData();

        _colorPickerForm.Hide();
        _balanceModeSettingsForm.ResetGradientPreviewImage();

        _savedColorSettings = false;
    }

    /// <summary>
    /// 色情報設定ファイルを読み込む
    /// </summary>
    private void LoadColorSettingsFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("ファイルが存在しません。");
            return;
        }

        try
        {
            ColorSettings? colorSettings = ColorSettingsFileService.Load(path);
            if (colorSettings == null)
            {
                FormUtils.ShowError("色情報設定ファイルの読み込みに失敗しました。");
            }
            else
            {
                ApplyColorSettings(colorSettings);
                FormUtils.ShowInfo("色情報設定ファイルの読み込みに成功しました！");
            }
        }
        catch
        {
            FormUtils.ShowError("色情報設定ファイルの読み込みに失敗しました。");
        }
    }

    /// <summary>
    /// 色情報設定ファイルを出力する
    /// </summary>
    private void SaveColorSettingsFile(string path)
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

            ColorSettingsFileService.Save(path, colorSettings);

            FormUtils.ShowInfo("色情報設定ファイルの出力に成功しました！");
            FormUtils.RevealFileWithConfirm(path);
            _savedColorSettings = true;
        }
        catch
        {
            FormUtils.ShowError("色情報設定ファイルの出力に失敗しました。");
        }
    }

    /// <summary>
    /// 与えられたColorSettingsの値を元に、各種設定を反映する
    /// </summary>
    private void ApplyColorSettings(ColorSettings colorSettings)
    {
        _previousColor = colorSettings.PreviousColor.ToColor();
        previousColorBox.BackColor = _previousColor;

        _balanceModeSettingsForm.LoadSettings(colorSettings.BalanceModeConfiguration);
        _advancedColorSettingsForm.LoadSettings(colorSettings.AdvancedColorConfiguration);

        SetNewColor(colorSettings.NewColor.ToColor());
        _colorPickerForm.SetInitialColor(_previousColor);
        _colorPickerForm.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);

        balanceMode.Checked = colorSettings.BalanceModeEnabled;
        transparentMode.Checked = colorSettings.TransparentModeEnabled;

        UpdateColorData();
        RenderColoredPreview();
    }

    /// <summary>
    /// 選択エリアを更新し、プレビューに反映する
    /// </summary>
    private void UpdateSelectedArea()
    {
        if (_sourceBitmap == null || _rawPreview == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitArray allSelectedAreas = _selectedAreaListForm.Layers.Combine();

        if (allSelectedAreas.Length == 0)
        {
            _selectedPointsForPreview = BitArrayHelper.Empty();

            Text = AppInfo.Title;
        }
        else
        {
            Text = AppInfo.Title + " - プレビュー用の選択エリア作成中...";
            _selectedPointsForPreview = SelectionService.ToPreviewSelection(allSelectedAreas, _sourceBitmap.Size, previewBox.Size, inverseMode.Checked);

            int totalSelectedPoints = BitArrayHelper.CountOf(allSelectedAreas);
            int enabledCount = _selectedAreaListForm.Layers.EnabledSelectionCount;
            int enabledEraserCount = _selectedAreaListForm.Layers.EnabledEraserCount;

            Text = AppInfo.Title + $" - {enabledCount} 個の選択エリア / {enabledEraserCount} 個の消去エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
        }

        RenderColoredPreview();
    }

    /// <summary>
    /// 色の追加設定の状態表示を更新する
    /// </summary>
    private void UpdateColorConfiguration()
    {
        _advancedColorSettingsForm.Configuration.UpdateComponentActivationStatus();

        if (_advancedColorSettingsForm.Configuration.Enabled)
        {
            advancedColorConfigStatus.Text = "有効";
            advancedColorConfigStatus.ForeColor = FormUtils.DefaultForegroundColor;
        }
        else
        {
            advancedColorConfigStatus.Text = "無効";
            advancedColorConfigStatus.ForeColor = FormUtils.DefaultUnselectedColor;
        }

        RenderColoredPreview();
    }

    /// <summary>
    /// テクスチャデータの表示を更新する
    /// </summary>
    private void UpdateTextureData()
    {
        Color foreColor = _sourceBitmap != null ? FormUtils.DefaultForegroundColor : FormUtils.DefaultUnselectedColor;

        if (_sourceBitmap == null)
        {
            textureResolution.Text = "N/A";
            memoryUsage.Text = "N/A";
            textureType.Text = "N/A";
        }
        else
        {
            textureResolution.Text = $"{_sourceBitmap.Width} x {_sourceBitmap.Height} px";
            memoryUsage.Text = ImageFileService.EstimateMemoryUsageMegabytes(_sourceBitmap).ToString("F2") + " MB";
            textureType.Text = _sourceBitmap.PixelFormat.ToString().Replace("Format", "");
        }

        textureResolution.ForeColor = foreColor;
        memoryUsage.ForeColor = foreColor;
        textureType.ForeColor = foreColor;
    }

    /// <summary>
    /// 色情報の表示を更新する
    /// </summary>
    private void UpdateColorData()
    {
        if (_previousColor == Color.Empty)
        {
            previousRGBLabel.Text = "未選択";
            previousRGBLabel.ForeColor = FormUtils.DefaultUnselectedColor;
        }
        else
        {
            previousRGBLabel.Text = $"{_previousColor.R}, {_previousColor.G}, {_previousColor.B}";
            previousRGBLabel.ForeColor = FormUtils.DefaultForegroundColor;
        }

        if (_newColor == Color.Empty)
        {
            newRGBLabel.Text = "未選択";
            newRGBLabel.ForeColor = FormUtils.DefaultUnselectedColor;
        }
        else
        {
            newRGBLabel.Text = $"{_newColor.R}, {_newColor.G}, {_newColor.B}";
            newRGBLabel.ForeColor = FormUtils.DefaultForegroundColor;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = "N/A";
            calculatedRGBLabel.ForeColor = FormUtils.DefaultUnselectedColor;
        }
        else
        {
            calculatedRGBLabel.Text = new ColorDifference(_previousColor, _newColor).ToString();
            calculatedRGBLabel.ForeColor = FormUtils.DefaultForegroundColor;
        }
    }
    #endregion

    #region 選択モード関連
    /// <summary>
    /// 選択モードでの操作を処理する
    /// </summary>
    private void HandleSelectionMode(MouseEventArgs e, Color color, Point sourcePoint, bool isMouseMoving)
    {
        if (_sourceBitmap == null) return;

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

        if (_selectionPenSettingsForm.PenEnabled)
        {
            if (!_selectionPenSettingsForm.IsInitialized) _selectionPenSettingsForm.Initialize(_sourceBitmap.Size);
            _selectionPenSettingsForm.PaintAt(sourcePoint);
            previewBox.Invalidate();
            return;
        }

        if (isMouseMoving) return;

        if (_backgroundColor == Color.Empty)
        {
            FormUtils.ShowError("背景色が選択されていません。");
            return;
        }

        string previousTitle = Text;
        Text = AppInfo.Title + " - 選択処理中...";

        BitArray? values = SelectionService.SelectByFloodFill(_sourceBitmap, sourcePoint, _backgroundColor);

        Text = previousTitle;

        if (values == null || values.Length == 0)
        {
            FormUtils.ShowError("選択エリアがありません。");
            return;
        }

        _selectedAreaListForm.Add(values);
    }

    private void OnMaskAreaChanged()
    {
        BitArray? area = _selectAreaFromImageMaskForm.SelectedArea;

        if (_sourceBitmap == null || _rawPreview == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        if (area == null || area.Length == 0)
        {
            FormUtils.ShowError("選択エリアがありません。");
            return;
        }

        if ((_sourceBitmap.Width * _sourceBitmap.Height) != area.Length)
        {
            FormUtils.ShowError("選択エリアが元画像のサイズと異なります。");
            return;
        }

        _selectedAreaListForm.Add(area);
        RenderColoredPreview();
    }

    private void ShowZoomImage(int mode)
    {
        if (_rawPreview == null) return;

        switch (mode)
        {
            case 0:
                _previewZoomForm.SetImage(_rawPreview, ownsImage: false);
                break;
            case 1:
                Bitmap? colored = GenerateColoredPreview(_rawPreview);
                if (colored != null) _previewZoomForm.SetImage(colored, ownsImage: true);
                break;
        }
    }
    #endregion

    #region イベントハンドラー
    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;
        if (sender is not PictureBox pictureBox) return;

        bool isPreviewBox = pictureBox == previewBox;
        Color color = isPreviewBox ? _previousColor : _newColor;

        FormUtils.DrawCrosshair(e.Graphics, _clickedPoint, color);

        if (!isPreviewBox) return;

        if (selectMode.Checked && _selectionPenSettingsForm.PenEnabled)
        {
            bool[,] previewMap = _selectionPenSettingsForm.BuildPreviewMap(previewBox.Size);

            FormUtils.DrawSelectionMap(e.Graphics, previewMap);
            _previewZoomForm.SetGraphics(previewMap);
        }
        else
        {
            _previewZoomForm.SetGraphics(null);
        }
    }

    private void PreviewBox_MouseUp(object? sender, MouseEventArgs e)
    {
        if (selectMode.Checked && _selectionPenSettingsForm.PenEnabled) _selectionPenSettingsForm.EndStroke();
        if (e.Button != MouseButtons.Left) return;

        RenderColoredPreview();
    }

    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (_sourceBitmap == null)
        {
            FormUtils.ShowError("画像が読み込まれていません。");
            return;
        }

        if (_previousColor == Color.Empty)
        {
            FormUtils.ShowError("変更前の色が選択されていません。(プレビューが作成できません)");
            return;
        }

        ShowColorPicker();
    }

    private void SelectMode_CheckedChanged(object sender, EventArgs e)
    {
        if (selectMode.Checked && _sourceBitmap == null)
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
            bool result = FormUtils.ShowConfirm(
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

            if (result) ShowWindow(_balanceModeSettingsForm);
        }

        RenderColoredPreview();
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (inverseMode.Checked)
            FormUtils.ShowInfo("選択反転モードがオンになりました。\n\n- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n- 透過画像作成モードでは、透過する部分が選択部分と逆になります。", "選択反転モード");

        UpdateSelectedArea();
    }

    private void SelectedAreaListButton_Click(object sender, EventArgs e)
        => ShowWindow(_selectedAreaListForm);

    private void AdvancedColorSettingsButton_Click(object sender, EventArgs e)
        => ShowWindow(_advancedColorSettingsForm);

    private void OpenFile_Click(object sender, EventArgs e)
    {
        string? path = FormUtils.PickFile("画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.ico|すべてのファイル|*.*", "画像ファイルを選択してください");
        if (path != null) LoadPictureFile(path);
    }

    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (_sourceBitmap == null)
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

        using SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNGファイル|*.png;",
            Title = "新規テクスチャ画像の保存先を選択してください",
            FileName = ImageFileService.GetNewFileName(_imageFilePath),
            InitialDirectory = ImageFileService.GetInitialDirectory(_imageFilePath)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        makeButton.Text = "作成中...";
        makeButton.Enabled = false;
        CreateTexture(dialog.FileName);
    }

    private void HelpUseButton_Click(object sender, EventArgs e)
        => ShowWindow(_helpForm);

    private void SelectColorFromTexture_Click(object sender, EventArgs e)
        => ShowWindow(_selectColorFromTextureForm);

    private void SelectionPenTool_Click(object sender, EventArgs e)
        => ShowWindow(_selectionPenSettingsForm);

    private void PreviewZoomTool_Click(object sender, EventArgs e)
        => ShowWindow(_previewZoomForm);

    private void SelectAreaFromImageMask_Click(object sender, EventArgs e)
        => ShowWindow(_selectAreaFromImageMaskForm);

    private void UndoButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.RemoveLast();

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + AppInfo.Version + "\n\n";
        message += "柔軟なテクスチャ色変換ツール\n指定した色の差分をもとに、テクスチャの色を簡単に変更できます。\n\n";
        message += $"ツール情報:\n制作者: ぷこるふ\nTwitter: @pukorufu\nGithub: {AppInfo.GithubUrl}\n\n";
        message += "このソフトウェアは、個人の趣味で作成されたものです。\nもしこのソフトウェアが役に立ったと感じたら、ぜひ支援をお願いします！\n\n";
        message += "ライセンス:\nこのソフトウェアは、MITライセンスのもとで配布されています。";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + AppInfo.Version);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
        => ShowWindow(_balanceModeSettingsForm);

    private async void UpdateCheck_Click(object sender, EventArgs e)
        => await CheckUpdateAsync(silent: false);

    private void DonationButton_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm(
            "支援していただける場合は、以下のリンクを開きます。\n\n" +
            $"支援先: {AppInfo.ItemUrl}\n\n" +
            "支援は任意です。無理のない範囲でお願いします。"
        );

        if (!result) return;

        OpenUrl(AppInfo.ItemUrl);
    }

    private void ImportColorSettings_Click(object sender, EventArgs e)
    {
        string? path = FormUtils.PickFile("色情報設定ファイル|*.ccs", "色情報設定ファイルを選択してください");
        if (path != null) LoadColorSettingsFile(path);
    }

    private void ExportColorSettings_Click(object sender, EventArgs e)
        => SaveColorSettingsDialog();

    private void SaveColorSettingsDialog()
    {
        using SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "色情報設定ファイル|*.ccs",
            Title = "色情報設定ファイルの保存先を選択してください",
            FileName = $"MyColorTheme_{DateTime.Now:MM-dd_HH-mm-ss}.ccs",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        SaveColorSettingsFile(dialog.FileName);
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        string? file = FormUtils.GetDroppedFile(e);
        if (file == null) return;

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
