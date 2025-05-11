namespace ColorChanger.Utils;

internal class FormUtils
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
}
