namespace ColorChanger.Models;

/// <summary>
/// 色変換（プレビュー・テクスチャ作成）に使う設定値
/// </summary>
internal class ConversionSettings
{
    internal required ColorDifference ColorDifference { get; init; }
    internal required AdvancedColorConfiguration AdvancedColor { get; init; }

    /// <summary>
    /// バランスモードの設定。バランスモードが無効な場合は null。
    /// </summary>
    internal BalanceModeConfiguration? Balance { get; init; }

    internal bool TransparentMode { get; init; }
    internal bool InverseMode { get; init; }
    internal bool MaskMode { get; init; }
}
