using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Drawing.Imaging;

namespace ColorChanger.Forms;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.11";
    private const string FORM_TITLE = $"Color Changer For Texture {CURRENT_VERSION}";

    private static readonly Color DEFAULT_BACKGROUND_COLOR = Color.LightGray;

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Color _backgroundColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;

    private Bitmap? _bmp;
    private string? _imageFilePath;

    // �I�����[�h
    private BitArray _selectedPoints = BitArrayUtils.GetEmpty();
    private BitArray _selectedPointsForPreview = BitArrayUtils.GetEmpty();
    private List<BitArray> _selectedHistory = new List<BitArray>();

    private readonly ColorPickerForm _colorPicker = new ColorPickerForm();
    private readonly BalanceModeSettingsForm _balanceModeSettings = new BalanceModeSettingsForm();

    private ColorDifference ColorDifference
        => new(_previousColor, _newColor);

    public MainForm()
    {
        InitializeComponent();

        Text = FORM_TITLE;

        previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        previewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        coloredPreviewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;

        SetupEventHandlers();
    }

    #region �F�I���֘A
    /// <summary>
    /// �ύX�O�̐F��I������B
    /// </summary>
    private void SelectPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;

        if (previewBox.Image is not Bitmap previewImage) return;

        Point originalCoordinates = BitmapUtils.ConvertToOriginalCoordinates(e, previewBox, previewImage);

        if (!BitmapUtils.IsValidCoordinate(originalCoordinates, previewImage.Size)) return;

        Color color = previewImage.GetPixel(originalCoordinates.X, originalCoordinates.Y);

        if (selectMode.Checked)
        {
            HandleSelectionMode(e, color, originalCoordinates, previewImage);
            return;
        }

        previousColorBox.BackColor = color;
        _previousColor = color;
        previousRGBLabel.Text = $"RGB: ({color.R}, {color.G}, {color.B})";
        UpdateCalculatedRGBValue();

        _clickedPoint = e.Location;
        previewBox.Invalidate();
        coloredPreviewBox.Invalidate();
    }

    /// <summary>
    /// �v�Z���RGB�l���X�V����
    /// </summary>
    private void UpdateCalculatedRGBValue()
    {
        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = "";
            return;
        }

        var colorDiff = new ColorDifference(
            _previousColor,
            _newColor
        );

        calculatedRGBLabel.Text = $"�v�Z���RGB: ({colorDiff})";
    }
    #endregion

    #region �����֘A
    /// <summary>
    /// �F��ύX����
    /// </summary>
    /// <param name="previousColor"></param>
    /// <param name="newColor"></param>
    /// <param name="filePath"></param>
    private void ApplyColorChange(string filePath)
    {
        try
        {
            if (_bmp == null)
            {
                FormUtils.ShowError("�摜���ǂݍ��܂�Ă��܂���B");
                return;
            }

            Rectangle rect = BitmapUtils.GetRectangle(_bmp);

            var bitMap = new Bitmap(_bmp);
            BitmapData? data = BitmapUtils.LockBitmap(bitMap, rect, ImageLockMode.ReadWrite);
            if (data == null) return;

            Bitmap? rawBitmap = BitmapUtils.CreateInverseBitmap(_bmp, InverseMode.Checked);
            BitmapData? rawBitmapData = BitmapUtils.LockBitmap(rawBitmap, rect, ImageLockMode.ReadOnly);

            Bitmap? transBitmap = BitmapUtils.CreateTransparentBitmap(_bmp, transMode.Checked, InverseMode.Checked);
            BitmapData? transData = BitmapUtils.LockBitmap(transBitmap, rect, ImageLockMode.ReadWrite);

            bool skipped = false;

            unsafe
            {
                Span<ColorPixel> sourcePixels = BitmapUtils.GetPixelSpan(data);
                Span<ColorPixel> rawPixels = BitmapUtils.GetPixelSpan(rawBitmapData);
                Span<ColorPixel> transPixels = BitmapUtils.GetPixelSpan(transData);

                var imageProcessor = new ImageProcessor(
                    bitMap.Size,
                    ColorDifference
                );

                if (balanceMode.Checked)
                    imageProcessor.SetBalanceSettings(_balanceModeSettings.Configuration);

                skipped = ProcessImage(sourcePixels, rawPixels, transPixels, imageProcessor);
            }

            bitMap.UnlockBits(data);
            if (rawBitmapData != null) rawBitmap?.UnlockBits(rawBitmapData);
            rawBitmap?.Dispose();

            if (!skipped && transMode.Checked && !InverseMode.Checked)
            {
                if (transBitmap != null && transData != null)
                {
                    transBitmap.UnlockBits(transData);
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
            FormUtils.ShowError($"�e�N�X�`���摜�쐬���ɃG���[���������܂����B\n{ex.Message}");
        }
        finally
        {
            MakeButton.Enabled = true;
            MakeButton.Text = "�쐬";
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
        ImageProcessor processor)
    {
        bool skipped = false;

        if (_selectedPoints.Length == 0)
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
            if (InverseMode.Checked)
            {
                processor.ProcessTransparentAndInversePixels(sourcePixels, _selectedPoints);
            }
            else
            {
                if (transPixels.IsEmpty)
                    FormUtils.ShowError("���߉摜�p�f�[�^�̎擾�Ɏ��s���܂����B�f�t�H���g�̉摜���g�p����܂��B");

                processor.ProcessTransparentSelectedPixels(sourcePixels, transPixels, _selectedPoints);
            }
        }
        else if (InverseMode.Checked)
        {
            processor.ProcessAllPixels(sourcePixels, sourcePixels);

            if (rawPixels.IsEmpty)
            {
                FormUtils.ShowError("���摜�̃f�[�^�̎擾�Ɏ��s���܂����B�I�𔽓]���[�h�̌��ʂ͍쐬����܂���B");
            }
            else
            {
                processor.ProcessInverseSelectedPixels(sourcePixels, rawPixels, _selectedPoints);
            }
        }
        else
        {
            processor.ProcessSelectedPixels(sourcePixels, sourcePixels, _selectedPoints);
        }

        return skipped;
    }

    /// <summary>
    /// �v���r���[�p�̃r�b�g�}�b�v�𐶐�����
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <returns></returns>
    private Bitmap GenerateColoredPreview(Bitmap sourceBitmap)
    {
        return ImageProcessorService.GeneratePreview(
            sourceBitmap,
            ColorDifference,
            balanceMode.Checked, _balanceModeSettings.Configuration,
            _selectedPointsForPreview,
            coloredPreviewBox.Size
        );
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

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _bmp = new Bitmap(stream);

            BitmapUtils.SetImage(previewBox, _bmp, disposeImage: false);
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
            BitmapUtils.ResetImage(previewBox);
        }
        finally
        {
            BitmapUtils.ResetImage(coloredPreviewBox);

            _imageFilePath = path;

            _previousColor = Color.Empty;
            _newColor = Color.Empty;
            _backgroundColor = Color.Empty;

            previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
            newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
            backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;

            _clickedPoint = Point.Empty;

            previousRGBLabel.Text = "";
            newRGBLabel.Text = "";
            calculatedRGBLabel.Text = "";

            _selectedPoints = BitArrayUtils.GetEmpty();
            _selectedPointsForPreview = BitArrayUtils.GetEmpty();
            _selectedHistory.Clear();

            Text = FORM_TITLE;
            selectMode.Checked = false;
        }
    }
    #endregion

    #region �I�����[�h�֘A
    /// <summary>
    /// �I�����[�h�̏���
    /// </summary>
    private void HandleSelectionMode(MouseEventArgs e, Color color, Point originalCoordinates, Bitmap previewImage)
    {
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

        int previousSelectedCount = BitArrayUtils.GetCount(_selectedPoints);

        BitArray values = BitmapUtils.GetSelectedArea(
            _selectedPoints,
            originalCoordinates,
            previewImage,
            _backgroundColor
        );

        int selectedCount = BitArrayUtils.GetCount(values);

        Text = previousFormTitle;

        if (previousSelectedCount == selectedCount) return;

        if (values.Length == 0)
        {
            FormUtils.ShowError("�I���\�ȃG���A������܂���B");
            return;
        }

        _selectedPoints = values;

        if (_selectedPoints.Length != 0)
            _selectedHistory.Add(BitArrayUtils.GetClone(_selectedPoints));

        AddSelectedArea(previewImage);
    }

    /// <summary>
    /// �I���G���A��ǉ�
    /// </summary>
    private void AddSelectedArea(Bitmap previewImage)
    {
        Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
        _selectedPointsForPreview = BitmapUtils.ConvertSelectedAreaToPreviewBox(_selectedPoints, previewImage, previewBox);

        int selectedPixels = BitArrayUtils.GetCount(_selectedPoints);

        Text = FORM_TITLE + $" - {_selectedHistory.Count} �̑I���G���A (���I���s�N�Z����: {selectedPixels:N0})";

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(previewImage));
    }
    #endregion

    #region �C�x���g�n���h���[
    /// <summary>
    /// �C�x���g�n���h���̃Z�b�g�A�b�v
    /// </summary>
    private void SetupEventHandlers()
    {
        _balanceModeSettings.ConfigurationChanged += (s, e) =>
        {
            if (_bmp == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_bmp));
        };

        _colorPicker.ColorChanged += (s, e) =>
        {
            if (_bmp == null || _previousColor == Color.Empty) return;

            Color color = _colorPicker.SelectedColor;

            _newColor = color;
            newColorBox.BackColor = color;
            newRGBLabel.Text = $"RGB: ({color.R}, {color.G}, {color.B})";

            UpdateCalculatedRGBValue();

            BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_bmp));
        };
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
        if (_bmp == null || _newColor == Color.Empty || _previousColor == Color.Empty) return;

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_bmp));
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

        _colorPicker.SetColor(_newColor == Color.Empty ? _previousColor : _newColor);
        _colorPicker.Show();
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

        backgroundColorBox.Enabled = selectMode.Checked;
        backgroundColorLabel.Enabled = selectMode.Checked;
        UndoButton.Enabled = selectMode.Checked;
    }

    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (_bmp == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_bmp));
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (InverseMode.Checked)
            FormUtils.ShowInfo("�I�𔽓]���[�h���I���ɂȂ�܂����B\n\n- �I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n- ���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B", "�I�𔽓]���[�h");
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

        var result = FormUtils.ShowConfirm("�摜���쐬���܂����H");
        if (!result) return;

        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNG�t�@�C��|*.png;",
            Title = "�V�K�e�N�X�`���摜�̕ۑ����I�����Ă�������",
            FileName = FileUtils.GetNewFileName(_imageFilePath),
            InitialDirectory = FileUtils.GetInitialDirectory(_imageFilePath)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        var newFilePath = dialog.FileName;

        if (newFilePath == "")
        {
            FormUtils.ShowError("�t�@�C���̕ۑ��悪�I������Ă��܂���B");
            return;
        }

        MakeButton.Text = "�쐬��...";
        MakeButton.Enabled = false;
        ApplyColorChange(newFilePath);
    }

    private void HelpUseButton_Click(object sender, EventArgs e)
    {
        string message = "���̃\�t�g�̊�{�I�Ȏg����:\n" +
            "1. �摜����ʓ��Ƀh���b�O���h���b�v���邩�A�u�t�@�C�����J���v�{�^���������ĉ摜��ǂݍ���ł��������B\n" +
            "2. �ύX�O�̐F�́A�摜�����N���b�N�܂��̓h���b�O���đI�����܂��B\n" +
            "3. �ύX��̐F�́A��ʉ����̃O���[�̘g���N���b�N���đI�����Ă��������B\n" +
            "4. �u�쐬�v�{�^���������ƁA�e�N�X�`���̍쐬���n�܂�܂��B";

        message += "\n\n�I�����[�h�̎g����:\n" +
            "- �I�����[�h�ł́A�o�͎��ɑI�����ꂽ�����̂ݐF���ύX����A���̏ꏊ�͕ύX����܂���B\n" +
            "1. �w�i�F���E�N���b�N�Őݒ肵�܂��B\n" +
            "2. �ύX���������������N���b�N�őI�����Ă��������i�E���̉摜�ɐԂ��g���Ńv���r���[����܂��B�����I�����\�ł��j�B\n" +
            "3. �I��������������A�u�I�����[�h�v�̃`�F�b�N���O���Ă��������B\n" +
            "���u�߂�v�{�^���������ƁA�Ō�ɑI�������G���A���珇�ɑI���������ł��܂��B";

        message += "\n\n�I�𔽓]���[�h�ɂ���:\n" +
            "- �I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n" +
            "- ���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B\n" +
            "���тȂǁA�ς������Ȃ����������Ȃ��ꍇ�ɗL���I�ł��B\n" +
            "�I�����[�h�݂̂��ƁA�ς������Ȃ������ȊO��S�đI��������Ԃɂ��Ȃ��Ƃ����Ȃ��̂ŕ֗��ł��B";

        message += "\n\n���߉摜�쐬���[�h�̎g����:\n" +
            "- ���߉摜�쐬���[�h�ł́A�I�����������������c�铧�߉摜�𐶐����܂��B\n" +
            "1. �I�����[�h�ŁA���߂��������Ȃ�������I�����܂��i�����I���j�B\n" +
            "2. �u���߃��[�h�v�Ƀ`�F�b�N�����Ă��������B\n" +
            "3. �u�쐬�v�{�^���������ƁA�I�����ꂽ�����������c�铧�߉摜����������܂��B";

        message += "\n\n�o�����X���[�h�ɂ���:\n" +
            "- �I�������F�ɋ߂��قǋ����A�����قǎキ�F��ύX���܂��B\n" +
            "�I���^�I�t��؂�ւ��āA�ǂ��炪��莩�R���m�F���Ă���F�ϊ������s���邱�Ƃ��������߂��܂��B�ݒ肩��l�Ȃǂ𒲐߂��邱�Ƃ��ł��܂��B\n" +
            "����肭�F���ς��Ȃ��ꍇ�́A�ݒ���̒l�𒲐����Ă݂Ă��������B\n" +
            "���u�d��v�͕ς������Ȃ��������ς���Ă��܂��ꍇ�͒l��傫���A�����ƕς������ꍇ�͏��������Ă��������B0�ɂ���ƒʏ탂�[�h�Ɠ����ɂȂ�܂��B";

        FormUtils.ShowInfo(message, "�\�t�g�̎g�����ꗗ");
    }

    private void UndoButton_Click(object sender, EventArgs e)
    {
        if (_selectedHistory.Count == 0 || _bmp == null) return;

        if (_selectedHistory.Count == 1)
        {
            _selectedHistory.Clear();
            _selectedPoints = BitArrayUtils.GetEmpty();
            _selectedPointsForPreview = BitArrayUtils.GetEmpty();

            Text = FORM_TITLE;
        }
        else
        {
            BitArray lastHistory = _selectedHistory[^2];

            _selectedHistory = _selectedHistory[..^1];
            _selectedPoints = lastHistory;
            _selectedPointsForPreview = BitmapUtils.ConvertSelectedAreaToPreviewBox(_selectedPoints, _bmp, previewBox);

            int totalSelectedPoints = BitArrayUtils.GetCount(_selectedPoints);
            Text = FORM_TITLE + $" - {_selectedHistory.Count} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
        }

        BitmapUtils.SetImage(coloredPreviewBox, GenerateColoredPreview(_bmp));
    }

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "�_��ȃe�N�X�`���F�ϊ��c�[��\n�w�肵���F�̍��������ƂɁA�e�N�X�`���̐F���ȒP�ɕύX�ł��܂��B\n\n";
        message += "�c�[�����:\n�����: �Ղ����\nTwitter: @pukorufu\nGithub: https://github.com/puk06/Color-Changer\n\n";
        message += "���̃\�t�g�E�F�A�́A�l�̎�ō쐬���ꂽ���̂ł��B\n�������̃\�t�g�E�F�A�����ɗ������Ɗ�������A���Ўx�������肢���܂��I\n�x����: https://pukorufu.booth.pm/items/6519471\n\n";
        message += "���C�Z���X:\n���̃\�t�g�E�F�A�́AMIT���C�Z���X�̂��ƂŔz�z����Ă��܂��B";

        FormUtils.ShowInfo(message, "Color Changer For Texture " + CURRENT_VERSION);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e)
        => _balanceModeSettings.Show();

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = _colorPicker.Visible ? DragDropEffects.None : DragDropEffects.All;
    #endregion
}
