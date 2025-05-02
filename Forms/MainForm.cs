using System.Drawing.Imaging;
using static VRC_Color_Changer.Classes.Helper;

namespace VRC_Color_Changer;

public partial class MainForm : Form
{
    private const string CURRENT_VERSION = "v1.0.8";
    private const string FORM_TITLE = $"VRChat Color Changer {CURRENT_VERSION} by ぷこるふ";

    private Color previousColor = Color.Empty;
    private Color newColor = Color.Empty;
    private Point clickedPoint = Point.Empty;

    private static readonly Color lightGray = Color.LightGray;
    private readonly Color DEFAULT_BACKGROUND_COLOR = lightGray;

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

    // ファイルのドラッグ&ドロップ
    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
        if (files == null || files.Length == 0) return;

        LoadPictureFile(files[0]);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

    // PreviewBoxの処理
    private void PreviewBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (bmp == null || newColor == Color.Empty || previousColor == Color.Empty) return;

        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // PreviewBoxの描画
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

    // 色の選択
    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
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

        var result = MessageBox.Show("画像を作成しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes) return;

        var originalFileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        var originalExtension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        var newFileName = originalFileName + "_new" + originalExtension;

        // 名前をつけて保存
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNGファイル|*.png;",
            Title = "新規テクスチャ画像の保存先を選択してください",
            FileName = newFileName,
            InitialDirectory = Path.GetDirectoryName(filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        var newFilePath = dialog.FileName;

        if (newFilePath == "")
        {
            MessageBox.Show("ファイルの保存先が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MakeButton.Text = "作成中...";
        MakeButton.Enabled = false;
        ChangeColor(previousColor, newColor, newFilePath);
    }

    // 使い方ボタン
    private void HelpUseButton_Click(object sender, EventArgs e)
    {
        string message = "このソフトの基本的な使い方:\n" +
            "1. 画像を画面内にドラッグ＆ドロップするか、「ファイルを開く」ボタンを押して画像を読み込んでください。\n" +
            "2. 変更前の色は、画像内をクリックまたはドラッグして選択します。\n" +
            "3. 変更後の色は、画面下部のグレーの枠をクリックして選択してください。\n" +
            "4. 「作成」ボタンを押すと、テクスチャの作成が始まります。";

        message += "\n\n選択モードの使い方:\n" +
            "選択モードでは、出力時に選択された部分のみ色が変更され、他の場所は変更されません。\n" +
            "1. 背景色を右クリックで設定します。\n" +
            "2. 変更したい部分を左クリックで選択してください（右側の画像に赤い枠線でプレビューされます。複数選択も可能です）。\n" +
            "3. 選択が完了したら、「選択モード」のチェックを外してください。\n" +
            "※「戻る」ボタンを押すと、最後に選択したエリアから順に選択を解除できます。";

        message += "\n\n反転モードについて:\n" +
            "選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n" +
            "透過画像作成モードでは、透過する部分が選択部分と逆になります。\n" +
            "耳毛など、変えたくない部分が少ない場合に有効的です。\n" +
            "選択モードのみだと、変えたくない部分以外を全て選択した状態にしないといけないので便利です。";

        message += "\n\n透過画像作成モードの使い方:\n" +
            "透過画像作成モードでは、選択した部分だけが残る透過画像を生成します。\n" +
            "1. 選択モードで、透過させたくない部分を選択します（複数選択可）。\n" +
            "2. 「透過モード」にチェックを入れてください。\n" +
            "3. 「作成」ボタンを押すと、選択された部分だけが残る透過画像が生成されます。";

        message += "\n\nバランスモードについて:\n" +
            "選択した色に近いほど強く、遠いほど弱く色を変更します。\n" +
            "オン／オフを切り替えて、どちらがより自然か確認してから色変換を実行することをおすすめします。\n" +
            "※上手く色が変わらない場合は、「重み」の値を調整してみてください。色変更率グラフ（非線形）のカーブの鋭さが変わります。\n" +
            "※変えたくない部分が変わってしまう場合は値を大きく、もっと変えたい場合は小さくしてください。0にすると通常モードと同じになります。";

        MessageBox.Show(message, "ソフトの使い方一覧", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // 処理部分
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

            // 透過用のビットマップを作成
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
                    if (sourcePtr[pixelIndex + 3] == 0) return; // A チャンネルが 0 ならスキップ

                    int r = sourcePtr[pixelIndex + 2]; // 赤
                    int g = sourcePtr[pixelIndex + 1]; // 緑
                    int b = sourcePtr[pixelIndex + 0]; // 青

                    if (balanceMode.Checked)
                    {
                        var (hasIntersection, IntersectionDistance) = GetRGBIntersectionDistance(previousColor.R, previousColor.G, previousColor.B, r, g, b);

                        //空間上での距離を計算
                        double distance = Math.Sqrt(
                            Math.Pow(r - previousColor.R, 2) +
                            Math.Pow(g - previousColor.G, 2) +
                            Math.Pow(b - previousColor.B, 2)
                        );

                        // 変化率
                        double adjustmentFactor = CalculateColorChangeRate(hasIntersection, IntersectionDistance, distance, weight);

                        // RGBの差分を補正
                        int adjustedDiffR = (int)(diffR * adjustmentFactor);
                        int adjustedDiffG = (int)(diffG * adjustmentFactor);
                        int adjustedDiffB = (int)(diffB * adjustmentFactor);

                        // 補正後のRGB値を計算
                        r = Math.Clamp(r + adjustedDiffR, 0, 255); // 赤
                        g = Math.Clamp(g + adjustedDiffG, 0, 255); // 緑
                        b = Math.Clamp(b + adjustedDiffB, 0, 255); // 青
                    }
                    else
                    {
                        r = Math.Clamp(r + diffR, 0, 255); // 赤
                        g = Math.Clamp(g + diffG, 0, 255); // 緑
                        b = Math.Clamp(b + diffB, 0, 255); // 青
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
                    if (targetPtr[pixelIndex + 3] == 0) return; // A チャンネルが 0 ならスキップ

                    targetPtr[pixelIndex + 2] = rawPtr[pixelIndex + 2];
                    targetPtr[pixelIndex + 1] = rawPtr[pixelIndex + 1];
                    targetPtr[pixelIndex + 0] = rawPtr[pixelIndex + 0];
                }

                void ProcessTransPixel(int x, int y, byte* targetPtr)
                {
                    int pixelIndex = (y * data.Stride) + (x * 4);
                    if (targetPtr[pixelIndex + 3] == 0) return; // A チャンネルが 0 ならスキップ

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
                        MessageBox.Show("元画像のデータの取得に失敗しました。選択反転モードの結果は作成されません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show("透過画像用データの取得に失敗しました。デフォルトの画像が使用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show("選択エリアがなかったため、透過モードはスキップされます。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            // ビットマップのロックを解除
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
                    MessageBox.Show("透過用画像が作成できませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            MessageBox.Show("テクスチャ画像の作成が完了しました。"
                , "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception e)
        {
            MessageBox.Show("テクスチャ画像作成中にエラーが発生しました。\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            MakeButton.Enabled = true;
            MakeButton.Text = "作成";
        }
    }

    // プレビュー画像の生成
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

        // 元のビットマップをロック
        var sourceRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
        var sourceBitmapData = sourceBitmap.LockBits(sourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        // プレビュー用のビットマップをロック
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
                    // 元のビットマップの対応する座標を計算
                    int sourceX = (int)(x * ratioX);
                    int sourceY = (int)(y * ratioY);

                    int pixelIndex = (sourceY * sourceBitmapData.Stride) + (sourceX * 4);

                    // ピクセルデータを取得
                    int b = sourcePtr[pixelIndex + 0];
                    int g = sourcePtr[pixelIndex + 1];
                    int r = sourcePtr[pixelIndex + 2];
                    int a = sourcePtr[pixelIndex + 3];

                    if (a == 0) continue; // 透明なピクセルはそのままスキップ

                    int newR = r;
                    int newG = g;
                    int newB = b;

                    if (balanceMode.Checked)
                    {
                        var (hasIntersection, IntersectionDistance) = GetRGBIntersectionDistance(previousColor.R, previousColor.G, previousColor.B, newR, newG, newB);

                        //空間上での距離を計算
                        double distance = Math.Sqrt(
                            Math.Pow(r - previousColor.R, 2) +
                            Math.Pow(g - previousColor.G, 2) +
                            Math.Pow(b - previousColor.B, 2)
                        );

                        // 変化率
                        double adjustmentFactor = CalculateColorChangeRate(hasIntersection, IntersectionDistance, distance, weight);

                        // RGBの差分を補正
                        int adjustedDiffR = (int)(diffR * adjustmentFactor);
                        int adjustedDiffG = (int)(diffG * adjustmentFactor);
                        int adjustedDiffB = (int)(diffB * adjustmentFactor);

                        // 補正後のRGB値を計算
                        newR = Math.Clamp(r + adjustedDiffR, 0, 255); // 赤
                        newG = Math.Clamp(g + adjustedDiffG, 0, 255); // 緑
                        newB = Math.Clamp(b + adjustedDiffB, 0, 255); // 青
                    }
                    else
                    {
                        // RGB 値を変更
                        newR = Math.Clamp(r + diffR, 0, 255);
                        newG = Math.Clamp(g + diffG, 0, 255);
                        newB = Math.Clamp(b + diffB, 0, 255);
                    }

                    // プレビュー用ビットマップに書き込み
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

        // ビットマップのロックを解除
        sourceBitmap.UnlockBits(sourceBitmapData);
        _previewBitmap.UnlockBits(previewBitmapData);

        return _previewBitmap;
    }

    // 変更前の色の選択 (選択モードの処理もこの中に入っている)
    private void SetPreviousColor(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left && !(e.Button == MouseButtons.Right && selectMode.Checked)) return;

        int x = e.X;
        int y = e.Y;

        // 元の画像のサイズを取得
        Bitmap previewImage = (Bitmap)previewBox.Image;
        if (previewImage == null) return;

        int originalWidth = previewImage.Width;
        int originalHeight = previewImage.Height;

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
            (int x, int y)[] values = GetSelectedArea(new Point(originalX, originalY), previewImage, backgroundColor);
            Text = previousFormTitle;

            if (values.Length == 0)
            {
                MessageBox.Show("選択可能なエリアがありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("選択済みのエリアが含まれています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedPointsArray = selectedPointsArray.Append(values).ToArray();
            }

            previousFormTitle = Text;
            Text = FORM_TITLE + " - プレビュー用の選択エリア作成中...";
            selectedPointsArrayForPreview = selectedPointsArray.Select(points => ConvertSelectedAreaToPreviewBox(points, previewImage, previewBox)).ToArray();
            Text = previousFormTitle;

            var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
            Text = FORM_TITLE + $" - {selectedPointsArray.Length} 個の選択エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
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
        catch (Exception exception)
        {
            if (exception is ArgumentException)
            {
                MessageBox.Show("画像の読み込みに失敗しました。非対応のファイルです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("画像の読み込みに失敗しました。\n\nエラー: " + exception, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        selectedPointsArrayForPreview = selectedPointsArray.Select(points => ConvertSelectedAreaToPreviewBox(points, bmp, previewBox)).ToArray();

        var totalSelectedPoints = selectedPointsArray.Sum(points => points.Length);
        if (selectedPointsArray.Length == 0)
        {
            Text = FORM_TITLE;
        }
        else
        {
            Text = FORM_TITLE + $" - {selectedPointsArray.Length} 個の選択エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
        }

        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // 選択モードの有効化、無効化
    private void SelectMode_CheckedChanged(object sender, EventArgs e)
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

    // バランスモードの有効化、無効化
    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (bmp == null || previousColor == Color.Empty || newColor == Color.Empty) return;
        coloredPreviewBox.Image = GenerateColoredPreview(bmp);
    }

    // 重みの値の変更時の処理
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
                MessageBox.Show("数値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                weightText.Text = "1.00";
            }
        }
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (InverseMode.Checked) MessageBox.Show("反転モードがオンになりました。\n選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n透過画像作成モードでは、透過する部分が選択部分と逆になります。\n", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
