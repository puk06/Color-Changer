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
        List<string> notices = new List<string>();

        Bitmap output = ImageFileService.CloneAsArgb32(source);
        Bitmap? rawCopy = settings.InverseMode ? ImageFileService.CloneAsArgb32(source) : null;
        Bitmap? transparent = settings.TransparentMode && !settings.InverseMode && selection.Length != 0
            ? CreateTransparentLayer(source.Size)
            : null;

        Bitmap? result = null;
        try
        {
            bool skipped = ProcessPixels(output, rawCopy, transparent, settings, selection, notices);

            bool useTransparentLayer = transparent != null && !skipped;
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
        Bitmap layer = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(layer);
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
        ImageProcessor processor = new ImageProcessor(settings.ColorDifference, settings.Balance, settings.AdvancedColor);

        using PixelLock outputLock = new PixelLock(output, ImageLockMode.ReadWrite);
        using PixelLock? rawLock = rawCopy == null ? null : new PixelLock(rawCopy, ImageLockMode.ReadOnly);
        using PixelLock? transparentLock = transparent == null ? null : new PixelLock(transparent, ImageLockMode.ReadWrite);

        Span<ColorPixel> sourcePixels = outputLock.Pixels;

        if (selection.Length == 0)
        {
            bool skipped = settings.TransparentMode;
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
