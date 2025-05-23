using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class BalanceModeSettingsForm : Form
{
    /// <summary>
    /// 設定値が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ConfigurationChanged;

    /// <summary>
    /// 現在のバランスモードの設定値
    /// </summary>
    private readonly BalanceModeConfiguration _configuration = new BalanceModeConfiguration();
    internal BalanceModeConfiguration Configuration
    {
        get {
            SetConfigurationFromInputs();
            ApplyConfigurationToInputs(_configuration);
            return _configuration;
        }
        set => ApplyConfigurationToInputs(value);
    }

    private static readonly string V1SettingsDescription = "このモードの説明:\n" +
        "従来のバランスモードの計算式を使った色変換モードです。\n\n" +
        "計算式について: \n" +
        "RGB空間上で選択されている色の点からそれぞれのピクセルの色までの距離と、その延長線上の点までの距離を元に色を計算し、色の変化率のグラフを作成します。" +
        "このモードは、色がRGB空間上の壁に近いと色があまり変化しないというデメリットがあります。";

    private static readonly string V2SettingsDescription = "このモードの説明:\n" +
        "新しいバランスモードの計算式を使った色変換モードです。\n\n" +
        "計算式について: \n" +
        "RGB空間上で、選択した色の点から球状に広がるように色の変化率を計算します。球の半径の場所から色の変化率を計算するようになります。" +
        "このモードはRGB空間上で壁に関係なく均等に色を変えることが出来ますが、v1と比べて値の設定が複雑で難しいというデメリットがあります。";

    internal BalanceModeSettingsForm()
    {
        InitializeComponent();

        v2radiusBar.Maximum = (int)Math.Sqrt(3 * 255 * 255) + 1;
        Configuration = new BalanceModeConfiguration();
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
    }

    private void ApplyConfigurationToInputs(BalanceModeConfiguration config)
    {
        balanceModeComboBox.SelectedIndex = config.ModeVersion - 1;
        v1weight.Text = config.V1Weight.ToString("F2");
        v1minValue.Text = config.V1MinimumValue.ToString("F2");
        v2weight.Text = config.V2Weight.ToString("F2");
        v2radius.Text = config.V2Radius.ToString("F2");
        v2radiusBar.Value = (int)config.V2Radius;
        v2minValue.Text = config.V2MinimumValue.ToString("F2");
        v2includeOutside.Checked = config.V2IncludeOutside;
    }
    #endregion

    #region イベントハンドラー
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
        }

        balanceModeSettingsTab.SelectedIndex = balanceModeComboBox.SelectedIndex;

        NotifyConfigurationChanged();
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        NotifyConfigurationChanged();
    }

    private void V2radiusBar_MouseUp(object sender, MouseEventArgs e)
    {
        v2radius.Text = v2radiusBar.Value.ToString("F2");
        NotifyConfigurationChanged();
    }

    private void V2includeOutside_CheckedChanged(object sender, EventArgs e)
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
