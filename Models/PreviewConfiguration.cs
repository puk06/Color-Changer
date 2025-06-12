using System.Collections;

namespace ColorChanger.Models;

internal class PreviewConfiguration
{
    internal Bitmap SourceBitmap { get; set; } = null!;
    internal ColorDifference ColorDifference { get; set; } = null!;
    internal bool BalanceMode { get; set; } = false;
    internal BalanceModeConfiguration BalanceModeConfiguration { get; set; } = null!;
    internal AdvancedColorConfiguration AdvancedColorConfiguration { get; set; } = null!;
    internal BitArray SelectedPoints { get; set; } = null!;
    internal Size PreviewBoxSize { get; set; } = Size.Empty;
    internal bool RawMode { get; set; } = false;
}
