using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class BalanceModeSettingsForm : Form
{
    /// <summary>
    /// 設定値が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ConfigurationChanged;

    private readonly BalanceModeConfiguration _configuration = new BalanceModeConfiguration();

    /// <summary>
    /// 現在のバランスモードの設定値
    /// </summary>
    internal BalanceModeConfiguration Configuration
    {
        get
        {
            SetConfigurationFromInputs();
            ApplyConfigurationToInputs(_configuration);
            return _configuration;
        }
    }

    private readonly ColorPickerForm _colorPickerForm = new ColorPickerForm();
    private readonly MainForm _mainForm;

    private static readonly string V1SettingsDescription = "このモードの説明:\n" +
        "従来のバランスモードの計算式を使った色変換です。\n\n" +
        "計算式について: \n" +
        "選んだ色と各ピクセルの色の距離、およびその延長線上の位置から変化率を計算します。\n\n" +
        "デメリットについて: \n" +
        "RGB空間の端に近い色は変化が小さくなります。";

    private static readonly string V2SettingsDescription = "このモードの説明:\n" +
        "新しい計算式を使ったバランスモードです。\n\n" +
        "計算式について: \n" +
        "選んだ色を中心に球状に色の変化率を計算し、半径の位置を基準とします。\n\n" +
        "デメリットについて: \n" +
        "RGB空間の制限を受けませんが、設定が少し複雑です。";

    private static readonly string V3SettingsDescription = "このモードの説明:\n" +
        "グレースケール値を使った色変換です。\n\n" +
        "計算式について: \n" +
        "開始色と終了色のグラデーションに沿って、ピクセルの明るさから変化率を決めます。\n\n" +
        "デメリットについて: \n" +
        "色が均一に変わりますが、意図しない部分も変化する可能性があります。";

    internal BalanceModeSettingsForm(MainForm mainForm)
    {
        _mainForm = mainForm;

        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        SetupEventHandlers();

        _colorPickerForm.SetInitialColor(Color.FromArgb(255, 255, 255));
        _colorPickerForm.SetColor(Color.FromArgb(255, 255, 255));

        v2radiusBar.Maximum = (int)Math.Sqrt(3 * 255 * 255) + 1;
        v2radiusBar.TickFrequency = v2radiusBar.Maximum;
        ApplyConfigurationToInputs(_configuration);
    }

    /// <summary>
    /// GradientPreviewImageをリセットする。
    /// </summary>
    internal void ResetGradientPreviewImage()
    {
        gradientPreview.Image?.Dispose();
        gradientPreview.Image = null;
    }

    internal void LoadSettings(BalanceModeConfiguration balanceModeConfiguration)
    {
        ApplyConfigurationToInputs(balanceModeConfiguration);
        GenerateGradientPreview();
    }

    #region Configuration関連
    private void SetConfigurationFromInputs()
    {
        _configuration.ModeVersion = balanceModeComboBox.SelectedIndex + 1;

        _configuration.V1Weight = MathUtils.ParseDoubleOrDefault(v1weight.Text);
        _configuration.V1MinimumValue = MathUtils.ParseDoubleOrDefault(v1minValue.Text);

        _configuration.V2Weight = MathUtils.ParseDoubleOrDefault(v2weight.Text);
        _configuration.V2Radius = MathUtils.ParseDoubleOrDefault(v2radius.Text);
        _configuration.V2MinimumValue = MathUtils.ParseDoubleOrDefault(v2minValue.Text);
        _configuration.V2IncludeOutside = v2includeOutside.Checked;

        _configuration.V3GradientColor = new SerializableColor(_colorPickerForm.SelectedColor);
        _configuration.V3GradientStart = v3gradientStart.Value;
        _configuration.V3GradientEnd = v3gradientEnd.Value;

        GenerateGradientPreview();
    }

    private void ApplyConfigurationToInputs(BalanceModeConfiguration config)
    {
        balanceModeComboBox.SelectedIndex = config.ModeVersion - 1;

        v1weight.Text = config.V1Weight.ToString("F2");
        v1minValue.Text = config.V1MinimumValue.ToString("F2");

        v2weight.Text = config.V2Weight.ToString("F2");
        v2radius.Text = config.V2Radius.ToString("F2");
        v2radiusBar.Value = Math.Clamp(Convert.ToInt32(config.V2Radius), v2radiusBar.Minimum, v2radiusBar.Maximum);
        v2minValue.Text = config.V2MinimumValue.ToString("F2");
        v2includeOutside.Checked = config.V2IncludeOutside;

        var selectedColor = _mainForm.ColorPickerForm.SelectedColor;
        v3gradientColorStart.BackColor = _mainForm.NewColorSelected ? selectedColor : ColorUtils.DefaultBackgroundColor;

        _colorPickerForm.SetColor(config.V3GradientColor.ToColor(), true);
        v3gradientColorEnd.BackColor = config.V3GradientColor.ToColor();
        v3gradientStart.Value = config.V3GradientStart;
        v3gradientEnd.Value = config.V3GradientEnd;
    }

    private void GenerateGradientPreview()
    {
        Color CurrentSelectedColor = _mainForm.ColorPickerForm.SelectedColor;
        if (CurrentSelectedColor == Color.Empty) return;

        Bitmap gradientPreviewImage = ColorUtils.GenerateGradientPreview(
            CurrentSelectedColor,
            _configuration.V3GradientColor.ToColor(),
            _configuration.V3GradientStart,
            _configuration.V3GradientEnd,
            gradientPreview.Size
        );

        ResetGradientPreviewImage();
        gradientPreview.Image = gradientPreviewImage;
    }
    #endregion

    #region イベントハンドラー
    /// <summary>
    /// イベントハンドラのセットアップ
    /// </summary>
    private void SetupEventHandlers()
    {
        _colorPickerForm.ColorChanged += (s, e) =>
        {
            v3gradientColorEnd.BackColor = _colorPickerForm.SelectedColor;
            NotifyConfigurationChanged();
        };
    }

    private void BalanceModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        int version = balanceModeComboBox.SelectedIndex + 1;

        switch (version)
        {
            case 1:
                balanceModeDescription.Text = V1SettingsDescription;
                break;
            case 2:
                balanceModeDescription.Text = V2SettingsDescription;
                break;
            case 3:
                balanceModeDescription.Text = V3SettingsDescription;
                break;
        }

        balanceModeSettingsTab.SelectedIndex = balanceModeComboBox.SelectedIndex;

        NotifyConfigurationChanged();
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        NotifyConfigurationChanged();
    }

    private void V2radiusBar_Change(object sender, EventArgs e)
    {
        v2radius.Text = v2radiusBar.Value.ToString("F2");
        NotifyConfigurationChanged();
    }

    private void V2includeOutside_CheckedChanged(object sender, EventArgs e)
        => NotifyConfigurationChanged();

    private void V3gradientColorStart_Click(object sender, EventArgs e)
    {
        _mainForm.ColorPickerForm.Show();
        _mainForm.ColorPickerForm.BringToFront();
    }

    private void V3GradientColorEnd_Click(object sender, EventArgs e)
    {
        _colorPickerForm.Show();
        _colorPickerForm.BringToFront();
    }

    private void V3gradient_Change(object sender, EventArgs e)
        => NotifyConfigurationChanged();

    private void NotifyConfigurationChanged()
        => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void BalanceModeSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
