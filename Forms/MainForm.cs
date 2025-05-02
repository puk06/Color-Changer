using System.Drawing.Imaging;
using static VRC_Color_Changer.Classes.Helper;

namespace VRC_Color_Changer;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.8";
    private const string FORM_TITLE = $"VRChat Color Changer {CURRENT_VERSION} by �Ղ����";

    private Color previousColor = Color.Empty;
    private Color newColor = Color.Empty;
    private Point clickedPoint = Point.Empty;

    private static readonly Color lightGray = Color.LightGray;
    private readonly Color DEFAULT_BACKGROUND_COLOR = lightGray;

    private Bitmap? bmp;
    private string? filePath;

    // �I�����[�h
    private Color backgroundColor;
    private (int x, int y)[][]? selectedPointsArray;
    private (int x, int y)[][]? selectedPointsArrayForPreview;

    public MainForm()
    {
        InitializeComponent();

        Text = FORM_TITLE;

        previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        previewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        coloredPreviewBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
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

        if (bmp == null || newColor == Color.Empty || previousColor == Color.Empty) return;

        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // PreviewBox�̕`��
    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (clickedPoint == Point.Empty) return;

        if (sender is PictureBox pictureBox)
        {
            Color color = pictureBox.Name == "previewBox" ? previousColor : newColor;

            Color inverseColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
            Pen pen = new Pen(inverseColor, 2);

            e.Graphics.DrawLine(pen, clickedPoint.X - 5, clickedPoint.Y, clickedPoint.X + 5, clickedPoint.Y);
            e.Graphics.DrawLine(pen, clickedPoint.X, clickedPoint.Y - 5, clickedPoint.X, clickedPoint.Y + 5);
        }
    }

    // �F�̑I��
    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (previousColor == Color.Empty)
        {
            MessageBox.Show("�ύX�O�̐F���I������Ă��܂���B(�v���r���[���쐬�ł��܂���)", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Disable Drag and Drop
        AllowDrop = false;

        ColorPicker colorPicker = new ColorPicker(newColor == Color.Empty ? previousColor : newColor);
        colorPicker.ShowDialog();

        // Enable Drag and Drop
        AllowDrop = true;

        Color color = colorPicker.SelectedColor;

        newColorBox.BackColor = color;
        newColor = color;
        newRGBLabel.Text = $"RGB: ({color.R}, {color.G}, {color.B})";

        UpdateCalculatedRGBValue();

        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // �쐬�{�^��
    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (previousColor == Color.Empty || newColor == Color.Empty)
        {
            MessageBox.Show("�F���I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var result = MessageBox.Show("�摜���쐬���܂����H", "�m�F", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes) return;

        var originalFileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        var originalExtension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        var newFileName = originalFileName + "_new" + originalExtension;

        // ���O�����ĕۑ�
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNG�t�@�C��|*.png;",
            Title = "�V�K�e�N�X�`���摜�̕ۑ����I�����Ă�������",
            FileName = newFileName,
            InitialDirectory = Path.GetDirectoryName(filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
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
        ChangeColor(previousColor, newColor, newFilePath);
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
            "�I�����[�h�ł́A�o�͎��ɑI�����ꂽ�����̂ݐF���ύX����A���̏ꏊ�͕ύX����܂���B\n" +
            "1. �w�i�F���E�N���b�N�Őݒ肵�܂��B\n" +
            "2. �ύX���������������N���b�N�őI�����Ă��������i�E���̉摜�ɐԂ��g���Ńv���r���[����܂��B�����I�����\�ł��j�B\n" +
            "3. �I��������������A�u�I�����[�h�v�̃`�F�b�N���O���Ă��������B\n" +
            "���u�߂�v�{�^���������ƁA�Ō�ɑI�������G���A���珇�ɑI���������ł��܂��B";

        message += "\n\n���]���[�h�ɂ���:\n" +
            "�I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n" +
            "���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B\n" +
            "���тȂǁA�ς������Ȃ����������Ȃ��ꍇ�ɗL���I�ł��B\n" +
            "�I�����[�h�݂̂��ƁA�ς������Ȃ������ȊO��S�đI��������Ԃɂ��Ȃ��Ƃ����Ȃ��̂ŕ֗��ł��B";

        message += "\n\n���߉摜�쐬���[�h�̎g����:\n" +
            "���߉摜�쐬���[�h�ł́A�I�����������������c�铧�߉摜�𐶐����܂��B\n" +
            "1. �I�����[�h�ŁA���߂��������Ȃ�������I�����܂��i�����I���j�B\n" +
            "2. �u���߃��[�h�v�Ƀ`�F�b�N�����Ă��������B\n" +
            "3. �u�쐬�v�{�^���������ƁA�I�����ꂽ�����������c�铧�߉摜����������܂��B";

        message += "\n\n�o�����X���[�h�ɂ���:\n" +
            "�I�������F�ɋ߂��قǋ����A�����قǎキ�F��ύX���܂��B\n" +
            "�I���^�I�t��؂�ւ��āA�ǂ��炪��莩�R���m�F���Ă���F�ϊ������s���邱�Ƃ��������߂��܂��B\n" +
            "����肭�F���ς��Ȃ��ꍇ�́A�u�d�݁v�̒l�𒲐����Ă݂Ă��������B�F�ύX���O���t�i����`�j�̃J�[�u�̉s�����ς��܂��B\n" +
            "���ς������Ȃ��������ς���Ă��܂��ꍇ�͒l��傫���A�����ƕς������ꍇ�͏��������Ă��������B0�ɂ���ƒʏ탂�[�h�Ɠ����ɂȂ�܂��B";

        MessageBox.Show(message, "�\�t�g�̎g�����ꗗ", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ��������
    private void ChangeColor(Color previousColor, Color newColor, string filePath)
    {
        try
        {
            if (bmp == null) return;
            if (previousColor == Color.Empty || newColor == Color.Empty) return;

            var weight = double.TryParse(weightText.Text, out double result) ? result : 1;
            if (balanceMode.Checked) weightText.Text = weight.ToString("F2");

            var bitMap = new Bitmap(bmp);
            var rect = new Rectangle(0, 0, bitMap.Width, bitMap.Height);

            Bitmap? rawBitMap = null;
            BitmapData? rawBitMapData = null;
            if (InverseMode.Checked)
            {
                rawBitMap = new Bitmap(bmp);
                rawBitMapData = rawBitMap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            }

            int diffR = newColor.R - previousColor.R;
            int diffG = newColor.G - previousColor.G;
            int diffB = newColor.B - previousColor.B;

            // ���ߗp�̃r�b�g�}�b�v���쐬
            Bitmap? transBitmap = null;
            BitmapData? transData = null;
            if (transMode.Checked && !InverseMode.Checked)
            {
                transBitmap = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(transBitmap);
                g.Clear(Color.Transparent);
                g.Dispose();
                transData = transBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            }

            var data = bitMap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var skipped = false;

            unsafe
            {
                byte* sourcePtr = (byte*)data.Scan0;
                byte* rawPtr = rawBitMapData != null ? (byte*)rawBitMapData.Scan0 : null;
                byte* transPtr = transData != null ? (byte*)transData.Scan0 : null;

                void ProcessPixel(int x, int y, byte* targetPtr)
                {
                    int pixelIndex = (y * data.Stride) + (x * 4);
                    if (sourcePtr[pixelIndex + 3] == 0) return; // A �`�����l���� 0 �Ȃ�X�L�b�v

                    int r = sourcePtr[pixelIndex + 2]; // ��
                    int g = sourcePtr[pixelIndex + 1]; // ��
                    int b = sourcePtr[pixelIndex + 0]; // ��

                    if (balanceMode.Checked)
                    {
                        var (hasIntersection, IntersectionDistance) = GetRGBIntersectionDistance(previousColor.R, previousColor.G, previousColor.B, r, g, b);

                        //��ԏ�ł̋������v�Z
                        double distance = Math.Sqrt(
                            Math.Pow(r - previousColor.R, 2) +
                            Math.Pow(g - previousColor.G, 2) +
                            Math.Pow(b - previousColor.B, 2)
                        );

                        // �ω���
                        double adjustmentFactor = CalculateColorChangeRate(hasIntersection, IntersectionDistance, distance, weight);

                        // RGB�̍�����␳
                        int adjustedDiffR = (int)(diffR * adjustmentFactor);
                        int adjustedDiffG = (int)(diffG * adjustmentFactor);
                        int adjustedDiffB = (int)(diffB * adjustmentFactor);

                        // �␳���RGB�l���v�Z
                        r = Math.Clamp(r + adjustedDiffR, 0, 255); // ��
                        g = Math.Clamp(g + adjustedDiffG, 0, 255); // ��
                        b = Math.Clamp(b + adjustedDiffB, 0, 255); // ��
                    }
                    else
                    {
                        r = Math.Clamp(r + diffR, 0, 255); // ��
                        g = Math.Clamp(g + diffG, 0, 255); // ��
                        b = Math.Clamp(b + diffB, 0, 255); // ��
                    }

                    targetPtr[pixelIndex + 2] = (byte)r;
                    targetPtr[pixelIndex + 1] = (byte)g;
                    targetPtr[pixelIndex + 0] = (byte)b;

                    if (targetPtr == transPtr)
                    {
                        targetPtr[pixelIndex + 3] = sourcePtr[pixelIndex + 3];
                    }
                }

                void ProcessInversePixel(int x, int y, byte* targetPtr)
                {
                    int pixelIndex = (y * data.Stride) + (x * 4);
                    if (targetPtr[pixelIndex + 3] == 0) return; // A �`�����l���� 0 �Ȃ�X�L�b�v

                    targetPtr[pixelIndex + 2] = rawPtr[pixelIndex + 2];
                    targetPtr[pixelIndex + 1] = rawPtr[pixelIndex + 1];
                    targetPtr[pixelIndex + 0] = rawPtr[pixelIndex + 0];
                }

                void ProcessTransPixel(int x, int y, byte* targetPtr)
                {
                    int pixelIndex = (y * data.Stride) + (x * 4);
                    if (targetPtr[pixelIndex + 3] == 0) return; // A �`�����l���� 0 �Ȃ�X�L�b�v

                    targetPtr[pixelIndex + 2] = 0;
                    targetPtr[pixelIndex + 1] = 0;
                    targetPtr[pixelIndex + 0] = 0;
                    targetPtr[pixelIndex + 3] = 0;
                }

                void ProcessAllPixels(byte* ptr)
                {
                    for (int y = 0; y < bitMap.Height; y++)
                        for (int x = 0; x < bitMap.Width; x++)
                            ProcessPixel(x, y, ptr);
                }

                void ProcessSelectedPixels(byte* ptr)
                {
                    foreach (var selectedPoints in selectedPointsArray)
                        foreach (var (x, y) in selectedPoints)
                            ProcessPixel(x, y, ptr);
                }

                void ProcessInverseSelectedPixels()
                {
                    if (rawBitMap == null || rawBitMapData == null)
                    {
                        MessageBox.Show("���摜�̃f�[�^�̎擾�Ɏ��s���܂����B�I�𔽓]���[�h�̌��ʂ͍쐬����܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    foreach (var selectedPoints in selectedPointsArray)
                        foreach (var (x, y) in selectedPoints)
                            ProcessInversePixel(x, y, sourcePtr);
                }

                void ProcessTransparentSelectedPixels()
                {
                    if (transPtr == null)
                    {
                        MessageBox.Show("���߉摜�p�f�[�^�̎擾�Ɏ��s���܂����B�f�t�H���g�̉摜���g�p����܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    foreach (var selectedPoints in selectedPointsArray)
                        foreach (var (x, y) in selectedPoints)
                            ProcessPixel(x, y, transPtr != null ? transPtr : sourcePtr);
                }

                void ProcessTransparentInverse()
                {
                    ProcessAllPixels(sourcePtr);
                    foreach (var selectedPoints in selectedPointsArray)
                        foreach (var (x, y) in selectedPoints)
                            ProcessTransPixel(x, y, sourcePtr);
                }

                if (selectedPointsArray == null)
                {
                    if (transMode.Checked)
                    {
                        MessageBox.Show("�I���G���A���Ȃ��������߁A���߃��[�h�̓X�L�b�v����܂��B", "���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        skipped = true;
                    }

                    ProcessAllPixels(sourcePtr);
                }
                else if (transMode.Checked)
                {
                    if (InverseMode.Checked)
                    {
                        ProcessTransparentInverse();
                    }
                    else
                    {
                        ProcessTransparentSelectedPixels();
                    }
                }
                else if (InverseMode.Checked)
                {
                    ProcessAllPixels(sourcePtr);
                    ProcessInverseSelectedPixels();
                }
                else
                {
                    ProcessSelectedPixels(sourcePtr);
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

            MessageBox.Show("�e�N�X�`���摜�̍쐬���������܂����B"
                , "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        var weight = double.TryParse(weightText.Text, out double result) ? result : 1;

        if (balanceMode.Checked)
        {
            weightText.Text = weight.ToString("F2");
        }

        int boxHeight = coloredPreviewBox.Height;
        int boxWidth = coloredPreviewBox.Width;

        float ratioX = (float)sourceBitmap.Width / boxWidth;
        float ratioY = (float)sourceBitmap.Height / boxHeight;

        Bitmap _previewBitmap = new Bitmap(boxWidth, boxHeight, PixelFormat.Format32bppArgb);

        int diffR = newColor.R - previousColor.R;
        int diffG = newColor.G - previousColor.G;
        int diffB = newColor.B - previousColor.B;

        // ���̃r�b�g�}�b�v�����b�N
        var sourceRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
        var sourceBitmapData = sourceBitmap.LockBits(sourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        // �v���r���[�p�̃r�b�g�}�b�v�����b�N
        var previewRect = new Rectangle(0, 0, _previewBitmap.Width, _previewBitmap.Height);
        var previewBitmapData = _previewBitmap.LockBits(previewRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        unsafe
        {
            byte* sourcePtr = (byte*)sourceBitmapData.Scan0;
            byte* previewPtr = (byte*)previewBitmapData.Scan0;

            for (int y = 0; y < boxHeight; y++)
            {
                for (int x = 0; x < boxWidth; x++)
                {
                    // ���̃r�b�g�}�b�v�̑Ή�������W���v�Z
                    int sourceX = (int)(x * ratioX);
                    int sourceY = (int)(y * ratioY);

                    int pixelIndex = (sourceY * sourceBitmapData.Stride) + (sourceX * 4);

                    // �s�N�Z���f�[�^���擾
                    int b = sourcePtr[pixelIndex + 0];
                    int g = sourcePtr[pixelIndex + 1];
                    int r = sourcePtr[pixelIndex + 2];
                    int a = sourcePtr[pixelIndex + 3];

                    if (a == 0) continue; // �����ȃs�N�Z���͂��̂܂܃X�L�b�v

                    int newR = r;
                    int newG = g;
                    int newB = b;

                    if (balanceMode.Checked)
                    {
                        var (hasIntersection, IntersectionDistance) = GetRGBIntersectionDistance(previousColor.R, previousColor.G, previousColor.B, newR, newG, newB);

                        //��ԏ�ł̋������v�Z
                        double distance = Math.Sqrt(
                            Math.Pow(r - previousColor.R, 2) +
                            Math.Pow(g - previousColor.G, 2) +
                            Math.Pow(b - previousColor.B, 2)
                        );

                        // �ω���
                        double adjustmentFactor = CalculateColorChangeRate(hasIntersection, IntersectionDistance, distance, weight);

                        // RGB�̍�����␳
                        int adjustedDiffR = (int)(diffR * adjustmentFactor);
                        int adjustedDiffG = (int)(diffG * adjustmentFactor);
                        int adjustedDiffB = (int)(diffB * adjustmentFactor);

                        // �␳���RGB�l���v�Z
                        newR = Math.Clamp(r + adjustedDiffR, 0, 255); // ��
                        newG = Math.Clamp(g + adjustedDiffG, 0, 255); // ��
                        newB = Math.Clamp(b + adjustedDiffB, 0, 255); // ��
                    }
                    else
                    {
                        // RGB �l��ύX
                        newR = Math.Clamp(r + diffR, 0, 255);
                        newG = Math.Clamp(g + diffG, 0, 255);
                        newB = Math.Clamp(b + diffB, 0, 255);
                    }

                    // �v���r���[�p�r�b�g�}�b�v�ɏ�������
                    int previewIndex = (y * previewBitmapData.Stride) + (x * 4);
                    previewPtr[previewIndex + 0] = (byte)newB;
                    previewPtr[previewIndex + 1] = (byte)newG;
                    previewPtr[previewIndex + 2] = (byte)newR;
                    previewPtr[previewIndex + 3] = (byte)a;
                }
            }

            if (selectedPointsArrayForPreview != null)
            {
                foreach (var selectedPoints in selectedPointsArrayForPreview)
                {
                    foreach (var (x, y) in selectedPoints)
                    {
                        int previewIndex = (y * previewBitmapData.Stride) + (x * 4);
                        previewPtr[previewIndex + 0] = 0;
                        previewPtr[previewIndex + 1] = 0;
                        previewPtr[previewIndex + 2] = 255;
                        previewPtr[previewIndex + 3] = 255;
                    }
                }
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
                backgroundColor = color;
                backgroundColorBox.BackColor = color;
                coloredPreviewBox.Image = GenerateColoredPreview(previewImage);
                return;
            }

            if (previousColor == Color.Empty || newColor == Color.Empty)
            {
                MessageBox.Show("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (backgroundColor == Color.Empty)
            {
                MessageBox.Show("�w�i�F���I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var previousFormTitle = Text;
            Text = FORM_TITLE + " - �I��������...";
            (int x, int y)[] values = GetSelectedArea(new Point(originalX, originalY), previewImage, backgroundColor);
            Text = previousFormTitle;

            if (values.Length == 0)
            {
                MessageBox.Show("�I���\�ȃG���A������܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (selectedPointsArray == null)
            {
                var newSelectedPointsArray = Array.Empty<(int x, int y)[]>();
                selectedPointsArray = newSelectedPointsArray.Append(values).ToArray();
            }
            else
            {
                if (selectedPointsArray.Any(points => points.Intersect(values).Any()))
                {
                    MessageBox.Show("�I���ς݂̃G���A���܂܂�Ă��܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedPointsArray = selectedPointsArray.Append(values).ToArray();
            }

            previousFormTitle = Text;
            Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
            selectedPointsArrayForPreview = selectedPointsArray.Select(points => ConvertSelectedAreaToPreviewBox(points, previewImage, previewBox)).ToArray();
            Text = previousFormTitle;

            var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
            Text = FORM_TITLE + $" - {selectedPointsArray.Length} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
            coloredPreviewBox.Image = GenerateColoredPreview(previewImage);
            return;
        }

        previousColorBox.BackColor = color;
        previousColor = color;
        previousRGBLabel.Text = $"RGB: ({color.R}, {color.G}, {color.B})";
        UpdateCalculatedRGBValue();

        clickedPoint = e.Location;
        previewBox.Invalidate();
        coloredPreviewBox.Invalidate();
    }

    // �v�Z���RGB���x���̒l�̍X�V
    private void UpdateCalculatedRGBValue()
    {
        if (previousColor == Color.Empty || newColor == Color.Empty)
        {
            calculatedRGBLabel.Text = "";
            return;
        }

        int diffR = newColor.R - previousColor.R;
        int diffG = newColor.G - previousColor.G;
        int diffB = newColor.B - previousColor.B;

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
            bmp = new Bitmap(path);
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

        filePath = path;

        if (previewBox.Image != null)
        {
            previewBox.Image.Dispose();
            previewBox.Image = null;
        }

        previewBox.Image = bmp;

        if (coloredPreviewBox.Image != null)
        {
            coloredPreviewBox.Image.Dispose();
            coloredPreviewBox.Image = null;
        }

        previousColor = Color.Empty;
        newColor = Color.Empty;
        backgroundColor = Color.Empty;

        previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
        backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;

        clickedPoint = Point.Empty;

        previousRGBLabel.Text = "";
        newRGBLabel.Text = "";
        calculatedRGBLabel.Text = "";

        selectedPointsArray = null;
        selectedPointsArrayForPreview = null;

        Text = FORM_TITLE;
        selectMode.Checked = false;
    }

    // �߂�{�^��(�I�����[�h)
    private void UndoButton_Click(object sender, EventArgs e)
    {
        if (bmp == null) return;
        if (selectedPointsArray == null || selectedPointsArray.Length == 0) return;
        selectedPointsArray = selectedPointsArray.Length == 1 ? null : selectedPointsArray[..^1];

        if (selectedPointsArray == null)
        {
            selectedPointsArrayForPreview = null;
            Text = FORM_TITLE;

            coloredPreviewBox.Image = GenerateColoredPreview(bmp);
            return;
        }

        selectedPointsArrayForPreview = selectedPointsArray.Select(points => ConvertSelectedAreaToPreviewBox(points, bmp, previewBox)).ToArray();

        var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
        if (selectedPointsArray.Length == 0)
        {
            Text = FORM_TITLE;
        }
        else
        {
            Text = FORM_TITLE + $" - {selectedPointsArray.Length} �̑I���G���A (���I���s�N�Z����: {totalSelectedPoints:N0})";
        }

        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // �I�����[�h�̗L�����A������
    private void SelectMode_CheckedChanged(object sender, EventArgs e)
    {
        if (selectMode.Checked && bmp == null)
        {
            MessageBox.Show("�摜���ǂݍ��܂�Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && (previousColor == Color.Empty || newColor == Color.Empty))
        {
            MessageBox.Show("�F���I������Ă��܂���B�i�v���r���[���쐬�ł��܂���j", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && backgroundColor == Color.Empty)
        {
            MessageBox.Show("�I�����[�h���L���ɂȂ�܂����B�͂��߂ɔw�i�F���E�N���b�N�Őݒ肵�Ă��������B", "���", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        backgroundColorBox.Enabled = selectMode.Checked;
        backgroundColorLabel.Enabled = selectMode.Checked;
        UndoButton.Enabled = selectMode.Checked;
    }

    // �o�����X���[�h�̗L�����A������
    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (bmp == null || previousColor == Color.Empty || newColor == Color.Empty) return;
        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // �d�݂̒l�̕ύX���̏���
    private void WeightText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && balanceMode.Checked)
        {
            if (double.TryParse(weightText.Text, out _))
            {
                if (bmp == null || previousColor == Color.Empty || newColor == Color.Empty) return;
                coloredPreviewBox.Image = GenerateColoredPreview(bmp);
            }
            else
            {
                MessageBox.Show("���l����͂��Ă��������B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                weightText.Text = "1.00";
            }
        }
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (InverseMode.Checked) MessageBox.Show("���]���[�h���I���ɂȂ�܂����B\n�I�����ꂽ�����̐F�͕ς�炸�A����ȊO�̏ꏊ�̐F�̂ݕς��܂��B\n���߉摜�쐬���[�h�ł́A���߂��镔�����I�𕔕��Ƌt�ɂȂ�܂��B\n", "���", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
