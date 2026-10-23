namespace ColorChanger.Forms;

/// <summary>
/// 閉じるボタンで破棄せず、非表示にするフォームの基底クラス
/// </summary>
internal class HideOnCloseForm : Form
{
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
