using ColorChanger.Utils;

namespace ColorChanger.Models;

internal class AdvancedColorConfiguration
{
    internal bool Enabled { get; private set; } = false;

    internal double Brightness { get; set; } = 1.0;
    internal double Contrast { get; set; } = 1.0;
    internal double Gamma { get; set; } = 1.0;
    internal double Exposure { get; set; } = 0.0;
    internal double Transparency { get; set; } = 0.0;

    internal bool BrightnessEnabled { get; private set; } = false;

    internal bool ContrastEnabled { get; private set; } = false;

    internal bool GammaEnabled { get; private set; } = false;

    internal bool ExposureEnabled { get; private set; } = false;

    internal bool TransparencyEnabled { get; private set; } = false;

    /// <summary>
    /// コンポーネントの有効/無効を更新する
    /// </summary>
    internal void UpdateComponentActivationStatus()
    {
        BrightnessEnabled = !MathUtils.EqualDouble(Brightness, 1.0);
        ContrastEnabled = !MathUtils.EqualDouble(Contrast, 1.0);
        GammaEnabled = !MathUtils.EqualDouble(Gamma, 1.0);
        ExposureEnabled = !MathUtils.EqualDouble(Exposure, 0.0);
        TransparencyEnabled = !MathUtils.EqualDouble(Transparency, 0.0);

        Enabled = BrightnessEnabled || ContrastEnabled || GammaEnabled || ExposureEnabled || TransparencyEnabled;
    }
}
