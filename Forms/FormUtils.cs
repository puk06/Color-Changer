using ColorChanger.Services;

namespace ColorChanger.Forms;

internal static class FormUtils
{
    internal static readonly Color DefaultBackgroundColor = Color.LightGray;
    internal static readonly Color DefaultForegroundColor = Color.LightGreen;
    internal static readonly Color DefaultUnselectedColor = Color.LightCoral;

    private static readonly Icon _softwareIcon = new Icon(new MemoryStream(Properties.Resources.AppIcon));

    /// <summary>
    /// フォームのアイコンで使用する、ソフトウェアアイコンを取得する。
    /// </summary>
    internal static Icon GetSoftwareIcon() => _softwareIcon;

    /// <summary>
    /// キー入力がEnterまたはTabかどうかを判定する
    /// </summary>
    internal static bool IsNavigationKey(KeyEventArgs e)
        => e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab;

    internal static void ShowError(string message, string title = "エラー")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    internal static void ShowInfo(string message, string title = "情報")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    internal static bool ShowConfirm(string message, string title = "確認")
        => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    /// <summary>
    /// ラベルの位置を指定された場所から右寄せにする
    /// </summary>
    internal static void AlignTextRight(Label label, Point point)
    {
        var textSize = TextRenderer.MeasureText(label.Text, label.Font);
        label.Location = new Point(point.X - textSize.Width, point.Y);
    }

    /// <summary>
    /// ファイルのドラッグ＆ドロップから、最初のファイルのパスを取得する
    /// </summary>
    internal static string? GetDroppedFile(DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop, false) is string[] files && files.Length > 0)
        {
            return files[0];
        }

        return null;
    }

    /// <summary>
    /// ファイル選択ダイアログを表示し、選択されたパスを返す
    /// </summary>
    internal static string? PickFile(string filter, string title)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }

    /// <summary>
    /// 出力したファイルをエクスプローラーで開くか確認し、開く
    /// </summary>
    internal static void RevealFileWithConfirm(string path)
    {
        if (!ShowConfirm("出力先のファイルのパスを開きますか？")) return;

        try
        {
            ShellService.RevealInExplorer(path);
        }
        catch
        {
            ShowError("出力先のファイルを開けませんでした。");
        }
    }

    /// <summary>
    /// PictureBox に画像を表示する。以前の画像は破棄される。
    /// </summary>
    internal static void SetOwnedImage(PictureBox pictureBox, Bitmap? image)
    {
        var previous = pictureBox.Image;
        pictureBox.Image = image;
        previous?.Dispose();
        pictureBox.Invalidate();
    }

    /// <summary>
    /// 指定座標に、色の反転した十字のマーカーを描画する
    /// </summary>
    internal static void DrawCrosshair(Graphics graphics, Point point, Color color)
    {
        using var pen = new Pen(ColorService.InverseColor(color), 2);
        graphics.DrawLine(pen, point.X - 5, point.Y, point.X + 5, point.Y);
        graphics.DrawLine(pen, point.X, point.Y - 5, point.X, point.Y + 5);
    }

    private static readonly SolidBrush _selectionOverlayBrush = new SolidBrush(Color.FromArgb(150, 255, 0, 0));

    /// <summary>
    /// 選択ペンのプレビュー（縮小マップ）を描画する
    /// </summary>
    internal static void DrawSelectionMap(Graphics graphics, bool[,] previewMap)
    {
        var width = previewMap.GetLength(0);
        var height = previewMap.GetLength(1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (previewMap[x, y])
                {
                    graphics.FillRectangle(_selectionOverlayBrush, x, y, 1, 1);
                }
            }
        }
    }
}
