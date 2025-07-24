using System.Diagnostics;

namespace ColorChanger.Utils;

internal static class FileSystemUtils
{
    /// <summary>
    /// アイテムのファイルパスを開きます。
    /// </summary>
    internal static void OpenFilePath(string path)
    {
        var result = FormUtils.ShowConfirm("出力先のファイルのパスを開きますか？");
        if (!result) return;

        try
        {
            Process.Start("explorer.exe", "/select," + path);
        }
        catch
        {
            FormUtils.ShowError("出力先のファイルを開けませんでした。");
        }
    }
}
