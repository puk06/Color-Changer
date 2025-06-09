using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace ColorChanger.Forms;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.13";
    private static readonly string FORM_TITLE = $"Color Changer For Texture {CURRENT_VERSION}";
    private static readonly Point VERSION_LABEL_POSITION = new Point(275, 54);
    private const int COLOR_UPDATE_DEBOUNCE_MS = 16;

    private static readonly string ITEM_URL = "https://pukorufu.booth.pm/items/6519471";
    private readonly ProcessStartInfo _processStartInfo = new ProcessStartInfo()
    {
        FileName = ITEM_URL,
        UseShellExecute = true
    };

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Color _backgroundColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;
    private DateTime _lastUpdateCall = DateTime.MinValue;

    private Bitmap? _bmp;
    private string? _imageFilePath;

    private BitArray _selectedPointsForPreview = BitArrayUtils.GetEmpty();
    private Bitmap? _previewBitmap;

    internal readonly ColorPickerForm ColorPickerForm = new ColorPickerForm();
    private readonly BalanceModeSettingsForm _balanceModeSettingsForm;
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
        _balanceModeSettingsForm = new BalanceModeSettingsForm(this);
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
        UpdateTextureData();
        UpdateColorData();
        UpdateColorConfigulation();
    }

    #region �F�I���֘A
    /// <summary>
    /// �ύX�O�̐F��I������B
    /// </summary>
    private void SelectPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;
        if (_bmp == null || _previewBitmap == null) return;

        if (!BitmapUtils.IsValidCoordinate(e.Location, _previewBitmap.Size)) return;

        if ((DateTime.Now - _lastUpdateCall).TotalMilliseconds <= COLOR_UPDATE_DEBOUNCE_MS) return;
        _lastUpdateCall = DateTime.Now;

        Color selectedColor = _previewBitmap.GetPixel(e.X, e.Y);

        if (selectMode.Checked)
        {
            Point originalCoordinates = BitmapUtils.ConvertToOriginalCoordinates(e, previewBox, _bmp);
            HandleSelectionMode(e, selectedColor, originalCoordinates);
            return;
        }

        previousColorBox.BackColor = selectedColor;
        _previousColor = selectedColor;
        ColorPickerForm.SetInitialColor(selectedColor);

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
                    FormUtils.ShowError("���ߗp�摜���쐬�ł��܂���ł����B");
                }
            }
            else
            {
                bitMap.Save(filePath);
            }

            bitMap.Dispose();
            FormUtils.ShowInfo("�e�N�X�`���摜�̍쐬���������܂����B");
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
            if (transMode.Checked)
            {
                FormUtils.ShowInfo("�I���G���A���Ȃ��������߁A���߃��[�h�̓X�L�b�v����܂��B");
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
                FormUtils.ShowError("�摜�̓ǂݍ��݂Ɏ��s���܂����B��Ή��̃t�@�C���ł��B");
            }
            else
            {
                FormUtils.ShowError("�摜�̓ǂݍ��݂Ɏ��s���܂����B\n\n�G���[: " + exception);
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
            ColorPickerForm.SetInitialColor(Color.Empty);

            previousColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            newColorBox.BackColor = ColorUtils.DefaultBackgroundColor;
            backgroundColorBox.BackColor = ColorUtils.DefaultBackgroundColor;

            _clickedPoint = Point.Empty;

            _selectedPointsForPreview = BitArrayUtils.GetEmpty();
            _selectedAreaListForm.Clear();

            Text = FORM_TITLE;
            selectMode.Checked = false;

            UpdateTextureData();
            UpdateColorData();

            ColorPickerForm.Hide();
            _balanceModeSettingsForm.ResetGradientPreviewImage();
        }
    }

    /// <summary>
    /// �I���G���A���X�V����
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
            Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
            _selectedPointsForPreview = BitmapUtils.ConvertSelectedAreaToPreviewBox(allSelectedAreas, _bmp, previewBox, inverseMode.Checked);

            int totalSelectedPoints = BitArrayUtils.GetCount(allSelectedAreas);
            Text = FORM_TITLE + $" - {enabledCount} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
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
            FormUtils.ShowError("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j");
            return;
        }

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
    #endregion

    #region �C�x���g�n���h���[
    /// <summary>
    /// �C�x���g�n���h���̃Z�b�g�A�b�v
    /// </summary>
    private void SetupEventHandlers()
    {
        _balanceModeSettingsForm.ConfigurationChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        ColorPickerForm.ColorChanged += (s, e) =>
        {
            if (_previewBitmap == null || _previousColor == Color.Empty) return;

            Color color = ColorPickerForm.SelectedColor;

            _newColor = color;
            newColorBox.BackColor = color;

            UpdateColorData();

            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_previewBitmap));
        };

        _selectedAreaListForm.OnCheckedChanged += (s, e)
            => UpdateSelectedArea();

        _advancedColorSettingsForm.ConfigurationChanged += (s, e)
            => UpdateColorConfigulation();
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
            FormUtils.ShowInfo("�I�����[�h���L���ɂȂ�܂����B�͂��߂ɔw�i�F���E�N���b�N�Őݒ肵�Ă��������B", "�I�����[�h");
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
            Filter = "�摜�t�@�C��|*.png;*.jpg;*.jpeg;*.bmp;",
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

    private void UndoButton_Click(object sender, EventArgs e)
        => _selectedAreaListForm.RemoveLast();

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "�_��ȃe�N�X�`���F�ϊ��c�[��\n�w�肵���F�̍��������ƂɁA�e�N�X�`���̐F���ȒP�ɕύX�ł��܂��B\n\n";
        message += "�c�[�����:\n�����: �Ղ����\nTwitter: @pukorufu\nGithub: https://github.com/puk06/Color-Changer\n\n";
        message += "���̃\�t�g�E�F�A�́A�l�̎�ō쐬���ꂽ���̂ł��B\n�������̃\�t�g�E�F�A�����ɗ������Ɗ�������A���Ўx�������肢���܂��I\n\n";
        message += "���C�Z���X:\n���̃\�t�g�E�F�A�́AMIT���C�Z���X�̂��ƂŔz�z����Ă��܂��B";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + CURRENT_VERSION);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
    {
        _balanceModeSettingsForm.Show();
        _balanceModeSettingsForm.BringToFront();
    }

    private void DonationButton_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm(
            "�x�����Ă���������ꍇ�́A�ȉ��̃����N���J���܂��B\n\n" +
            "�x����: https://pukorufu.booth.pm/items/6519471\n\n" +
            "�x���͔C�ӂł��B�����̂Ȃ��͈͂ł��肢���܂��B"
        );

        if (!result) return;

        try
        {
            Process.Start(_processStartInfo);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"�����N���J�����Ƃ��ł��܂���ł����B\n{ex.Message}");
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
        => e.Effect = DragDropEffects.All;
    #endregion
}
