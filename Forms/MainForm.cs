using System.Linq.Expressions;
using Helper = VRC_Color_Changer.Classes.Helper;

namespace VRC_Color_Changer
{
    public partial class MainForm : Form
    {
        private const string CURRENT_VERSION = "v1.0.2";
        private const string FORM_TITLE = $"VRChat Color Changer {CURRENT_VERSION} by �Ղ����";

        private Color previousColor = Color.Empty;
        private Color newColor = Color.Empty;
        private Point clickedPoint = Point.Empty;
        private static readonly Color lightGray = Color.LightGray;
        private Color DEFAULT_BACKGROUND_COLOR = lightGray;

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
        private void openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
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
            if (files == null) return;
            var path = files[0];

            LoadPictureFile(path);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        // PreviewBox�̏���
        private void previewBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (bmp == null || newColor == Color.Empty || previousColor == Color.Empty) return;

            coloredPreviewBox.Image = GenerateColoredPreview(bmp);
        }

        private void previewBox_Paint(object sender, PaintEventArgs e)
        {
            if (clickedPoint == Point.Empty) return;

            Color inverseColor = Color.FromArgb(255 - previousColor.R, 255 - previousColor.G, 255 - previousColor.B);
            Pen pen = new(inverseColor, 2);

            e.Graphics.DrawLine(pen, clickedPoint.X - 5, clickedPoint.Y, clickedPoint.X + 5, clickedPoint.Y);
            e.Graphics.DrawLine(pen, clickedPoint.X, clickedPoint.Y - 5, clickedPoint.X, clickedPoint.Y + 5);
        }

        private void coloredPreviewBox_Paint(object sender, PaintEventArgs e)
        {
            if (clickedPoint == Point.Empty) return;

            Color inverseColor = Color.FromArgb(255 - newColor.R, 255 - newColor.G, 255 - newColor.B);
            Pen pen = new Pen(inverseColor, 2);

            e.Graphics.DrawLine(pen, clickedPoint.X - 5, clickedPoint.Y, clickedPoint.X + 5, clickedPoint.Y);
            e.Graphics.DrawLine(pen, clickedPoint.X, clickedPoint.Y - 5, clickedPoint.X, clickedPoint.Y + 5);
        }

        // �F�̑I��
        private void newColorBox_MouseDown(object sender, MouseEventArgs e)
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

            ColorPicker colorPicker = new ColorPicker(newColor);
            colorPicker.ShowDialog();
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

            var result = MessageBox.Show("�摜���쐬���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.Cancel) return;

            // ���O�����ĕۑ�
            var fileDirectory = Path.GetDirectoryName(filePath);

            var newFilePath = "";
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PNG�t�@�C��|*.png;",
                Title = "�V�K�e�N�X�`���摜�̕ۑ����I�����Ă�������",
                FileName = "new_" + Path.GetFileName(filePath),
                InitialDirectory = fileDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                newFilePath = dialog.FileName;
            }

