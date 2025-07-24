namespace ColorChanger.Models;

public class BalanceModeConfiguration
{
    public int ModeVersion { get; set; } = 1;

    public double V1Weight { get; set; } = 1.0;
    public double V1MinimumValue { get; set; } = 0.0;

    public double V2Weight { get; set; } = 1.0;
    public double V2Radius { get; set; } = 0.0;
    public double V2MinimumValue { get; set; } = 0.0;
    public bool V2IncludeOutside { get; set; } = false;

    public SerializableColor V3GradientColor { get; set; } = new SerializableColor(Color.FromArgb(255, 255, 255));
    public int V3GradientStart { get; set; } = 0;
    public int V3GradientEnd { get; set; } = 100;
}