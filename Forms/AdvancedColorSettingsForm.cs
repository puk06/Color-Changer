using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.Forms;

public partial class AdvancedColorSettingsForm : Form
{
    /// <summary>
    /// 設定値が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ConfigurationChanged;

    /// <summary>
    /// 現在の色の設定値
    /// </summary>
    private readonly AdvancedColorConfiguration _configuration = new AdvancedColorConfiguration();
    internal AdvancedColorConfiguration Configuration
    {
        get
        {
            SetConfigurationFromInputs();
            ApplyConfigurationToInputs(_configuration);
            return _configuration;
        }
        set => ApplyConfigurationToInputs(value);
    }

    #region Configuration関連
    private void SetConfigurationFromInputs()
    {
        _configuration.Brightness = MathUtils.ParseDoubleOrDefault(brightness.Text, 1.0);
        _configuration.Contrast = MathUtils.ParseDoubleOrDefault(contrast.Text, 1.0);
        _configuration.Gamma = MathUtils.ParseDoubleOrDefault(gamma.Text, 1.0);
        _configuration.Exposure = MathUtils.ParseDoubleOrDefault(exposure.Text);
    }

    private void ApplyConfigurationToInputs(AdvancedColorConfiguration config)
    {
        brightness.Text = config.Brightness.ToString("F2");
        contrast.Text = config.Contrast.ToString("F2");
        gamma.Text = config.Gamma.ToString("F2");
        exposure.Text = config.Exposure.ToString("F2");
    }
    #endregion

    public AdvancedColorSettingsForm()
    {
        InitializeComponent();

        Configuration = new AdvancedColorConfiguration();
    }


    #region イベントハンドラー
    private void ResetButton_Click(object sender, EventArgs e)
    {
        brightness.Text = "1.00";
        contrast.Text = "1.00";
        gamma.Text = "1.00";
        exposure.Text = "0.00";
        SetConfigurationFromInputs();

        NotifyConfigurationChanged();
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) NotifyConfigurationChanged();
    }

    private void NotifyConfigurationChanged() 
        => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void AdvancedColorSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
