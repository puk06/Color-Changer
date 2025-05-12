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
    /// <returns></returns>
    public static Bitmap GeneratePreview(
        Bitmap sourceBitmap,
        ColorDifference colorDifference,
        bool balanceMode, BalanceModeConfiguration configuration,
        BitArray selectedPoints,
        Size previewBoxSize)
    {
        int boxHeight = previewBoxSize.Height;
        int boxWidth = previewBoxSize.Width;

        float ratioX = (float)sourceBitmap.Width / boxWidth;
        float ratioY = (float)sourceBitmap.Height / boxHeight;
        var ratios = (ratioX, ratioY);

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

                if (balanceMode)
                    imageProcessor.SetBalanceSettings(configuration);

                imageProcessor.ProcessAllPreviewPixels(sourcePixels, previewPixels, ratios, previewBoxSize);

                if (selectedPoints.Length != 0)
                {
                    ImageProcessor.ChangeSelectedPixelsColor(previewPixels, selectedPoints, PreviewOutlineColor);
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
