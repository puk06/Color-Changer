namespace ColorChanger.Models;

internal class BalanceModeConfiguration
{
    internal int ModeVersion { get; set; } = 1;

    internal double V1Weight { get; set; } = 1.0;
    internal double V1MinimumValue { get; set; } = 0.0;

    internal double V2Weight { get; set; } = 1.0;
    internal double V2Radius { get; set; } = 0.0;
    internal double V2MinimumValue { get; set; } = 0.0;
    internal bool V2IncludeOutside { get; set; } = false;

    internal Color V3GradientColor { get; set; } = Color.FromArgb(255, 255, 255);
    internal int V3GradientStart { get; set; } = 0;
    internal int V3GradientEnd { get; set; } = 100;
}