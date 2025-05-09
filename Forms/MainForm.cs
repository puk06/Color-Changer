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

    // 選択モード
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
        if (_bmp == null || _newColor == Color.Empty || _previousColor == Color.Empty) return;

        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    // PreviewBoxの描画
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

    // 色の選択
    private void NewColorBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (_bmp == null)
        {
            MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_previousColor == Color.Empty)
        {
            MessageBox.Show("変更前の色が選択されていません。(プレビューが作成できません)", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    // 作成ボタン
    private void MakeButton_Click(object sender, EventArgs e)
    {
        if (_bmp == null)
        {
            MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_previousColor == Color.Empty || _newColor == Color.Empty)
        {
            MessageBox.Show("色が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var result = MessageBox.Show("画像を作成しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes) return;

        var originalFileName = Path.GetFileNameWithoutExtension(_filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        var originalExtension = Path.GetExtension(_filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        var newFileName = originalFileName + "_new" + originalExtension;

        // 名前をつけて保存
        SaveFileDialog dialog = new SaveFileDialog()
        {
            Filter = "PNGファイル|*.png;",
            Title = "新規テクスチャ画像の保存先を選択してください",
            FileName = newFileName,
            InitialDirectory = Path.GetDirectoryName(_filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
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
        ChangeColor(_previousColor, _newColor, newFilePath);
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
            "- 選択モードでは、出力時に選択された部分のみ色が変更され、他の場所は変更されません。\n" +
            "1. 背景色を右クリックで設定します。\n" +
            "2. 変更したい部分を左クリックで選択してください（右側の画像に赤い枠線でプレビューされます。複数選択も可能です）。\n" +
            "3. 選択が完了したら、「選択モード」のチェックを外してください。\n" +
            "※「戻る」ボタンを押すと、最後に選択したエリアから順に選択を解除できます。";

        message += "\n\n選択反転モードについて:\n" +
            "- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n" +
            "- 透過画像作成モードでは、透過する部分が選択部分と逆になります。\n" +
            "耳毛など、変えたくない部分が少ない場合に有効的です。\n" +
            "選択モードのみだと、変えたくない部分以外を全て選択した状態にしないといけないので便利です。";

        message += "\n\n透過画像作成モードの使い方:\n" +
            "- 透過画像作成モードでは、選択した部分だけが残る透過画像を生成します。\n" +
            "1. 選択モードで、透過させたくない部分を選択します（複数選択可）。\n" +
            "2. 「透過モード」にチェックを入れてください。\n" +
            "3. 「作成」ボタンを押すと、選択された部分だけが残る透過画像が生成されます。";

        message += "\n\nバランスモードについて:\n" +
            "- 選択した色に近いほど強く、遠いほど弱く色を変更します。\n" +
            "オン／オフを切り替えて、どちらがより自然か確認してから色変換を実行することをおすすめします。設定から値などを調節することができます。\n" +
            "※上手く色が変わらない場合は、設定内の値を調整してみてください。\n" +
            "※「重り」は変えたくない部分が変わってしまう場合は値を大きく、もっと変えたい場合は小さくしてください。0にすると通常モードと同じになります。";

        MessageBox.Show(message, "ソフトの使い方一覧", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // 処理部分
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

            // 透過用のビットマップを作成
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
                        MessageBox.Show("選択エリアがなかったため、透過モードはスキップされます。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        if (transPixels == default) MessageBox.Show("透過画像用データの取得に失敗しました。デフォルトの画像が使用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        imageProcessor.ProcessTransparentSelectedPixels(sourcePixels, transPixels, _selectedPointsArray);
                    }
                }
                else if (InverseMode.Checked)
                {
                    imageProcessor.ProcessAllPixels(sourcePixels, sourcePixels);

                    if (rawBitMap == null || rawBitMapData == null)
                    {
                        MessageBox.Show("元画像のデータの取得に失敗しました。選択反転モードの結果は作成されません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            MessageBox.Show("テクスチャ画像の作成が完了しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // 元のビットマップをロック
        var sourceRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
        var sourceBitmapData = sourceBitmap.LockBits(sourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        // プレビュー用のビットマップをロック
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

            // 選択範囲の描画
            if (_selectedPointsArrayForPreview != null)
            {
                ImageProcessor.ChangeSelectedPixelsColor(previewPixels, boxWidth, _selectedPointsArrayForPreview, new ColorPixel(255, 0, 0, 255));
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
                _backgroundColor = color;
                backgroundColorBox.BackColor = color;
                coloredPreviewBox.Image = GenerateColoredPreview(previewImage);
                return;
            }

            if (_previousColor == Color.Empty || _newColor == Color.Empty)
            {
                MessageBox.Show("色が選択されていません。（プレビューが作成できません）", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_backgroundColor == Color.Empty)
            {
                MessageBox.Show("背景色が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var previousFormTitle = Text;
            Text = FORM_TITLE + " - 選択処理中...";
            (int x, int y)[] values = BitmapUtils.GetSelectedArea(new Point(originalX, originalY), previewImage, _backgroundColor);
            Text = previousFormTitle;

            if (values.Length == 0)
            {
                MessageBox.Show("選択可能なエリアがありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("選択済みのエリアが含まれています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _selectedPointsArray = _selectedPointsArray.Append(values).ToArray();
            }

            previousFormTitle = Text;
            Text = FORM_TITLE + " - プレビュー用の選択エリア作成中...";
            _selectedPointsArrayForPreview = _selectedPointsArray.Select(points => BitmapUtils.ConvertSelectedAreaToPreviewBox(points, previewImage, previewBox)).ToArray();
            Text = previousFormTitle;

            var totalSelectedPoints = _selectedPointsArray.Sum(points => points.Length);
            Text = FORM_TITLE + $" - {_selectedPointsArray.Length} 個の選択エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
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

    // 計算後のRGBラベルの値の更新
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
                MessageBox.Show("画像の読み込みに失敗しました。非対応のファイルです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("画像の読み込みに失敗しました。\n\nエラー: " + exception, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    // 戻るボタン(選択モード)
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
            Text = FORM_TITLE + $" - {_selectedPointsArray.Length} 個の選択エリア (総選択ピクセル数: {totalSelectedPoints:N0})";
        }

        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    // 選択モードの有効化、無効化
    private void SelectMode_CheckedChanged(object sender, EventArgs e)
    {
        if (selectMode.Checked && _bmp == null)
        {
            MessageBox.Show("画像が読み込まれていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && (_previousColor == Color.Empty || _newColor == Color.Empty))
        {
            MessageBox.Show("色が選択されていません。（プレビューが作成できません）", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            selectMode.Checked = false;
            return;
        }

        if (selectMode.Checked && _backgroundColor == Color.Empty)
        {
            MessageBox.Show("選択モードが有効になりました。はじめに背景色を右クリックで設定してください。", "選択モード", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        backgroundColorBox.Enabled = selectMode.Checked;
        backgroundColorLabel.Enabled = selectMode.Checked;
        UndoButton.Enabled = selectMode.Checked;
    }

    // バランスモードの有効化、無効化
    private void BalanceMode_CheckedChanged(object sender, EventArgs e)
    {
        if (_bmp == null || _previousColor == Color.Empty || _newColor == Color.Empty) return;
        coloredPreviewBox.Image = GenerateColoredPreview(_bmp);
    }

    private void InverseMode_CheckedChanged(object sender, EventArgs e)
    {
        if (InverseMode.Checked) MessageBox.Show("選択反転モードがオンになりました。\n\n- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n- 透過画像作成モードでは、透過する部分が選択部分と逆になります。", "選択反転モード", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AboutThisSoftware_Click(object sender, EventArgs e)
    {
        string message = "Color Changer For Texture " + CURRENT_VERSION + "\n\n";
        message += "柔軟なテクスチャ色変換ツール\n指定した色の差分をもとに、テクスチャの色を簡単に変更できます。\n\n";
        message += "ツール情報:\n制作者: ぷこるふ\nTwitter: @pukorufu\nGithub: https://github.com/puk06/Color-Changer\n\n";
        message += "このソフトウェアは、個人の趣味で作成されたものです。\nもしこのソフトウェアが役に立ったと感じたら、ぜひ支援をお願いします！\n支援先: https://pukorufu.booth.pm/items/6519471\n\n";
        message += "ライセンス:\nこのソフトウェアは、MITライセンスのもとで配布されています。";

        MessageBox.Show(message, "Color Changer For Texture " + CURRENT_VERSION, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BalanceModeSettingsButton_Click(object sender, EventArgs e) => _balanceModeSettings.Show();
}
