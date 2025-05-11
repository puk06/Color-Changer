namespace ColorChanger.Utils;

internal class FileUtils
{
    /// <summary>
    /// 新しいファイル名を取得する
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static string GetNewFileName(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return "New Texture.png";

        string originalFileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        string originalExtension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        return originalFileName + "_new" + originalExtension;
    }

    /// <summary>
    /// 初期ディレクトリを取得する
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static string GetInitialDirectory(string? filePath)
        => Path.GetDirectoryName(filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
}
