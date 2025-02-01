using System.Linq.Expressions;
using Helper = VRC_Color_Changer.Classes.Helper;

namespace VRC_Color_Changer
{
    public partial class MainForm : Form
    {
        private const string CURRENT_VERSION = "v1.0.2";
        private const string FORM_TITLE = $"VRChat Color Changer {CURRENT_VERSION} by ぷこるふ";

        private Color previousColor = Color.Empty;
        private Color newColor = Color.Empty;
        private Point clickedPoint = Point.Empty;
        private static readonly Color lightGray = Color.LightGray;
        private Color DEFAULT_BACKGROUND_COLOR = lightGray;

        private Bitmap? bmp;
        private string? filePath;

        // 選択モード
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

        // ファイルを開く
        private void openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;",
                Title = "画像ファイルを選択してください"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;
            LoadPictureFile(dialog.FileName);
        }

        // ファイルのドラッグ&ドロップ
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (files == null) return;
            var path = files[0];

            LoadPictureFile(path);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        // PreviewBoxの処理
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

        // 色の選択
        private void newColorBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (bmp == null)
            {
                MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (previousColor == Color.Empty)
            {
                MessageBox.Show("変更前の色が選択されていません。(プレビューが作成できません)", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // 作成ボタン
        private void MakeButton_Click(object sender, EventArgs e)
        {
            if (bmp == null)
            {
                MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (previousColor == Color.Empty || newColor == Color.Empty)
            {
                MessageBox.Show("色が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("画像を作成しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.Cancel) return;

            // 名前をつけて保存
            var fileDirectory = Path.GetDirectoryName(filePath);

            var newFilePath = "";
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PNGファイル|*.png;",
                Title = "新規テクスチャ画像の保存先を選択してください",
                FileName = "new_" + Path.GetFileName(filePath),
                InitialDirectory = fileDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                newFilePath = dialog.FileName;
            }

            if (newFilePath == "")
            {
                MessageBox.Show("ファイルの保存先が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MakeButton.Text = "作成中...";
            MakeButton.Enabled = false;
            ChangeColor(previousColor, newColor, newFilePath);
        }

        // ヘルプボタン
        private void helpUseButton_Click(object sender, EventArgs e)
        {
            string message = "このソフトの使い方: \n" +
                "1. 画像を画面内にドラッグ&ドロップ、もしくは\"ファイルを開く\"を押して画像を開いてください。\n" +
                "2. 変更前の色を画像内をクリックしたりドラッグして決めてください。\n" +
                "3. 変更後の色を画面下のグレーの枠をクリックして選択してください。\n" +
                "4. 作成ボタンを押すと、テクスチャの新規作成が開始されます。";

            message += "\n\n選択モードの使い方: \n" +
                "1. 背景色を右クリックで設定してください。\n" +
                "2. 変更したい部分をクリックで選択してください(右に赤い枠線でプレビューが出ます)\n" +
                "3. 選択が終わったら、選択モードのチェックを外してください。";

            MessageBox.Show(message, "ヘルプ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 処理部分
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

                // Bitmap をロックしてピクセルデータに直接アクセス
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
                                // ピクセルデータのインデックスを計算
                                int index = (y * data.Stride) + (x * 4);

                                // A チャンネルが 0 ならスキップ
                                if (ptr[index + 3] == 0) continue;

                                // RGB 値を変更
                                int r = ptr[index + 2] + diffR; // 赤
                                int g = ptr[index + 1] + diffG; // 緑
                                int b = ptr[index + 0] + diffB; // 青

                                // 値を 0 ～ 255 にクランプ
                                r = Math.Max(0, Math.Min(255, r));
                                g = Math.Max(0, Math.Min(255, g));
                                b = Math.Max(0, Math.Min(255, b));

                                // 新しい RGB 値を設定
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
                                // ピクセルデータのインデックスを計算
                                int index = (y * data.Stride) + (x * 4);

                                // A チャンネルが 0 ならスキップ
                                if (ptr[index + 3] == 0) continue;

                                // RGB 値を変更
                                int r = ptr[index + 2] + diffR; // 赤
                                int g = ptr[index + 1] + diffG; // 緑
                                int b = ptr[index + 0] + diffB; // 青

                                // 値を 0 ～ 255 にクランプ
                                r = Math.Max(0, Math.Min(255, r));
                                g = Math.Max(0, Math.Min(255, g));
                                b = Math.Max(0, Math.Min(255, b));

                                // 新しい RGB 値を設定
                                ptr[index + 2] = (byte)r;
                                ptr[index + 1] = (byte)g;
                                ptr[index + 0] = (byte)b;
                            }
                        }
                    }
                }

                // ビットマップのロックを解除
                bitMap.UnlockBits(data);
                bitMap.Save(filePath);

                MessageBox.Show("テクスチャ画像の作成が完了しました。"
                    , "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MakeButton.Enabled = true;
                MakeButton.Text = "作成";
            }
        }

        // プレビュー画像の生成
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

            // 元のビットマップをロック
            var rawRect = new Rectangle(0, 0, rawBitmap.Width, rawBitmap.Height);
            var rawData = rawBitmap.LockBits(rawRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // プレビュー用のビットマップをロック
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
                        // 元のビットマップの対応する座標を計算
                        int rawX = (int)(x * ratioX);
                        int rawY = (int)(y * ratioY);

                        int rawIndex = (rawY * rawData.Stride) + (rawX * 4);

                        // ピクセルデータを取得
                        byte b = rawPtr[rawIndex + 0];
                        byte g = rawPtr[rawIndex + 1];
                        byte r = rawPtr[rawIndex + 2];
                        byte a = rawPtr[rawIndex + 3];

                        if (a == 0) continue; // 透明なピクセルはそのままスキップ

                        // RGBA 値を変更
                        int newR = r + diffR;
                        int newG = g + diffG;
                        int newB = b + diffB;
                        var newA = a;

                        // 値を 0 ～ 255 にクランプ
                        newR = Math.Max(0, Math.Min(255, newR));
                        newG = Math.Max(0, Math.Min(255, newG));
                        newB = Math.Max(0, Math.Min(255, newB));

                        // 背景部分は緑に変更する。
                        if (backgroundColor != Color.Empty && backgroundColor.R == r && backgroundColor.G == g && backgroundColor.B == b)
                        {
                            newR = 0;
                            newG = 255;
                            newB = 0;
                            newA = 255;
                        }

                        // プレビュー用ビットマップに書き込み
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

            // ビットマップのロックを解除
            rawBitmap.UnlockBits(rawData);
            _previewBitmap.UnlockBits(previewData);

            return _previewBitmap;
        }

        // 変更前の色の選択 (選択モードの処理もこの中に入っている)
        private void SetPreviousColor(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;

            int x = e.X;
            int y = e.Y;

            // 元の画像のサイズを取得
            Bitmap bmp = (Bitmap)previewBox.Image;
            if (bmp == null) return;

            int originalWidth = bmp.Width;
            int originalHeight = bmp.Height;

            // PictureBox の表示サイズを取得
            int pictureBoxWidth = previewBox.Width;
            int pictureBoxHeight = previewBox.Height;

            // PictureBox のサイズと元の画像のサイズの比率を計算
            float ratioX = (float)originalWidth / pictureBoxWidth;
            float ratioY = (float)originalHeight / pictureBoxHeight;

            // クリックした座標を元の画像の座標に変換
            int originalX = (int)(x * ratioX);
            int originalY = (int)(y * ratioY);

            // 座標が画像の範囲外なら何もしない
            if (originalX < 0 || originalX >= originalWidth || originalY < 0 || originalY >= originalHeight) return;

            // 変換した座標で元の画像のピクセルカラーを取得
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
                    MessageBox.Show("色が選択されていません。（プレビューが作成できません）", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (backgroundColor == Color.Empty)
                {
                    MessageBox.Show("背景色が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var previousFormTitle = Text;
                Text = FORM_TITLE + " - 選択処理中...";
                (int x, int y)[] values = Helper.GetSelectedArea(new Point(originalX, originalY), bmp, backgroundColor);
                Text = previousFormTitle;

                if (values.Length == 0)
                {
                    MessageBox.Show("選択可能なエリアがありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            MessageBox.Show("選択済みのエリアが含まれています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    selectedPointsArray = selectedPointsArray.Append(values).ToArray();
                }

                previousFormTitle = Text;
                Text = FORM_TITLE + " - プレビュー用の選択エリア作成中...";
                selectedPointsArrayForPreview = selectedPointsArray.Select(points => Helper.ConvertSelectedAreaToPreviewBox(points, bmp, previewBox)).ToArray();
                Text = previousFormTitle;

                var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
                Text = FORM_TITLE + $" - {selectedPointsArray.Length} 個の選択エリア (処理予定ピクセル数: {totalSelectedPoints.ToString("N0")})";
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

        // 計算後のRGBラベルの値の更新
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

            calculatedRGBLabel.Text = $"計算後のRGB: ({diffRStr}, {diffGStr}, {diffBStr})";
        }

        // 画像の読み込み
        private void LoadPictureFile(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("ファイルが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                bmp = new Bitmap(path);
            }
            catch
            {
                MessageBox.Show("画像の読み込みに失敗しました。おそらく非対応のファイルです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // 戻るボタン(選択モード)
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
                Text = FORM_TITLE + $" - {selectedPointsArray.Length} 個の選択エリア (処理予定ピクセル数: {totalSelectedPoints.ToString("N0")})";
            }

            coloredPreviewBox.Image = GenerateColoredPreview(bmp);
        }

        // 選択モードの有効化、無効化
        private void selectMode_CheckedChanged(object sender, EventArgs e)
        {
            if (selectMode.Checked && bmp == null)
            {
                MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                selectMode.Checked = false;
                return;
            }

            if (selectMode.Checked && (previousColor == Color.Empty || newColor == Color.Empty))
            {
                MessageBox.Show("色が選択されていません。（プレビューが作成できません）", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                selectMode.Checked = false;
                return;
            }

            if (selectMode.Checked && backgroundColor == Color.Empty)
            {
                MessageBox.Show("選択モードが有効になりました。はじめに背景色を右クリックで設定してください。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            backgroundColorBox.Enabled = selectMode.Checked;
            backgroundColorLabel.Enabled = selectMode.Checked;
            UndoButton.Enabled = selectMode.Checked;
        }
    }
}
