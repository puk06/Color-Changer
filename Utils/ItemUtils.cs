using System.Diagnostics;

namespace ColorChanger.Utils;

internal static class ItemUtils
{
    private static readonly string ITEM_URL = Properties.Resources.ItemURL;
    private static readonly string GITHUB_URL = Properties.Resources.GithubURL;
    private static readonly string UPDATE_CHECK_URL = Properties.Resources.UpdateCheckURL;
    private static readonly ProcessStartInfo UrlProcessStartInfo = new ProcessStartInfo()
    {
        FileName = ITEM_URL,
        UseShellExecute = true
    };

    internal static string ItemURL = ITEM_URL;
    internal static string GithubURL = GITHUB_URL;
    internal static string UpdateCheckURL = UPDATE_CHECK_URL;

    internal static void OpenItemURL()
    {
        try
        {
            Process.Start(UrlProcessStartInfo);
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"リンクを開くことができませんでした。\n{ex.Message}");
        }
    }
}
