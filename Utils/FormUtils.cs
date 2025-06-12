namespace ColorChanger.Utils;

internal static class FormUtils
{
    /// <summary>
    /// キー入力がナビゲーションキーかどうかを判定する
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    internal static bool IsNavigationKey(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
        {
            return true;
        }

        if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Shift)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// エラーメッセージを表示する
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    internal static void ShowError(string message, string title = "エラー")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    /// <summary>
    /// 情報メッセージを表示する
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    internal static void ShowInfo(string message, string title = "情報")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    /// <summary>
    /// Yes / No 形式の確認メッセージを表示する
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    internal static bool ShowConfirm(string message, string title = "確認")
    {
        DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        return result == DialogResult.Yes;
    }

    /// <summary>
    /// ラベルの位置を指定された場所から右寄せにする
    /// </summary>
    /// <param name="label"></param>
    /// <param name="point"></param>
    internal static void AlignTextRight(Label label, Point point)
    {
        int x = point.X;
        int y = point.Y;

        Size textSize = TextRenderer.MeasureText(label.Text, label.Font);
        int textWidthSize = textSize.Width;

        label.Location = new Point(x - textWidthSize, y);
    }
}
