using System.Text.Json;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 公開されている最新バージョン情報を取得する
/// </summary>
internal static class UpdateService
{
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// 最新バージョン情報を取得する。通信や解析に失敗した場合は例外を投げる。
    /// </summary>
    internal static async Task<VersionData?> FetchLatestVersionAsync()
    {
        var response = await _httpClient.GetStringAsync(AppInfo.UpdateCheckUrl);
        return JsonSerializer.Deserialize<VersionData>(response);
    }
}
