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
    private const string CURRENT_VERSION = "v1.0.20";
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
    private readonly SelectAreaFromImageMaskForm _selectAreaFromImageMaskForm = new SelectAreaFromImageMaskForm();

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
    /// �ύX��̐F���I������Ă��邩�ǂ������擾���܂��B
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

        _ = CheckUpdate();
    }

    private async Task CheckUpdate()
    {
        bool result = await UpdateUtils.CheckUpdate(CURRENT_VERSION, silent: true);
        if (result) updateCheck.BackColor = Color.LightGreen;
    }

    private void RegisterPreviewMouseEvents()
    {
        previewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        previewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
        coloredPreviewBox.MouseDown += (s, e) => SelectPreviousColor(false, e);
        coloredPreviewBox.MouseMove += (s, e) => SelectPreviousColor(true, e);
    }

    #region �F�I���֘A
    /// <summary>
    /// �ύX�O�̐F��I������B
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

    #region �����֘A
    /// <summary>
    /// �F��ύX����
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
                    FormUtils.ShowError("���ߗp�摜���쐬�ł��܂���ł����B");
                }
            }
            else
            {
                bitMap.Save(filePath);
            }

            bitMap.Dispose();
            FormUtils.ShowInfo("�e�N�X�`���摜�̍쐬���������܂����B");

            FileSystemUtils.OpenFilePath(filePath);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"�e�N�X�`���摜�쐬���ɃG���[���������܂����B\n{ex}");
        }
        finally
        {
            makeButton.Enabled = true;
            makeButton.Text = "�쐬";
        }
    }

    /// <summary>
    /// �摜����������
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
                FormUtils.ShowInfo("�I���G���A���Ȃ��������߁A���߃��[�h�̓X�L�b�v����܂��B");
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
                    FormUtils.ShowError("���߉摜�p�f�[�^�̎擾�Ɏ��s���܂����B�f�t�H���g�̉摜���g�p����܂��B");

                processor.ProcessTransparentSelectedPixels(sourcePixels, transPixels, selectedPoints);
            }
        }
        else if (inverseMode.Checked)
        {
            processor.ProcessAllPixels(sourcePixels, sourcePixels);

            if (rawPixels.IsEmpty)
            {
                FormUtils.ShowError("���摜�̃f�[�^�̎擾�Ɏ��s���܂����B�I�𔽓]���[�h�̌��ʂ͍쐬����܂���B");
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
    /// �v���r���[�p�̃r�b�g�}�b�v�𐶐�����
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
            FormUtils.ShowError($"�v���r���[�摜�̐����Ɏ��s���܂����B\n{ex.Message}");
            return sourceBitmap;
        }
    }

    /// <summary>
    /// �摜�t�@�C����ǂݍ���
    /// </summary>
    /// <param name="path"></param>
    private void LoadPictureFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("�t�@�C�������݂��܂���B");
            return;
        }

        if (_bmp != null && _previousColor != Color.Empty && _newColor != Color.Empty && !_savedColorSettings)
        {
            var result = FormUtils.ShowConfirm("���݂̐F�ݒ��ۑ������ɉ摜��ύX���悤�Ƃ��Ă��܂��B\n�摜�ύX�O�ɕۑ����܂����H", "�摜�ύX");
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
                FormUtils.ShowError("�摜�̓ǂݍ��݂Ɏ��s���܂����B��Ή��̃t�@�C���ł��B");
            }
            else
            {
                FormUtils.ShowError("�摜�̓ǂݍ��݂Ɏ��s���܂����B\n\n�G���[: " + exception);
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
    /// �F���ݒ�t�@�C����ǂݍ���
    /// </summary>
    /// <param name="path"></param>
    private void LoadColorSettingsFile(string path)
    {
        if (!File.Exists(path))
        {
            FormUtils.ShowError("�t�@�C�������݂��܂���B");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            ColorSettings? colorSettings = JsonSerializer.Deserialize<ColorSettings>(json);
            if (colorSettings == null)
            {
                FormUtils.ShowError("�F���ݒ�t�@�C���̓ǂݍ��݂Ɏ��s���܂����B");
            }
            else
            {
                SetColorSettingsValues(colorSettings);
                FormUtils.ShowInfo("�F���ݒ�t�@�C���̓ǂݍ��݂ɐ������܂����I");
            }
        }
        catch
        {
            FormUtils.ShowError("�F���ݒ�t�@�C���̓ǂݍ��݂Ɏ��s���܂����B");
        }
    }

    /// <summary>
    /// �F���ݒ�t�@�C�����o�͂���
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

            FormUtils.ShowInfo("�F���ݒ�t�@�C���̏o�͂ɐ������܂����I");
            FileSystemUtils.OpenFilePath(path);
            _savedColorSettings = true;
        }
        catch
        {
            FormUtils.ShowError("�F���ݒ�t�@�C���̏o�͂Ɏ��s���܂����B");
        }
    }

    /// <summary>
    /// �^����ꂽColorSettings�̒l������Form�̒l��ݒ肵�܂�
    /// </summary>
    /// <param name="colorSettings"></param>
    private void SetColorSettingsValues(ColorSettings colorSettings)
    {
        _previousColor = colorSettings.PreviousColor.ToColor();
        previousColorBox.BackColor = colorSettings.PreviousColor.ToColor();

        _newColor = colorSettings.NewColor.ToColor();
        newColorBox.BackColor = colorSettings.NewColor.ToColor();
        ColorPickerForm.SetInitialColor(_previousColor);
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
    /// �I���G���A���X�V����
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
            Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
            _selectedPointsForPreview = BitmapUtils.ConvertSelectedAreaToPreviewBox(allSelectedAreas, _bmp, previewBox, inverseMode.Checked);

            int totalSelectedPoints = BitArrayUtils.GetCount(allSelectedAreas);
            Text = FORM_TITLE + $" - {enabledCount} �̑I���G���A / {enabledEraserCount} �̏����G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
        }

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// �F�̒ǉ��ݒ���X�V����
    /// </summary>
    private void UpdateColorConfigulation()
    {
        _advancedColorSettingsForm.Configuration.UpdateComponentActivationStatus();

        if (_advancedColorSettingsForm.Configuration.Enabled)
        {
            advancedColorConfigStatus.Text = "�L��";
            advancedColorConfigStatus.ForeColor = ColorUtils.DefaultForeColor;
        }
        else
        {
            advancedColorConfigStatus.Text = "����";
            advancedColorConfigStatus.ForeColor = ColorUtils.DefaultUnselectedColor;
        }

        if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
    }

    /// <summary>
    /// �e�N�X�`���f�[�^���X�V����
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
    /// �F�����X�V����
    /// </summary>
    private void UpdateColorData()
    {
        if (_previousColor == Color.Empty)
        {
            previousRGBLabel.Text = "���I��";
            previousRGBLabel.ForeColor = ColorUtils.DefaultUnselectedColor;
        }
        else
        {
            previousRGBLabel.Text = $"{_previousColor.R}, {_previousColor.G}, {_previousColor.B}";
            previousRGBLabel.ForeColor = ColorUtils.DefaultForeColor;
        }

        if (_newColor == Color.Empty)
        {
            newRGBLabel.Text = "���I��";
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

    #region �I�����[�h�֘A
    /// <summary>
    /// �I�����[�h�̏���
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
            FormUtils.ShowError("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j");
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
            FormUtils.ShowError("�w�i�F���I������Ă��܂���B");
            return;
        }

        string previousFormTitle = Text;
        Text = FORM_TITLE + " - �I��������...";

        BitArray? values = BitmapUtils.GetSelectedArea(
            originalCoordinates,
            _bmp,
            _backgroundColor
        );

        Text = previousFormTitle;

        if (values == null || values.Length == 0)
        {
            FormUtils.ShowError("�I���G���A������܂���B");
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

    #region �C�x���g�n���h���[
    /// <summary>
    /// �C�x���g�n���h���̃Z�b�g�A�b�v
    /// </summary>
    private void SetupEventHandlers()
    {
        SetupColorChangeHandlers();
        SetupFormInteractionHandlers();
        SetupPreviewZoomHandlers();
        SetupImageMaskHandlers();
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

    private void SetupImageMaskHandlers()
    {
        _selectAreaFromImageMaskForm.AreaChanged += (s, e) =>
        {
            if (_bmp == null || _previewBitmap == null)
            {
                FormUtils.ShowError("�摜���ǂݍ��܂�Ă��܂���B");
                return;
            }

            if (_selectAreaFromImageMaskForm.SelectedArea == null || _selectAreaFromImageMaskForm.SelectedArea.Length == 0)
            {
                FormUtils.ShowError("�I���G���A������܂���B");
                return;
            }
            
            if ((_bmp.Width * _bmp.Height) != _selectAreaFromImageMaskForm.SelectedArea.Length)
            {
                FormUtils.ShowError("�I���G���A�����摜�̃T�C�Y�ƈقȂ�܂��B");
                return;
            }

            _selectedAreaListForm.Add(_selectAreaFromImageMaskForm.SelectedArea);
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
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
            FormUtils.ShowError("�摜���ǂݍ��܂�Ă��܂���B");
            return;
        }

        if (_previousColor == Color.Empty)
        {
            FormUtils.ShowError("�ύX�O�̐F���I������Ă��܂���B(�v���r���[���쐬�ł��܂���)");
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
            FormUtils.ShowError("�摜���ǂݍ��܂�Ă��܂���B");
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && (_previousColor == Color.Empty || _newColor == Color.Empty))
        {
            FormUtils.ShowError("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j");
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && _backgroundColor == Color.Empty)
        {
            FormUtils.ShowInfo("�I�����[�h���L���ɂȂ�܂����B\n�y���c�[���ȊO�ŃI�u�W�F�N�g�I������ꍇ�́A�͂��߂ɔw�i�F���E�N���b�N�Őݒ肵�Ă��������B", "�I�����[�h");
        }

        selectModePanel.Enabled = selectMode.Checked;
    }

    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (balanceMode.Checked)
        {
            int currentBalanceMode = _balanceModeSettingsForm.Configuration.ModeVersion;
            var result = FormUtils.ShowConfirm(
                "�o�����X���[�h���L��������܂����I\n\n" +
                "�F���Y��ɉ��ς��邽�߂ɂ́A�ȉ��̐ݒ荀�ڂ̒�������������܂��B\n\n" +
                "�����ݒ荀�ځF\n" +
                "- V1: �d��̒���\n" +
                "- V2: ���a�̒����̒���\n" +
                "- V3: �ύX��̐F�A����уO���f�[�V�����ʒu�̒���\n\n" +
                $"�������߃o�[�W����: V3 (���݂̃o�[�W����: V{currentBalanceMode})\n\n" +
                "�ݒ��ʂ��J���܂����H",
                "�o�����X���[�h"
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
            FormUtils.ShowInfo("�I�𔽓]���[�h���I���ɂȂ�܂����B\n\n- �I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n- ���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B", "�I�𔽓]���[�h");

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
            Filter = "�摜�t�@�C��|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.ico|���ׂẴt�@�C��|*.*",
            Title = "�摜�t�@�C����I�����Ă�������"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadPictureFile(dialog.FileName);
    }

    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (_bmp == null)
        {
            FormUtils.ShowError("�摜���ǂݍ��܂�Ă��܂���B");
            return;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            FormUtils.ShowError("�F���I������Ă��܂���B");
            return;
        }

        bool result = FormUtils.ShowConfirm("�摜���쐬���܂����H");
        if (!result) return;

        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNG�t�@�C��|*.png;",
            Title = "�V�K�e�N�X�`���摜�̕ۑ����I�����Ă�������",
            FileName = FileUtils.GetNewFileName(_imageFilePath),
            InitialDirectory = FileUtils.GetInitialDirectory(_imageFilePath)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        string newFilePath = dialog.FileName;

        if (newFilePath == string.Empty)
        {
            FormUtils.ShowError("�t�@�C���̕ۑ��悪�I������Ă��܂���B");
            return;
        }

        makeButton.Text = "�쐬��...";
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
    
    private void SelectAreaFromImageMask_Click(object sender, EventArgs e)
    {
        _selectAreaFromImageMaskForm.Show();
        _selectAreaFromImageMaskForm.BringToFront();
    }

    private void UndoButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.RemoveLast();

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "�_��ȃe�N�X�`���F�ϊ��c�[��\n�w�肵���F�̍��������ƂɁA�e�N�X�`���̐F���ȒP�ɕύX�ł��܂��B\n\n";
        message += $"�c�[�����:\n�����: �Ղ����\nTwitter: @pukorufu\nGithub: {ItemUtils.GithubURL}\n\n";
        message += "���̃\�t�g�E�F�A�́A�l�̎�ō쐬���ꂽ���̂ł��B\n�������̃\�t�g�E�F�A�����ɗ������Ɗ�������A���Ўx�������肢���܂��I\n\n";
        message += "���C�Z���X:\n���̃\�t�g�E�F�A�́AMIT���C�Z���X�̂��ƂŔz�z����Ă��܂��B";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + CURRENT_VERSION);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
    {
        _balanceModeSettingsForm.Show();
        _balanceModeSettingsForm.BringToFront();
    }

    private async void UpdateCheck_Click(object sender, EventArgs e)
        => await UpdateUtils.CheckUpdate(CURRENT_VERSION);

    private void DonationButton_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm(
            "�x�����Ă���������ꍇ�́A�ȉ��̃����N���J���܂��B\n\n" +
            $"�x����: {ItemUtils.ItemURL}\n\n" +
            "�x���͔C�ӂł��B�����̂Ȃ��͈͂ł��肢���܂��B"
        );

        if (!result) return;

        ItemUtils.OpenItemURL();
    }

    private void ImportColorSettings_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog()
        {
            Filter = "�F���ݒ�t�@�C��|*.ccs",
            Title = "�F���ݒ�t�@�C����I�����Ă�������"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadColorSettingsFile(dialog.FileName);
    }

    private void ExportColorSettings_Click(object? sender, EventArgs? e)
    {
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "�F���ݒ�t�@�C��|*.ccs",
            Title = "�F���ݒ�t�@�C���̕ۑ����I�����Ă�������",
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
