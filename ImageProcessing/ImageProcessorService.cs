using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Drawing.Imaging;

namespace ColorChanger.ImageProcessing;

internal static class ImageProcessorService
{
    private static readonly ColorPixel PreviewOutlineColor = new ColorPixel(255, 0, 0, 255);

    /// <summary>
    /// プレビュー画像を生成する
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    internal static Bitmap GeneratePreview(PreviewConfiguration configuration)
    {
        int boxHeight = configuration.PreviewBoxSize.Height;
        int boxWidth = configuration.PreviewBoxSize.Width;

        Bitmap previewBitmap = new Bitmap(boxWidth, boxHeight, PixelFormat.Format32bppArgb);

        Rectangle sourceRect = BitmapUtils.GetRectangle(configuration.SourceBitmap);
        BitmapData? sourceBitmapData = BitmapUtils.LockBitmap(configuration.SourceBitmap, sourceRect, ImageLockMode.ReadOnly);

        Rectangle previewRect = BitmapUtils.GetRectangle(previewBitmap);
        BitmapData? previewBitmapData = BitmapUtils.LockBitmap(previewBitmap, previewRect, ImageLockMode.WriteOnly);

        try
        {
            unsafe
            {
                Span<ColorPixel> sourcePixels = BitmapUtils.GetPixelSpan(sourceBitmapData);
                Span<ColorPixel> previewPixels = BitmapUtils.GetPixelSpan(previewBitmapData);

                if (sourcePixels.IsEmpty || previewPixels.IsEmpty)
                {
                    FormUtils.ShowError("画像の読み込みに失敗しました。");
                    return configuration.SourceBitmap;
                }

                ImageProcessor imageProcessor = new ImageProcessor(configuration.SourceBitmap.Size, configuration.ColorDifference);

                if (configuration.RawMode)
                {
                    float ratioX = (float)configuration.SourceBitmap.Width / boxWidth;
                    float ratioY = (float)configuration.SourceBitmap.Height / boxHeight;
                    var ratios = (ratioX, ratioY);

                    imageProcessor.ProcessAllPreviewPixelsWithoutAdjustment(sourcePixels, previewPixels, ratios, configuration.PreviewBoxSize);
                    return previewBitmap;
                }

                if (configuration.BalanceMode)
                    imageProcessor.SetBalanceSettings(configuration.BalanceModeConfiguration);

                if (configuration.AdvancedColorConfiguration.Enabled)
                    imageProcessor.SetColorSettings(configuration.AdvancedColorConfiguration);

                imageProcessor.ProcessAllPixels(sourcePixels, previewPixels);

                if (configuration.SelectedPoints.Length != 0)
                {
                    BitArray innerAreaMask = BitmapUtils.RemoveInnerSelectedArea(
                        configuration.SelectedPoints,
                        configuration.PreviewBoxSize.Width,
                        configuration.PreviewBoxSize.Height
                    );

                    ImageProcessor.ChangeSelectedPixelsColor(previewPixels, innerAreaMask, PreviewOutlineColor);
                }
            }
        }
        finally
        {
            if (sourceBitmapData != null) configuration.SourceBitmap.UnlockBits(sourceBitmapData);
            if (previewBitmapData != null) previewBitmap.UnlockBits(previewBitmapData);
        }

        return previewBitmap;
    }
}
