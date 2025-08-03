using ColorChanger.Utils;
using ColorChanger.UserControls;

namespace ColorChanger.Forms;

internal partial class ColorPickerForm : Form
{
    /// <summary>
    /// 色が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ColorChanged;

    /// <summary>
    /// 現在選択されている色
    /// </summary>
    internal Color SelectedColor { get; private set; } = Color.Empty;

    private Color _initialColor = Color.Empty;

    internal ColorPickerForm()
    {
        colorPalette = new ColorPalette();
        InitializeComponent();
        RegisterEvents();
        Icon = FormUtils.GetSoftwareIcon();

        UpdateSelectedColor(SelectedColor);
    }

    private void RegisterEvents()
    {
        colorPalette.ColorSelected += (_, color) => UpdateSelectedColor(color);
    }

    #region 色設定関連
    /// <summary>
    /// 色を設定する
    /// </summary>
    /// <param name="newColor"></param>
    /// <param name="skipUpdate"></param>
    internal void SetColor(Color newColor, bool skipUpdate = false)
    {
        SelectedColor = newColor == Color.Empty ? Color.White : newColor;
        if (!skipUpdate)
        {
            UpdateSelectedColor(SelectedColor);
            colorPalette.SelectColor(SelectedColor);
        }
    }

    /// <summary>
    /// デフォルトの色を設定する
    /// </summary>
    /// <param name="initialColor"></param>
    internal void SetInitialColor(Color initialColor)
        => _initialColor = initialColor;

    /// <summary>
    /// 色を更新する
    /// </summary>
    /// <param name="color"></param>
    private void UpdateSelectedColor(Color color)
    {
        SelectedColor = color;

        previewColorBox.BackColor = color;

        redBar.Value = color.R;
        greenBar.Value = color.G;
        blueBar.Value = color.B;

        redTextBox.Text = color.R.ToString();
        greenTextBox.Text = color.G.ToString();
        blueTextBox.Text = color.B.ToString();

        colorCodeTextBox.Text = ColorUtils.GetColorCodeFromColor(color);
        NotifyColorChanged();
    }

    /// <summary>
    /// 色をテキストボックスから取得して設定する
    /// </summary>
    private void SetColorFromTextFields()
    {
        int r = MathUtils.ParseAndClamp(redTextBox.Text);
        int g = MathUtils.ParseAndClamp(greenTextBox.Text);
        int b = MathUtils.ParseAndClamp(blueTextBox.Text);

        Color color = Color.FromArgb(r, g, b);
        UpdateSelectedColor(color);
        colorPalette.SelectColor(color);
    }
    #endregion

    #region イベントハンドラー

    private void HandleSliderChanged(object sender, EventArgs e)
        => UpdateSelectedColor(Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value));

    private void HandleSliderEnd(object sender, EventArgs e)
    {
        Color color = Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value);
        UpdateSelectedColor(color);
        colorPalette.SelectColor(color);
    }

    private void HandleTextKeyDown(object sender, KeyEventArgs e)
    {
        if (!FormUtils.IsNavigationKey(e)) return;

        SetColorFromTextFields();
        SelectNextControl((Control)sender, true, true, true, true);
    }

    private void ColorCodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (!FormUtils.IsNavigationKey(e)) return;

        Color color = ColorUtils.GetColorFromColorCode(colorCodeTextBox.Text);
        if (color == Color.Empty) return;

        UpdateSelectedColor(color);
        colorPalette.SelectColor(color);
        SelectNextControl((Control)sender, true, true, true, true);
    }

    private void OnColorTextChanged(object sender, EventArgs e)
        => SetColorFromTextFields();

    private void ResetButton_Click(object sender, EventArgs e)
    {
        UpdateSelectedColor(_initialColor);
        colorPalette.SelectColor(_initialColor);
    }

    private void SelectButton_Click(object sender, EventArgs e)
    {
        SetColorFromTextFields();
        Hide();
    }

    private void NotifyColorChanged()
        => ColorChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void ColorPicker_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
