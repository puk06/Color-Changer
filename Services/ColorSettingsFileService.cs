using System.Text.Json;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 色情報設定ファイル（.ccs）の読み書き
/// </summary>
internal static class ColorSettingsFileService
{
    /// <summary>
    /// 色情報設定ファイルを読み込む。失敗時は例外を投げる。
    /// </summary>
    internal static ColorSettings? Load(string path)
        => JsonSerializer.Deserialize<ColorSettings>(File.ReadAllText(path));

    /// <summary>
    /// 色情報設定ファイルを書き出す。失敗時は例外を投げる。
    /// </summary>
    internal static void Save(string path, ColorSettings colorSettings)
        => File.WriteAllText(path, JsonSerializer.Serialize(colorSettings));
}
