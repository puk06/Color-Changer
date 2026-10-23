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
        BrightnessEnabled = !IsSame(Brightness, 1.0);
        ContrastEnabled = !IsSame(Contrast, 1.0);
        GammaEnabled = !IsSame(Gamma, 1.0);
        ExposureEnabled = !IsSame(Exposure, 0.0);
        TransparencyEnabled = !IsSame(Transparency, 0.0);

        Enabled = BrightnessEnabled || ContrastEnabled || GammaEnabled || ExposureEnabled || TransparencyEnabled;
    }

    /// <summary>
    /// 小数点以下2桁までの値が一致するかを判定する
    /// </summary>
    private static bool IsSame(double value1, double value2)
        => (int)(value1 * 100) == (int)(value2 * 100);
}
