using System.Drawing.Imaging;

namespace ColorChanger.Services;

/// <summary>
/// 画像ファイルの読み込み・保存とファイル名に関する処理
/// </summary>
internal static class ImageFileService
{
    /// <summary>
    /// 画像ファイルを読み込み、32bppArgb の Bitmap として返す。失敗時は例外を投げる。
    /// </summary>
    internal static Bitmap Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var decoded = new Bitmap(stream);

        return CloneAsArgb32(decoded);
    }

    /// <summary>
    /// Bitmap を 32bppArgb の新しいコピーとして複製する
    /// </summary>
    internal static Bitmap CloneAsArgb32(Bitmap source)
        => source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format32bppArgb);

    /// <summary>
    /// Bitmap のメモリ使用量（MB）を推定する
    /// </summary>
    internal static double EstimateMemoryUsageMegabytes(Bitmap bitmap)
    {
        var bitsPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat);
        var totalBytes = (long)bitmap.Width * bitmap.Height * bitsPerPixel / 8;

        return totalBytes / 1024.0 / 1024.0;
    }

    /// <summary>
    /// 変換元のファイル名から、出力用の新しいファイル名を取得する
    /// </summary>
    internal static string GetNewFileName(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return "New Texture.png";

        var originalFileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrEmpty(originalFileName)) originalFileName = "New Texture";

        var originalExtension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(originalExtension)) originalExtension = ".png";

        return originalFileName + "_new" + originalExtension;
    }

    /// <summary>
    /// 保存ダイアログの初期フォルダを取得する
    /// </summary>
    internal static string GetInitialDirectory(string? filePath)
        => Path.GetDirectoryName(filePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
}
