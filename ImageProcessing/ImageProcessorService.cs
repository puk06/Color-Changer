using ColorChanger.Models;
using ColorChanger.Utils;
using System.Collections;
using System.Drawing.Imaging;

namespace ColorChanger.ImageProcessing;

internal class ImageProcessorService
{
    private static readonly ColorPixel PreviewOutlineColor = new ColorPixel(255, 0, 0, 255);

    /// <summary>
    /// プレビュー画像を生成する
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <param name="colorDifference"></param>
    /// <param name="balanceMode"></param>
    /// <param name="configuration"></param>
    /// <param name="selectedPoints"></param>
    /// <param name="previewBoxSize"></param>
    /// <param name="rawMode"></param>
    /// <returns></returns>
    internal static Bitmap GeneratePreview(
        Bitmap sourceBitmap,
        ColorDifference colorDifference,
        bool balanceMode, BalanceModeConfiguration configuration,
        AdvancedColorConfiguration advancedColorConfiguration,
        BitArray selectedPoints,
        Size previewBoxSize,
        bool rawMode = false
    )
    {
        int boxHeight = previewBoxSize.Height;
        int boxWidth = previewBoxSize.Width;

        var previewBitmap = new Bitmap(boxWidth, boxHeight, PixelFormat.Format32bppArgb);

        Rectangle sourceRect = BitmapUtils.GetRectangle(sourceBitmap);
        BitmapData? sourceBitmapData = BitmapUtils.LockBitmap(sourceBitmap, sourceRect, ImageLockMode.ReadOnly);

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
                    return sourceBitmap;
                }

                ImageProcessor imageProcessor = new ImageProcessor(sourceBitmap.Size, colorDifference);

                if (rawMode)
                {
                    float ratioX = (float)sourceBitmap.Width / boxWidth;
                    float ratioY = (float)sourceBitmap.Height / boxHeight;
                    var ratios = (ratioX, ratioY);

                    imageProcessor.ProcessAllPreviewPixelsWithoutAdjustment(sourcePixels, previewPixels, ratios, previewBoxSize);
                    return previewBitmap;
                }

                if (balanceMode)
                    imageProcessor.SetBalanceSettings(configuration);

                if (advancedColorConfiguration.Enabled)
                    imageProcessor.SetColorSettings(advancedColorConfiguration);

                imageProcessor.ProcessAllPixels(sourcePixels, previewPixels);

                if (selectedPoints.Length != 0)
                {
                    BitArray innerAreaMask = BitmapUtils.RemoveInnerSelectedArea(
                        selectedPoints,
                        previewBoxSize.Width,
                        previewBoxSize.Height
                    );

                    ImageProcessor.ChangeSelectedPixelsColor(previewPixels, innerAreaMask, PreviewOutlineColor);
                }
            }
        }
        finally
        {
            if (sourceBitmapData != null) sourceBitmap.UnlockBits(sourceBitmapData);
            if (previewBitmapData != null) previewBitmap.UnlockBits(previewBitmapData);
        }

        return previewBitmap;
    }
}