            if (newFilePath == "")
            {
                MessageBox.Show("�t�@�C���̕ۑ��悪�I������Ă��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MakeButton.Text = "�쐬��...";
            MakeButton.Enabled = false;
            ChangeColor(previousColor, newColor, newFilePath);
        }

        // �w���v�{�^��
        private void helpUseButton_Click(object sender, EventArgs e)
        {
            string message = "���̃\�t�g�̎g����: \n" +
                "1. �摜����ʓ��Ƀh���b�O&�h���b�v�A��������\"�t�@�C�����J��\"�������ĉ摜���J���Ă��������B\n" +
                "2. �ύX�O�̐F���摜�����N���b�N������h���b�O���Č��߂Ă��������B\n" +
                "3. �ύX��̐F����ʉ��̃O���[�̘g���N���b�N���đI�����Ă��������B\n" +
                "4. �쐬�{�^���������ƁA�e�N�X�`���̐V�K�쐬���J�n����܂��B";

            message += "\n\n�I�����[�h�̎g����: \n" +
                "1. �w�i�F���E�N���b�N�Őݒ肵�Ă��������B\n" +
                "2. �ύX�������������N���b�N�őI�����Ă�������(�E�ɐԂ��g���Ńv���r���[���o�܂�)\n" +
                "3. �I�����I�������A�I�����[�h�̃`�F�b�N���O���Ă��������B";

            MessageBox.Show(message, "�w���v", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ��������
        private void ChangeColor(Color previousColor, Color newColor, string filePath)
        {
            try
            {
                if (bmp == null) return;
                if (previousColor == Color.Empty || newColor == Color.Empty) return;

                var bitMap = new Bitmap(bmp);

                int diffR = newColor.R - previousColor.R;
                int diffG = newColor.G - previousColor.G;
                int diffB = newColor.B - previousColor.B;

                // Bitmap �����b�N���ăs�N�Z���f�[�^�ɒ��ڃA�N�Z�X
                var rect = new Rectangle(0, 0, bitMap.Width, bitMap.Height);
                var data = bitMap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    if (selectedPointsArray != null)
                    {
                        foreach (var selectedPoints in selectedPointsArray)
                        {
                            foreach (var (x, y) in selectedPoints)
                            {
                                // �s�N�Z���f�[�^�̃C���f�b�N�X���v�Z
                                int index = (y * data.Stride) + (x * 4);

                                // A �`�����l���� 0 �Ȃ�X�L�b�v
                                if (ptr[index + 3] == 0) continue;

                                // RGB �l��ύX
                                int r = ptr[index + 2] + diffR; // ��
                                int g = ptr[index + 1] + diffG; // ��
                                int b = ptr[index + 0] + diffB; // ��

                                // �l�� 0 �` 255 �ɃN�����v
                                r = Math.Max(0, Math.Min(255, r));
                                g = Math.Max(0, Math.Min(255, g));
                                b = Math.Max(0, Math.Min(255, b));

                                // �V���� RGB �l��ݒ�
                                ptr[index + 2] = (byte)r;
                                ptr[index + 1] = (byte)g;
                                ptr[index + 0] = (byte)b;
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < bitMap.Height; y++)
                        {
                            for (int x = 0; x < bitMap.Width; x++)
                            {
                                // �s�N�Z���f�[�^�̃C���f�b�N�X���v�Z
                                int index = (y * data.Stride) + (x * 4);

                                // A �`�����l���� 0 �Ȃ�X�L�b�v
                                if (ptr[index + 3] == 0) continue;

                                // RGB �l��ύX
                                int r = ptr[index + 2] + diffR; // ��
                                int g = ptr[index + 1] + diffG; // ��
                                int b = ptr[index + 0] + diffB; // ��

                                // �l�� 0 �` 255 �ɃN�����v
                                r = Math.Max(0, Math.Min(255, r));
                                g = Math.Max(0, Math.Min(255, g));
                                b = Math.Max(0, Math.Min(255, b));

                                // �V���� RGB �l��ݒ�
                                ptr[index + 2] = (byte)r;
                                ptr[index + 1] = (byte)g;
                                ptr[index + 0] = (byte)b;
                            }
                        }
                    }
                }

                // �r�b�g�}�b�v�̃��b�N������
                bitMap.UnlockBits(data);
                bitMap.Save(filePath);

                MessageBox.Show("�e�N�X�`���摜�̍쐬���������܂����B"
                    , "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("�G���[���������܂����B\n" + e.Message, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MakeButton.Enabled = true;
                MakeButton.Text = "�쐬";
            }
        }

        // �v���r���[�摜�̐���
        private Bitmap GenerateColoredPreview(Bitmap rawBitmap)
        {
            int boxHeight = coloredPreviewBox.Height;
            int boxWidth = coloredPreviewBox.Width;

            float ratioX = (float)rawBitmap.Width / boxWidth;
            float ratioY = (float)rawBitmap.Height / boxHeight;

            Bitmap _previewBitmap = new Bitmap(boxWidth, boxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int diffR = newColor.R - previousColor.R;
            int diffG = newColor.G - previousColor.G;
            int diffB = newColor.B - previousColor.B;

            // ���̃r�b�g�}�b�v�����b�N
            var rawRect = new Rectangle(0, 0, rawBitmap.Width, rawBitmap.Height);
            var rawData = rawBitmap.LockBits(rawRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // �v���r���[�p�̃r�b�g�}�b�v�����b�N
            var previewRect = new Rectangle(0, 0, _previewBitmap.Width, _previewBitmap.Height);
            var previewData = _previewBitmap.LockBits(previewRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* rawPtr = (byte*)rawData.Scan0;
                byte* previewPtr = (byte*)previewData.Scan0;

                for (int y = 0; y < boxHeight; y++)
                {
                    for (int x = 0; x < boxWidth; x++)
                    {
                        // ���̃r�b�g�}�b�v�̑Ή�������W���v�Z
                        int rawX = (int)(x * ratioX);
                        int rawY = (int)(y * ratioY);

                        int rawIndex = (rawY * rawData.Stride) + (rawX * 4);

                        // �s�N�Z���f�[�^���擾
                        byte b = rawPtr[rawIndex + 0];
                        byte g = rawPtr[rawIndex + 1];
                        byte r = rawPtr[rawIndex + 2];
                        byte a = rawPtr[rawIndex + 3];

                        if (a == 0) continue; // �����ȃs�N�Z���͂��̂܂܃X�L�b�v

                        // RGBA �l��ύX
                        int newR = r + diffR;
                        int newG = g + diffG;
                        int newB = b + diffB;
                        var newA = a;

                        // �l�� 0 �` 255 �ɃN�����v
                        newR = Math.Max(0, Math.Min(255, newR));
                        newG = Math.Max(0, Math.Min(255, newG));
                        newB = Math.Max(0, Math.Min(255, newB));

                        // �w�i�����͗΂ɕύX����B
                        if (backgroundColor != Color.Empty && backgroundColor.R == r && backgroundColor.G == g && backgroundColor.B == b)
                        {
                            newR = 0;
                            newG = 255;
                            newB = 0;
                            newA = 255;
                        }

                        // �v���r���[�p�r�b�g�}�b�v�ɏ�������
                        int previewIndex = (y * previewData.Stride) + (x * 4);
                        previewPtr[previewIndex + 0] = (byte)newB;
                        previewPtr[previewIndex + 1] = (byte)newG;
                        previewPtr[previewIndex + 2] = (byte)newR;
                        previewPtr[previewIndex + 3] = newA;
                    }
                }

                if (selectedPointsArrayForPreview != null)
                {
                    foreach (var selectedPoints in selectedPointsArrayForPreview)
                    {
                        foreach (var (x, y) in selectedPoints)
                        {
                            int previewIndex = (y * previewData.Stride) + (x * 4);
                            previewPtr[previewIndex + 0] = 0;
                            previewPtr[previewIndex + 1] = 0;
                            previewPtr[previewIndex + 2] = 255;
                            previewPtr[previewIndex + 3] = 255;
                        }
                    }
                }
            }

            // �r�b�g�}�b�v�̃��b�N������
            rawBitmap.UnlockBits(rawData);
            _previewBitmap.UnlockBits(previewData);

            return _previewBitmap;
        }

        // �ύX�O�̐F�̑I�� (�I�����[�h�̏��������̒��ɓ����Ă���)
        private void SetPreviousColor(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;

            int x = e.X;
            int y = e.Y;

            // ���̉摜�̃T�C�Y���擾
            Bitmap bmp = (Bitmap)previewBox.Image;
            if (bmp == null) return;

            int originalWidth = bmp.Width;
            int originalHeight = bmp.Height;

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
            Color color = bmp.GetPixel(originalX, originalY);

            if (selectMode.Checked)
            {
                if (e.Button == MouseButtons.Right)
                {
                    backgroundColor = color;
                    backgroundColorBox.BackColor = color;
                    coloredPreviewBox.Image = GenerateColoredPreview(bmp);
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
                (int x, int y)[] values = Helper.GetSelectedArea(new Point(originalX, originalY), bmp, backgroundColor);
                Text = previousFormTitle;

                if (values.Length == 0)
                {
                    MessageBox.Show("�I���\�ȃG���A������܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (selectedPointsArray == null)
                {
                    selectedPointsArray = [values];
                }
                else
                {
                    foreach (var points in selectedPointsArray)
                    {
                        if (points.Intersect(values).Any())
                        {
                            MessageBox.Show("�I���ς݂̃G���A���܂܂�Ă��܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    selectedPointsArray = selectedPointsArray.Append(values).ToArray();
                }

                previousFormTitle = Text;
                Text = FORM_TITLE + " - �v���r���[�p�̑I���G���A�쐬��...";
                selectedPointsArrayForPreview = selectedPointsArray.Select(points => Helper.ConvertSelectedAreaToPreviewBox(points, bmp, previewBox)).ToArray();
                Text = previousFormTitle;

                var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
                Text = FORM_TITLE + $" - {selectedPointsArray.Length} �̑I���G���A (�����\��s�N�Z����: {totalSelectedPoints.ToString("N0")})";
                coloredPreviewBox.Image = GenerateColoredPreview(bmp);
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
            catch
            {
                MessageBox.Show("�摜�̓ǂݍ��݂Ɏ��s���܂����B�����炭��Ή��̃t�@�C���ł��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            clickedPoint = Point.Empty;
            previousRGBLabel.Text = "";
            newRGBLabel.Text = "";
            calculatedRGBLabel.Text = "";
            backgroundColor = Color.Empty;
            previousColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
            newColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;
            backgroundColorBox.BackColor = DEFAULT_BACKGROUND_COLOR;

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

            selectedPointsArrayForPreview = selectedPointsArray.Select(points => Helper.ConvertSelectedAreaToPreviewBox(points, bmp, previewBox)).ToArray();

            var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
            if (selectedPointsArray.Length == 0)
            {
                Text = FORM_TITLE;
            }
            else
            {
                Text = FORM_TITLE + $" - {selectedPointsArray.Length} �̑I���G���A (�����\��s�N�Z����: {totalSelectedPoints.ToString("N0")})";
            }

            coloredPreviewBox.Image = GenerateColoredPreview(bmp);
        }

        // �I�����[�h�̗L�����A������
        private void selectMode_CheckedChanged(object sender, EventArgs e)
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
    }
}
