using ColorChanger.Models;
using System.Text.Json;

namespace ColorChanger.Utils;

internal static class UpdateUtils
{
    private static readonly HttpClient _httpClient = new HttpClient();

    internal async static Task<bool> CheckUpdate(string currentVersion, bool silent = false)
    {
        try
        {
            string response = await _httpClient.GetStringAsync(ItemUtils.UpdateCheckURL);
            VersionData? versionData = JsonSerializer.Deserialize<VersionData>(response);
            if (versionData == null)
            {
                if (!silent) FormUtils.ShowError("アップデート情報の取得中にエラーが発生しました");
                return false;
            }

            if (silent) return versionData.LatestVersion != currentVersion;
            
            if (versionData.LatestVersion != currentVersion)
            {
                bool result = FormUtils.ShowConfirm(
                    "新しいColor Changerのバージョンが利用可能です！\n\n" +
                    "「はい」をクリックすると商品ページを開きます。\n\n" +
                    $"現在のバージョン: {currentVersion}\n" +
                    $"最新のバージョン: {versionData.LatestVersion}\n\n" +
                    $"以下は最新版（{versionData.LatestVersion}）の変更内容です。\n\n" +
                    string.Join("\n", versionData.ChangeLog.Select(log => $"・{log}"))
                );

                if (result)
                {
                    ItemUtils.OpenItemURL();
                }

                return true;
            }
            else if (versionData.LatestVersion == currentVersion)
            {
                FormUtils.ShowInfo(
                    "ご利用中のColor Changerは最新版です！\n\n" +
                    "いつもご利用いただき、ありがとうございます。\n" +
                    "今後のアップデートもぜひお楽しみに！\n\n" +
                    $"以下は最新版（{versionData.LatestVersion}）の変更内容です。\n\n" +
                    string.Join("\n", versionData.ChangeLog.Select(log => $"・{log}"))
                );
            }

            return false;
        }
        catch
        {
            if (!silent)
            {
                FormUtils.ShowError(
                    "アップデート情報の取得中にエラーが発生しました。\n" +
                    "通信環境やサーバーの状況が原因の可能性があります。"
                );
            }

            return false;
        }
    }
}
