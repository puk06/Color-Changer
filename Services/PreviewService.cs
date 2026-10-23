using System.Collections;
using System.Drawing.Imaging;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// プレビュー用の画像を生成する
/// </summary>
internal static class PreviewService
{
    private static readonly ColorPixel OutlineColor = new ColorPixel(255, 0, 0, 255);

    /// <summary>
    /// 元画像をプレビューサイズに縮小した、色変換なしの画像を生成する
    /// </summary>
    internal static Bitmap CreateRawPreview(Bitmap source, Size previewSize)
    {
        Bitmap preview = new Bitmap(previewSize.Width, previewSize.Height, PixelFormat.Format32bppArgb);

        using PixelLock sourceLock = new PixelLock(source, ImageLockMode.ReadOnly);
        using PixelLock previewLock = new PixelLock(preview, ImageLockMode.WriteOnly);

        Downscale(sourceLock.Pixels, source.Size, previewLock.Pixels, previewSize);

        return preview;
    }

    /// <summary>
    /// プレビュー用の色変換済み画像を生成する
    /// </summary>
    /// <param name="rawPreview">CreateRawPreview で作成した画像</param>
    /// <param name="previewSize">生成する画像のサイズ</param>
    /// <param name="settings">色変換の設定</param>
    /// <param name="previewSelection">プレビュー座標系での選択エリア</param>
    internal static Bitmap Generate(Bitmap rawPreview, Size previewSize, ConversionSettings settings, BitArray previewSelection)
    {
        Bitmap preview = new Bitmap(previewSize.Width, previewSize.Height, PixelFormat.Format32bppArgb);

        using PixelLock sourceLock = new PixelLock(rawPreview, ImageLockMode.ReadOnly);
        using PixelLock previewLock = new PixelLock(preview, ImageLockMode.WriteOnly);

        ImageProcessor processor = new ImageProcessor(settings.ColorDifference, settings.Balance, settings.AdvancedColor);
        processor.ProcessAllPixels(sourceLock.Pixels, previewLock.Pixels);

        if (previewSelection.Length != 0)
        {
            BitArray outline = SelectionService.CreateOverlayMask(previewSelection, previewSize.Width, previewSize.Height);
            ImageProcessor.ChangeSelectedPixelsColor(previewLock.Pixels, outline, OutlineColor);
        }

        return preview;
    }

    private static void Downscale(Span<ColorPixel> source, Size sourceSize, Span<ColorPixel> target, Size targetSize)
    {
        int sourceLength = source.Length;
        int targetLength = target.Length;

        float ratioX = (float)sourceSize.Width / targetSize.Width;
        float ratioY = (float)sourceSize.Height / targetSize.Height;

        for (int y = 0; y < targetSize.Height; y++)
        {
            int sourceY = (int)(y * ratioY);
            int targetRowOffset = y * targetSize.Width;

            for (int x = 0; x < targetSize.Width; x++)
            {
                int sourceX = (int)(x * ratioX);
                int sourceIndex = (sourceY * sourceSize.Width) + sourceX;
                int targetIndex = targetRowOffset + x;

                if (sourceIndex < 0 || sourceIndex >= sourceLength) continue;
                if (targetIndex < 0 || targetIndex >= targetLength) continue;

                target[targetIndex] = source[sourceIndex];
            }
        }
    }
}
