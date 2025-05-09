namespace ColorChanger.Models;

internal class BalanceModeConfiguration
{
    internal int modeVersion;

    internal double v1Weight = 1.0;
    internal double v1MinimumValue = 0.0;

    internal double v2Weight = 1.0;
    internal double v2Radius = 0.0;
    internal double v2MinimumValue = 0.0;
    internal bool v2IncludeOutside = false;
}