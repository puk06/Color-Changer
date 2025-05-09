using ColorChanger.ImageProcessing;
using ColorChanger.Models;
using ColorChanger.Utils;
using System.Drawing.Imaging;

namespace ColorChanger.Forms;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.11";
    private const string FORM_TITLE = $"Color Changer For Texture {CURRENT_VERSION}";

    private Color _previousColor = Color.Empty;
    private Color _newColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;

    private static readonly Color lightGray = Color.LightGray;
    private readonly Color DEFAULT_BACKGROUND_COLOR = lightGray;

    private Bitmap? _bmp;
    private string? _filePath;

    // �I�����[�h
    private Color _backgroundColor;
    private (int x, int y)[][]? _selectedPointsArray;
    private (int x, int y)[][]? _selectedPointsArrayForPreview;

    private readonly BalanceModeSettings _balanceModeSettings = new BalanceModeSettings();

    public MainForm()
    {
        InitializeComponent();

        Text = FORM_TITLE;

        previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        previewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        coloredPreviewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;

        _balanceModeSettings.ValueChanged += (s, e) =>
        {
            if (_bmp == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
            coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
        };
    }

    // �t�@�C�����J��
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

    // �t�@�C���̃h���b�O&�h���b�v
    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

    // PreviewBox�̏���
    private void PreviewBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (_bmp == null || _newColor == Color.Empty || _previousColor == Color.Empty) return;

        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    // PreviewBox�̕`��
    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;

        if (sender is PictureBox pictureBox)
        {
            Color color = pictureBox.Name == "previewBox" ? _previousColor : _newColor;

            Color inverseColor = ColorUtils.InverseColor(color);
            Pen pen = new Pen(inverseColor, 2);

            e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
            e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
        }
    }

    // �F�̑I��
    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (_bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_previousColor == Color.Empty)
        {
            MessageBox.Show("�ύX�O�̐F���I������Ă��܂���B(�v���r���[���쐬�ł��܂���)", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AllowDrop = false;

        ColorPicker colorPicker = new ColorPicker(_newColor == Color.Empty ? _previousColor : _newColor);
        colorPicker.ShowDialog();

        AllowDrop = true;

        Color color = colorPicker.SelectedColor;

        newColorBox.BackColor = color;
        _newColor = color;
        newRGBLabel.Text = $"RGB: ({color.R}, {color.G}, {color.B})";

        UpdateCalculatedRGBValue();

        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    // �쐬�{�^��
    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (_bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            MessageBox.Show("�F���I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var result = MessageBox.Show("�摜���쐬���܂����H", "�m�F", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes) return;

        var originalFileName = Path.GetFileNameWithoutExtension(_filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        var originalExtension = Path.GetExtension(_filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        var newFileName = originalFileName + "_new" + originalExtension;

        // ���O�����ĕۑ�
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNG�t�@�C��|*.png;",
            Title = "�V�K�e�N�X�`���摜�̕ۑ����I�����Ă�������",
            FileName = newFileName,
            InitialDirectory = Path.GetDirectoryName(_filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        var newFilePath = dialog.FileName;

        if (newFilePath == "")
        {
            MessageBox.Show("�t�@�C���̕ۑ��悪�I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MakeButton.Text = "�쐬��...";
        MakeButton.Enabled = false;
        ChangeColor(_previousColor, _newColor, newFilePath);
    }

    // �g�����{�^��
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

        MessageBox.Show(message, "�\�t�g�̎g�����ꗗ", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ��������
    private void ChangeColor(Color previousColor, Color newColor, string filePath)
    {
        try
        {
            if (_bmp == null) return;
            if (previousColor == Color.Empty || newColor == Color.Empty) return;

            var bitMap = new Bitmap(_bmp);
            var rect = new Rectangle(0, 0, bitMap.Width, bitMap.Height);

            Bitmap? rawBitMap = null;
            BitmapData? rawBitMapData = null;
            if (InverseMode.Checked)
            {
                rawBitMap = new Bitmap(_bmp);
                rawBitMapData = rawBitMap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            }

            ColorDifference colorDifference = new ColorDifference(
                newColor.R - previousColor.R,
                newColor.G - previousColor.G,
                newColor.B - previousColor.B
            );

            // ���ߗp�̃r�b�g�}�b�v���쐬
            Bitmap? transBitmap = null;
            BitmapData? transData = null;
            if (transMode.Checked && !InverseMode.Checked)
            {
                transBitmap = new Bitmap(_bmp.Width, _bmp.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(transBitmap);
                g.Clear(Color.Transparent);
                g.Dispose();
                transData = transBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            }

            var data = bitMap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var skipped = false;

            unsafe
            {
                Span<ColorPixel> sourcePixels = new Span<ColorPixel>((void*)data.Scan0, (data.Stride * data.Height) / sizeof(ColorPixel));
                Span<ColorPixel> rawPixels = rawBitMap != null && rawBitMapData != null ? new Span<ColorPixel>((void*)rawBitMapData.Scan0, (rawBitMapData.Stride * rawBitMap.Height) / sizeof(ColorPixel)) : default;
                Span<ColorPixel> transPixels = transData != null ? new Span<ColorPixel>((void*)transData.Scan0, (transData.Stride * bitMap.Height) / sizeof(ColorPixel)) : default;

                ImageProcessor imageProcessor = new ImageProcessor(
                    bitMap.Width, bitMap.Height,
                    previousColor, colorDifference,
                    balanceMode.Checked, _balanceModeSettings.Configuration
                );

                if (_selectedPointsArray == null)
                {
                    if (transMode.Checked)
                    {
                        MessageBox.Show("�I���G���A���Ȃ��������߁A���߃��[�h�̓X�L�b�v����܂��B", "���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        skipped = true;
                    }

                    imageProcessor.ProcessAllPixels(sourcePixels, sourcePixels);
                }
                else if (transMode.Checked)
                {
                    if (InverseMode.Checked)
                    {
                        imageProcessor.ProcessTransparentInverse(sourcePixels, _selectedPointsArray);
                    }
                    else
                    {
                        if (transPixels == default) MessageBox.Show("���߉摜�p�f�[�^�̎擾�Ɏ��s���܂����B�f�t�H���g�̉摜���g�p����܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        imageProcessor.ProcessTransparentSelectedPixels(sourcePixels, transPixels, _selectedPointsArray);
                    }
                }
                else if (InverseMode.Checked)
                {
                    imageProcessor.ProcessAllPixels(sourcePixels, sourcePixels);

                    if (rawBitMap == null || rawBitMapData == null)
                    {
                        MessageBox.Show("���摜�̃f�[�^�̎擾�Ɏ��s���܂����B�I�𔽓]���[�h�̌��ʂ͍쐬����܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        imageProcessor.ProcessInverseSelectedPixels(sourcePixels, rawPixels, _selectedPointsArray);
                    }
                }
                else
                {
                    imageProcessor.ProcessSelectedPixels(sourcePixels, sourcePixels, _selectedPointsArray);
                }
            }

            // �r�b�g�}�b�v�̃��b�N������
            bitMap.UnlockBits(data);

            if (rawBitMap != null && rawBitMapData != null)
            {
                rawBitMap.UnlockBits(rawBitMapData);
                rawBitMap.Dispose();
            }

            if (!skipped && transMode.Checked && !InverseMode.Checked)
            {
                if (transBitmap == null || transData == null)
                {
                    MessageBox.Show("���ߗp�摜���쐬�ł��܂���ł����B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    transBitmap.UnlockBits(transData);
                    transBitmap.Save(filePath);
                    transBitmap.Dispose();
                }
            }
            else
            {
                bitMap.Save(filePath);
            }

            bitMap.Dispose();

            MessageBox.Show("�e�N�X�`���摜�̍쐬���������܂����B", "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception e)
        {
            MessageBox.Show("�e�N�X�`���摜�쐬���ɃG���[���������܂����B\n" + e.Message, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            MakeButton.Enabled = true;
            MakeButton.Text = "�쐬";
        }
    }

    // �v���r���[�摜�̐���
    private Bitmap GenerateColoredPreview(Bitmap sourceBitmap)
    {
        int boxHeight = coloredPreviewBox.Height;
        int boxWidth = coloredPreviewBox.Width;

        float ratioX = (float)sourceBitmap.Width / boxWidth;
        float ratioY = (float)sourceBitmap.Height / boxHeight;

        Bitmap _previewBitmap = new Bitmap(boxWidth, boxHeight, PixelFormat.Format32bppArgb);

        ColorDifference colorDifference = new ColorDifference(
            _newColor.R - _previousColor.R,
            _newColor.G - _previousColor.G,
            _newColor.B - _previousColor.B
        );

        // ���̃r�b�g�}�b�v�����b�N
        var sourceRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
        var sourceBitmapData = sourceBitmap.LockBits(sourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        // �v���r���[�p�̃r�b�g�}�b�v�����b�N
        var previewRect = new Rectangle(0, 0, _previewBitmap.Width, _previewBitmap.Height);
        var previewBitmapData = _previewBitmap.LockBits(previewRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        unsafe
        {
            var sourcePixels = new Span<ColorPixel>(
                (void*)sourceBitmapData.Scan0,
                (sourceBitmapData.Stride / 4) * sourceBitmapData.Height
            );

            var previewPixels = new Span<ColorPixel>(
                (void*)previewBitmapData.Scan0,
                (previewBitmapData.Stride / 4) * previewBitmapData.Height
            );

            ImageProcessor imageProcessor = new ImageProcessor(
                sourceBitmapData.Width, sourceBitmapData.Height,
                _previousColor, colorDifference,
                balanceMode.Checked, _balanceModeSettings.Configuration
            );

            imageProcessor.ProcessAllPreviewPixels(sourcePixels, previewPixels, ratioX, ratioY, boxWidth, boxHeight);

            // �I��͈͂̕`��
            if (_selectedPointsArrayForPreview != null)
            {
                ImageProcessor.ChangeSelectedPixelsColor(previewPixels, boxWidth, _selectedPointsArrayForPreview, new ColorPixel(255, 0, 0, 255));
            }
        }

        // �r�b�g�}�b�v�̃��b�N������
        sourceBitmap.UnlockBits(sourceBitmapData);
        _previewBitmap.UnlockBits(previewBitmapData);

        return _previewBitmap;
    }

    // �ύX�O�̐F�̑I�� (�I�����[�h�̏��������̒��ɓ����Ă���)
    private void SetPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;

        int x = e.X;
        int y = e.Y;

        // ���̉摜�̃T�C�Y���擾
        Bitmap previewImage = (Bitmap)previewBox.Image;
        if (previewImage == null) return;

        int originalWidth = previewImage.Width;
        int originalHeight = previewImage.Height;

        // PictureBox �̕\���T�C�Y���擾
        int pictureBoxWidth = previewBox.Width;
        int pictureBoxHeight = previewBox.Height;

        // PictureBox �̃T�C�Y�ƌ��̉摜�̃T�C�Y�̔䗦���v�Z
        float ratioX = (float)originalWidth / pictureBoxWidth;
        float ratioY = (float)originalHeight / pictureBoxHeight;

        // �N���b�N�������W�����̉摜�̍��W�ɕϊ�
        int originalX = (int)(x * ratioX);
        int originalY = (int)(y * ratioY);

        // ���W���摜�͈̔͊O�Ȃ牽�����Ȃ�
        if (originalX < 0 || originalX >= originalWidth || originalY < 0 || originalY >= originalHeight) return;

        // �ϊ��������W�Ō��̉摜�̃s�N�Z���J���[���擾
        Color color = previewImage.GetPixel(originalX, originalY);

        if (selectMode.Checked)
        {
            if (e.Button == MouseButtons.Right)
            {
                _backgroundColor = color;
                backgroundColorBox.BackColor = color;
                coloredPreviewBox.Image = GenerateColoredPreview(previewImage);
                return;
            }

            if (_previousColor == Color.Empty || _newColor == Color.Empty)
            {
                MessageBox.Show("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_backgroundColor == Color.Empty)
            {
                MessageBox.Show("�w�i�F���I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var previousFormTitle = Text;
            Text = FORM_TITLE + " - �I��������...";
            (int x, int y)[] values = BitmapUtils.GetSelectedArea(new Point(originalX, originalY), previewImage, _backgroundColor);
            Text = previousFormTitle;

            if (values.Length == 0)
            {
                MessageBox.Show("�I���\�ȃG���A������܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_selectedPointsArray == null)
            {
                var newSelectedPointsArray = Array.Empty<(int x, int y)[]>();
                _selectedPointsArray = newSelectedPointsArray.Append(values).ToArray();
            }
            else
            {
                if (_selectedPointsArray.Any(points => points.Intersect(values).Any()))
                {
                    MessageBox.Show("�I���ς݂̃G���A���܂܂�Ă��܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _selectedPointsArray = _selectedPointsArray.Append(values).ToArray();
            }

            previousFormTitle = Text;
            Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
            _selectedPointsArrayForPreview = _selectedPointsArray.Select(points => BitmapUtils.ConvertSelectedAreaToPreviewBox(points, previewImage, previewBox)).ToArray();
            Text = previousFormTitle;

            var totalSelectedPoints = _selectedPointsArray.Sum(points => points.Length);
            Text = FORM_TITLE + $" - {_selectedPointsArray.Length} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
            coloredPreviewBox.Image = GenerateColoredPreview(previewImage);
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

    // �v�Z���RGB���x���̒l�̍X�V
    private void UpdateCalculatedRGBValue()
    {
        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = "";
            return;
        }

        int diffR = _newColor.R - _previousColor.R;
        int diffG = _newColor.G - _previousColor.G;
        int diffB = _newColor.B - _previousColor.B;

        var diffRStr = diffR > 0 ? "+" + diffR : diffR.ToString();
        var diffGStr = diffG > 0 ? "+" + diffG : diffG.ToString();
        var diffBStr = diffB > 0 ? "+" + diffB : diffB.ToString();

        calculatedRGBLabel.Text = $"�v�Z���RGB: ({diffRStr}, {diffGStr}, {diffBStr})";
    }

    // �摜�̓ǂݍ���
    private void LoadPictureFile(string path)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show("�t�@�C�������݂��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            if (_bmp != null)
            {
                _bmp.Dispose();
                _bmp = null;
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _bmp = new Bitmap(stream);
        }
        catch (Exception exception)
        {
            if (exception is ArgumentException)
            {
                MessageBox.Show("�摜�̓ǂݍ��݂Ɏ��s���܂����B��Ή��̃t�@�C���ł��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("�摜�̓ǂݍ��݂Ɏ��s���܂����B\n\n�G���[: " + exception, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;
        }

        _filePath = path;

        if (previewBox.Image != null)
        {
            previewBox.Image.Dispose();
            previewBox.Image = null;
        }

        previewBox.Image = _bmp;

        if (coloredPreviewBox.Image != null)
        {
            coloredPreviewBox.Image.Dispose();
            coloredPreviewBox.Image = null;
        }

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

        _selectedPointsArray = null;
        _selectedPointsArrayForPreview = null;

        Text = FORM_TITLE;
        selectMode.Checked = false;
    }

    // �߂�{�^��(�I�����[�h)
    private void UndoButton_Click(object sender, EventArgs e)
    {
        if (_bmp == null) return;
        if (_selectedPointsArray == null || _selectedPointsArray.Length == 0) return;
        _selectedPointsArray = _selectedPointsArray.Length == 1 ? null : _selectedPointsArray[..^1];

        if (_selectedPointsArray == null)
        {
            _selectedPointsArrayForPreview = null;
            Text = FORM_TITLE;

            coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
            return;
        }

        _selectedPointsArrayForPreview = _selectedPointsArray.Select(points => BitmapUtils.ConvertSelectedAreaToPreviewBox(points, _bmp, previewBox)).ToArray();

        var totalSelectedPoints = _selectedPointsArray.Sum(points => points.Length);
        if (_selectedPointsArray.Length == 0)
        {
            Text = FORM_TITLE;
        }
        else
        {
            Text = FORM_TITLE + $" - {_selectedPointsArray.Length} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
        }

        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    // �I�����[�h�̗L�����A������
    private void SelectMode_CheckedChanged(object sender, EventArgs e)
    {
        if (selectMode.Checked && _bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && (_previousColor == Color.Empty || _newColor == Color.Empty))
        {
            MessageBox.Show("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && _backgroundColor == Color.Empty)
        {
            MessageBox.Show("�I�����[�h���L���ɂȂ�܂����B�͂��߂ɔw�i�F���E�N���b�N�Őݒ肵�Ă��������B", "�I�����[�h", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        backgroundColorBox.Enabled = selectMode.Checked;
        backgroundColorLabel.Enabled = selectMode.Checked;
        UndoButton.Enabled = selectMode.Checked;
    }

    // �o�����X���[�h�̗L�����A������
    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (_bmp == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (InverseMode.Checked) MessageBox.Show("�I�𔽓]���[�h���I���ɂȂ�܂����B\n\n- �I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n- ���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B", "�I�𔽓]���[�h", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "�_��ȃe�N�X�`���F�ϊ��c�[��\n�w�肵���F�̍��������ƂɁA�e�N�X�`���̐F���ȒP�ɕύX�ł��܂��B\n\n";
        message += "�c�[�����:\n�����: �Ղ����\nTwitter: @pukorufu\nGithub: https://github.com/puk06/Color-Changer\n\n";
        message += "���̃\�t�g�E�F�A�́A�l�̎�ō쐬���ꂽ���̂ł��B\n�������̃\�t�g�E�F�A�����ɗ������Ɗ�������A���Ўx�������肢���܂��I\n�x����: https://pukorufu.booth.pm/items/6519471\n\n";
        message += "���C�Z���X:\n���̃\�t�g�E�F�A�́AMIT���C�Z���X�̂��ƂŔz�z����Ă��܂��B";

        MessageBox.Show(message, "Color Changer For Texture " + CURRENT_VERSION, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e) => _balanceModeSettings.Show();
}
