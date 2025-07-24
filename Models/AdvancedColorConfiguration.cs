using ColorChanger.Utils;

namespace ColorChanger.Models;

public class AdvancedColorConfiguration
{
    public bool Enabled { get; private set; } = false;

    public double Brightness { get; set; } = 1.0;
    public double Contrast { get; set; } = 1.0;
    public double Gamma { get; set; } = 1.0;
    public double Exposure { get; set; } = 0.0;
    public double Transparency { get; set; } = 0.0;

    public bool BrightnessEnabled { get; private set; } = false;
    public bool ContrastEnabled { get; private set; } = false;
    public bool GammaEnabled { get; private set; } = false;
    public bool ExposureEnabled { get; private set; } = false;
    public bool TransparencyEnabled { get; private set; } = false;

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
