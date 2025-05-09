using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class BalanceModeSettings : Form
{
    internal event EventHandler? ValueChanged;

    internal BalanceModeConfiguration Configuration
    {
        get => CreateConfigurationFromInputs();
        set => ApplyConfigurationToInputs(value);
    }

    private BalanceModeConfiguration CreateConfigurationFromInputs()
    {
        return new BalanceModeConfiguration
        {
            modeVersion = balanceModeComboBox.SelectedIndex + 1,
            v1Weight = MathUtils.ParseDoubleOrDefault(v1weight.Text),
            v1MinimumValue = MathUtils.ParseDoubleOrDefault(v1minValue.Text),
            v2Weight = MathUtils.ParseDoubleOrDefault(v2weight.Text),
            v2Radius = MathUtils.ParseDoubleOrDefault(v2radius.Text),
            v2MinimumValue = MathUtils.ParseDoubleOrDefault(v2minValue.Text),
            v2IncludeOutside = v2includeOutside.Checked
        };
    }

    private void ApplyConfigurationToInputs(BalanceModeConfiguration config)
    {
        v1weight.Text = config.v1Weight.ToString("F2");
        v1minValue.Text = config.v1MinimumValue.ToString("F2");
        v2weight.Text = config.v2Weight.ToString("F2");
        v2radius.Text = config.v2Radius.ToString("F2");
        v2radiusBar.Value = (int)config.v2Radius;
        v2minValue.Text = config.v2MinimumValue.ToString("F2");
        v2includeOutside.Checked = config.v2IncludeOutside;
    }

    internal BalanceModeSettings()
    {
        InitializeComponent();
        balanceModeComboBox.SelectedIndex = 0;
        v2radiusBar.Maximum = (int)Math.Sqrt(3 * 255 * 255) + 1;
        Configuration = new BalanceModeConfiguration();
    }

    private void BalanceModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        string description = "このモードの説明:\n";
        int version = balanceModeComboBox.SelectedIndex + 1;

        switch (version)
        {
            case 1:
                description +=
                    "従来のバランスモードの計算式を使った色変換モードです。\n\n" +
                    "計算式について: \n" +
                    "RGB空間上で選択されている色からそれぞれのピクセルの色までの距離と、その延長線上の点までの距離を元に色を計算し、色の変化率のグラフを作成します。" +
                    "このモードは、色がRGB空間上の壁に近いと色があまり変化しないというデメリットがあります。";
                break;
            case 2:
                description +=
                    "新しいバランスモードの計算式を使った色変換モードです。\n\n" +
                    "計算式について: \n" +
                    "RGB空間上に選択した色から球状に広がるように色の変化率を計算します。" +
                    "このモードはRGB空間上で壁に関係なく均等に色を変えることが出来ますが、v1と比べて値の設定が複雑で難しいというデメリットがあります。";
                break;
        }

        balanceModeSettingsTab.SelectedIndex = balanceModeComboBox.SelectedIndex;
        balanceModeDescription.Text = description;

        TriggerValueChanged();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) TriggerValueChanged();
    }

    private void V2radiusBar_MouseUp(object sender, MouseEventArgs e)
    {
        v2radius.Text = v2radiusBar.Value.ToString("F2");
        TriggerValueChanged();
    }

    private void V2includeOutside_CheckedChanged(object sender, EventArgs e) => TriggerValueChanged();

    private void BalanceModeSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }

    private void TriggerValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
}
