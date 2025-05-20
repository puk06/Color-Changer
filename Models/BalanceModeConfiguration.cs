namespace ColorChanger.Models;

internal class BalanceModeConfiguration
{
    internal int ModeVersion { get; set; } = 1; // 1: v1, 2: v2

    internal double V1Weight { get; set; } = 1.0;
    internal double V1MinimumValue { get; set; } = 0.0;

    internal double V2Weight { get; set; } = 1.0;
    internal double V2Radius { get; set; } = 0.0;
    internal double V2MinimumValue { get; set; } = 0.0;
    internal bool V2IncludeOutside { get; set; } = false;
}