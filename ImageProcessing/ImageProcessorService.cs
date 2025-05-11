using ColorChanger.Models;
using ColorChanger.Utils;
using System.Drawing.Imaging;

namespace ColorChanger.ImageProcessing;

internal class ImageProcessorService
{
    /// <summary>
    /// プレビュー画像を生成する
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <param name="colorDifference"></param>
    /// <param name="balanceMode"></param>
    /// <param name="configuration"></param>
    /// <param name="selectedPointsArrayForPreview"></param>
    /// <param name="previewBoxSize"></param>
    /// <returns></returns>
    public static Bitmap GeneratePreview(
        Bitmap sourceBitmap,
        ColorDifference colorDifference,
        bool balanceMode, BalanceModeConfiguration configuration,
        (int x, int y)[][]? selectedPointsArrayForPreview,
        Size previewBoxSize)
    {
        int boxHeight = previewBoxSize.Height;
        int boxWidth = previewBoxSize.Width;

        float ratioX = (float)sourceBitmap.Width / boxWidth;
        float ratioY = (float)sourceBitmap.Height / boxHeight;

        var ratios = (ratioX, ratioY);

        Bitmap previewBitmap = new Bitmap(boxWidth, boxHeight, PixelFormat.Format32bppArgb);

        var sourceRect = BitmapUtils.GetRectangle(sourceBitmap);
        var sourceBitmapData = sourceBitmap.LockBits(sourceRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        var previewRect = BitmapUtils.GetRectangle(previewBitmap);
        var previewBitmapData = previewBitmap.LockBits(previewRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            unsafe
            {
                var sourcePixels = new Span<ColorPixel>((void*)sourceBitmapData.Scan0, BitmapUtils.GetSpanLength(sourceBitmapData));
                var previewPixels = new Span<ColorPixel>((void*)previewBitmapData.Scan0, BitmapUtils.GetSpanLength(previewBitmapData));

                ImageProcessor imageProcessor = new ImageProcessor(sourceBitmap.Size, colorDifference);

                if (balanceMode)
                    imageProcessor.SetBalanceSettings(configuration);

                imageProcessor.ProcessAllPreviewPixels(sourcePixels, previewPixels, ratios, previewBoxSize);

                if (selectedPointsArrayForPreview != null)
                {
                    ImageProcessor.ChangeSelectedPixelsColor(previewPixels, boxWidth, selectedPointsArrayForPreview, new ColorPixel(255, 0, 0, 255));
                }
            }
        }
        finally
        {
            sourceBitmap.UnlockBits(sourceBitmapData);
            previewBitmap.UnlockBits(previewBitmapData);
        }

        return previewBitmap;
    }
}
