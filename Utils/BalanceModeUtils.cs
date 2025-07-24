using ColorChanger.Models;

namespace ColorChanger.Utils;

internal static class BalanceModeUtils
{
    private static readonly BalanceModeConfiguration _configuration = new BalanceModeConfiguration();

    /// <summary>
    /// 空のバランスモードを取得します。
    /// </summary>
    /// <returns></returns>
    internal static BalanceModeConfiguration GetEmpty()
        => _configuration;
}
