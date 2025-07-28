using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal class InputDialogForm : Form
{
    private readonly TextBox _inputBox;

    internal string InputText => _inputBox.Text;

    internal InputDialogForm(string promptText, string title)
    {
        Text = title;
        Icon = FormUtils.GetSoftwareIcon();
        Width = 400;
        Height = 150;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        Label label = new Label
        {
            Left = 10,
            Top = 10,
            Text = promptText,
            Width = 360,
            Font = new Font("Yu Gothic UI", 11F)
        };

        _inputBox = new TextBox
        {
            Left = 10,
            Top = 40,
            Width = 360,
            Font = new Font("Yu Gothic UI", 10F)
        };

        Button okButton = new Button
        {
            Text = "OK",
            Left = 280,
            Width = 90,
            Top = 75,
            DialogResult = DialogResult.OK
        };

        okButton.Click += (sender, e) => Close();

        Controls.Add(label);
        Controls.Add(_inputBox);
        Controls.Add(okButton);

        AcceptButton = okButton;
    }

    /// <summary>
    /// InputDialogFormを表示します。
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    internal string? ShowDialog(string defaultValue)
    {
        _inputBox.Text = defaultValue;
        return ShowDialog() == DialogResult.OK ? InputText : null;
    }
}
