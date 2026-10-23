using System.Diagnostics;

namespace ColorChanger.Services;

/// <summary>
/// 外部アプリケーション（ブラウザ・エクスプローラー）の起動
/// </summary>
internal static class ShellService
{
    internal static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// エクスプローラーでファイルを選択した状態で開く
    /// </summary>
    internal static void RevealInExplorer(string path)
    {
        Process.Start("explorer.exe", "/select," + path);
    }
}
