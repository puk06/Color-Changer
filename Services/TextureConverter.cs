using System.Collections;
using System.Drawing.Imaging;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 元画像に色変換を適用し、テクスチャ画像を生成する
/// </summary>
internal static class TextureConverter
{
    internal sealed record ConversionResult(Bitmap Image, IReadOnlyList<string> Notices);

    /// <summary>
    /// 色変換を実行する
    /// </summary>
    /// <param name="source">元画像（32bppArgb）</param>
    /// <param name="settings">色変換の設定</param>
    /// <param name="selection">元画像座標系での選択エリア。空の場合は全体が対象。</param>
    /// <returns>生成画像。呼び出し側が Dispose すること。</returns>
    internal static ConversionResult Convert(Bitmap source, ConversionSettings settings, BitArray selection)
    {
        var notices = new List<string>();

        var output = ImageFileService.CloneAsArgb32(source);
        var rawCopy = settings.InverseMode ? ImageFileService.CloneAsArgb32(source) : null;
        var transparent = settings.TransparentMode && !settings.InverseMode && selection.Length != 0
            ? CreateTransparentLayer(source.Size)
            : null;

        Bitmap? result = null;
        try
        {
            var skipped = ProcessPixels(output, rawCopy, transparent, settings, selection, notices);

            var useTransparentLayer = transparent != null && !skipped;
            result = useTransparentLayer ? transparent : output;

            return new ConversionResult(result!, notices);
        }
        finally
        {
            rawCopy?.Dispose();

            if (result != output) output.Dispose();
            if (result != transparent) transparent?.Dispose();
        }
    }

    private static Bitmap CreateTransparentLayer(Size size)
    {
        var layer = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(layer);
        graphics.Clear(Color.Transparent);
        return layer;
    }

    private static bool ProcessPixels(
        Bitmap output,
        Bitmap? rawCopy,
        Bitmap? transparent,
        ConversionSettings settings,
        BitArray selection,
        List<string> notices
    )
    {
        var processor = new ImageProcessor(settings.ColorDifference, settings.Balance, settings.AdvancedColor);

        using var outputLock = new PixelLock(output, ImageLockMode.ReadWrite);
        using var rawLock = rawCopy == null ? null : new PixelLock(rawCopy, ImageLockMode.ReadOnly);
        using var transparentLock = transparent == null ? null : new PixelLock(transparent, ImageLockMode.ReadWrite);

        var sourcePixels = outputLock.Pixels;

        if (selection.Length == 0)
        {
            var skipped = settings.TransparentMode;
            if (skipped)
            {
                notices.Add("選択エリアがなかったため、透過モード、マスクモードはスキップされます。");
            }

            processor.ProcessAllPixels(sourcePixels, sourcePixels);
            return skipped;
        }

        if (settings.TransparentMode)
        {
            if (settings.InverseMode)
            {
                processor.ProcessTransparentAndInversePixels(sourcePixels, selection);
            }
            else
            {
                processor.ProcessSelectedPixels(sourcePixels, transparentLock!.Pixels, selection);
            }
        }
        else if (settings.MaskMode)
        {
            ImageProcessor.ProcessMaskImage(sourcePixels, selection, settings.InverseMode);
        }
        else if (settings.InverseMode)
        {
            processor.ProcessAllPixels(sourcePixels, sourcePixels);
            ImageProcessor.ProcessInverseSelectedPixels(sourcePixels, rawLock!.Pixels, selection);
        }
        else
        {
            processor.ProcessSelectedPixels(sourcePixels, sourcePixels, selection);
        }

        return false;
    }
}
