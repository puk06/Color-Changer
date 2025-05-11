namespace ColorChanger.Utils;

internal class FormUtils
{
    private static readonly Keys ModiferKeys = Control.ModifierKeys;

    /// <summary>
    /// キー入力がナビゲーションキーかどうかを判定する
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    internal static bool IsNavigationKey(KeyEventArgs e)
    {
        return e.KeyCode == Keys.Enter
            || e.KeyCode == Keys.Tab
            || (ModiferKeys == Keys.Shift && e.KeyCode == Keys.Tab);
    }
}
