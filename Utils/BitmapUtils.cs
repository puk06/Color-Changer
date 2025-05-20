using System.Collections;
using System.Drawing.Imaging;

namespace ColorChanger.Utils;

internal class BitmapUtils
{
    private const int COLOR_PIXEL_SIZE = 4; // 32ビットARGB形式のピクセルサイズ

    /// <summary>
    /// 選択範囲を取得する
    /// </summary>
    /// <param name="clickedLocation"></param>
    /// <param name="rawImage"></param>
    /// <param name="backgroundColor"></param>
    /// <returns></returns>
    internal static unsafe BitArray? GetSelectedArea(Point clickedLocation, Bitmap rawImage, Color backgroundColor)
    {
        int width = rawImage.Width;
        int height = rawImage.Height;
        int totalPixels = width * height;

        BitArray selected = new BitArray(totalPixels, false);

        var queue = new Queue<PixelPoint>();

        Rectangle rect = GetRectangle(rawImage);
        BitmapData bmpData = rawImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            Span<ColorPixel> pixels = new(
                (void*)bmpData.Scan0,
                totalPixels
            );

            int startX = clickedLocation.X;
            int startY = clickedLocation.Y;

            if (startX < 0 || startY < 0 || startX >= width || startY >= height)
            {
                return null;
            }

            int startIndex = PixelUtils.GetPixelIndex(startX, startY, width);
            ColorPixel targetPixel = pixels[startIndex];

            if (targetPixel.Equals(backgroundColor))
            {
                return null;
            }

            selected[startIndex] = true;
            queue.Enqueue(new PixelPoint(startX, startY));

            ReadOnlySpan<int> dx = [-1, 1, 0, 0];
            ReadOnlySpan<int> dy = [0, 0, -1, 1];

            while (queue.Count > 0)
            {
                PixelPoint point = queue.Dequeue();
                var (x, y) = point;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    int nIndex = PixelUtils.GetPixelIndex(nx, ny, width);
                    if (selected[nIndex]) continue;

                    if (!pixels[nIndex].Equals(backgroundColor))
                    {
                        selected[nIndex] = true;
                        queue.Enqueue(new PixelPoint(nx, ny));
                    }
                }
            }

            return selected;
        }
        finally
        {
            rawImage.UnlockBits(bmpData);
        }
    }

    /// <summary>
    /// 選択範囲をプレビュー用の座標に変換する
    /// </summary>
    /// <param name="selectedArea"></param>
    /// <param name="sourceImage"></paramImageProcessing
    /// <param name="previewBox"></param>
    /// <param name="inverseMode"></param>
    /// <returns></returns>
    internal static BitArray ConvertSelectedAreaToPreviewBox(
        BitArray selectedArea,
        Bitmap sourceImage,
        PictureBox previewBox,
        bool inverseMode)
    {
        int previewHeight = previewBox.Height;
        int previewWidth = previewBox.Width;
        int totalPixels = previewWidth * previewHeight;

        float ratioX = (float)sourceImage.Width / previewWidth;
        float ratioY = (float)sourceImage.Height / previewHeight;

        BitArray bitArray = new BitArray(totalPixels, inverseMode);

        for (int y = 0; y < previewHeight; y++)
        {
            int sourceY = (int)(y * ratioY);
            if (sourceY >= sourceImage.Height)
                continue;

            int previewRowOffset = y * previewWidth;
            int sourceRowOffset = sourceY * sourceImage.Width;

            for (int x = 0; x < previewWidth; x++)
            {
                int sourceX = (int)(x * ratioX);
                if (sourceX >= sourceImage.Width)
                    continue;

                int sourceIndex = sourceRowOffset + sourceX;
                if (!selectedArea[sourceIndex])
                    continue;

                int previewIndex = previewRowOffset + x;
                bitArray[previewIndex] = !bitArray[previewIndex];
            }
        }

        return bitArray;
    }

    /// <summary>
    /// 選択範囲の内側の点を削除する
    /// </summary>
    /// <param name="selectedArea"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    internal static BitArray RemoveInnerSelectedArea(BitArray selectedArea, int width, int height)
    {
        BitArray result = new BitArray(selectedArea.Length);
        int stripeInterval = 6; // 斜め線の間隔（ピクセル数）

        // 内側のピクセル判定
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int index = PixelUtils.GetPixelIndex(x, y, width);

                int upperIndex = PixelUtils.GetPixelIndex(x, y - 2, width); // 上
                int lowerIndex = PixelUtils.GetPixelIndex(x, y + 2, width); // 下
                int leftIndex = PixelUtils.GetPixelIndex(x - 2, y, width);  // 左
                int rightIndex = PixelUtils.GetPixelIndex(x + 2, y, width); // 右

                if (!PixelUtils.IsValid(selectedArea, upperIndex) ||
                    !PixelUtils.IsValid(selectedArea, lowerIndex) ||
                    !PixelUtils.IsValid(selectedArea, leftIndex) ||
                    !PixelUtils.IsValid(selectedArea, rightIndex))
                {
                    continue;
                }

                if (!selectedArea[index])
                {
                    if ((x + y) % stripeInterval == 0) result[index] = true;
                    continue;
                }

                bool isInner =
                    selectedArea[upperIndex] &&
                    selectedArea[lowerIndex] &&
                    selectedArea[leftIndex] &&
                    selectedArea[rightIndex];

                if (isInner) continue;

                // 外周や端のピクセルはそのまま
                result[index] = true;
            }
        }

        return result;
    }

    /// <summary>
    /// 座標が範囲内かどうかを判定する
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    internal static bool IsValidCoordinate(Point coords, Size size)
        => coords.X >= 0 && coords.X < size.Width && coords.Y >= 0 && coords.Y < size.Height;

    /// <summary>
    /// クリックした座標を元の画像の座標に変換する
    /// </summary>
    /// <param name="clickedPoint"></param>
    /// <param name="originalSize"></param>
    /// <param name="displaySize"></param>
    /// <returns></returns>
    internal static Point GetOriginalCoordinates(Point clickedPoint, Size originalSize, Size displaySize)
    {
        float ratioX = (float)originalSize.Width / displaySize.Width;
        float ratioY = (float)originalSize.Height / displaySize.Height;

        return new Point((int)(clickedPoint.X * ratioX), (int)(clickedPoint.Y * ratioY));
    }

    /// <summary>
    /// BitmapのサイズからRectangleを取得する
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    internal static Rectangle GetRectangle(Bitmap bitmap)
    {
        Size size = bitmap.Size;
        return new Rectangle(0, 0, size.Width, size.Height);
    }

    /// <summary>
    /// BitmapをDisposeする
    /// </summary>
    /// <param name="bitmap"></param>
    internal static void DisposeBitmap(ref Bitmap? bitmap)
    {
        bitmap?.Dispose();
        bitmap = null;
    }

    /// <summary>
    /// BitmapをPictureBoxにセットする
    /// </summary>
    /// <param name="pictureBox"></param>
    /// <param name="bitmapImage"></param>
    /// <param name="disposeImage"></param>
    internal static void SetImage(PictureBox pictureBox, Bitmap bitmapImage, bool disposeImage = true)
    {
        try
        {
            ResetImage(pictureBox, invalidate: false);
            pictureBox.Image = disposeImage ? new Bitmap(bitmapImage) : bitmapImage;
            pictureBox.Invalidate();
            if (disposeImage) bitmapImage.Dispose();
        }
        catch (Exception ex)
        {
            FormUtils.ShowError($"画像の読み込みに失敗しました。\n{ex.Message}");
        }
    }

    /// <summary>
    /// BitmapをPictureBoxから削除する
    /// </summary>
    /// <param name="pictureBox"></param>
    /// <param name="invalidate"></param>
    internal static void ResetImage(PictureBox pictureBox, bool invalidate = true)
    {
        pictureBox.Image?.Dispose();
        pictureBox.Image = null;
        if (invalidate) pictureBox.Invalidate();
    }

    /// <summary>
    /// クリックした座標を元の画像の座標に変換する
    /// </summary>
    /// <param name="e"></param>
    /// <param name="pictureBox"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    internal static Point ConvertToOriginalCoordinates(MouseEventArgs e, PictureBox pictureBox, Bitmap image)
    {
        int x = e.X;
        int y = e.Y;

        float ratioX = (float)image.Width / pictureBox.Width;
        float ratioY = (float)image.Height / pictureBox.Height;

        return new Point((int)(x * ratioX), (int)(y * ratioY));
    }

    /// <summary>
    /// BitmapDataからピクセル数を取得する
    /// </summary>
    /// <param name="bitmapData"></param>
    /// <returns></returns>
    private static int GetSpanLength(BitmapData? bitmapData)
    {
        if (bitmapData == null) return 0;
        return (bitmapData.Stride * bitmapData.Height) / COLOR_PIXEL_SIZE;
    }

    /// <summary>
    /// Bitmapをロックする
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="rect"></param>
    /// <param name="imageLockMode"></param>
    /// <returns></returns>
    internal static BitmapData? LockBitmap(Bitmap? bmp, Rectangle rect, ImageLockMode imageLockMode)
        => bmp?.LockBits(rect, imageLockMode, PixelFormat.Format32bppArgb);

    /// <summary>
    /// BitmapのSpanを取得する
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    internal static unsafe Span<ColorPixel> GetPixelSpan(BitmapData? data)
    {
        return data != null
            ? new Span<ColorPixel>((void*)data.Scan0, GetSpanLength(data))
            : default;
    }

    /// <summary>
    /// 反転モード用のBitmapコピーを生成する
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="inverseMode"></param>
    /// <returns></returns>
    internal static Bitmap? CreateInverseBitmap(Bitmap? bitmap, bool inverseMode)
    {
        if (bitmap == null) return null;
        return inverseMode ? CopyBitmap(bitmap) : null;
    }

    /// <summary>
    /// 透明モード用のBitmapをコピーを生成する
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="transMode"></param>
    /// <param name="inverseMode"></param>
    /// <returns></returns>
    internal static Bitmap? CreateTransparentBitmap(Bitmap bitmap, bool transMode, bool inverseMode)
    {
        if (bitmap == null) return null;
        if (transMode && !inverseMode)
        {
            Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            return bmp;
        }

        return null;
    }

    /// <summary>
    /// 指定されたPixelFormatのビット数を取得する
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private static int GetBitsPerPixel(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.Format1bppIndexed => 1,
            PixelFormat.Format4bppIndexed => 4,
            PixelFormat.Format8bppIndexed => 8,
            PixelFormat.Format16bppGrayScale => 16,
            PixelFormat.Format16bppRgb555 => 16,
            PixelFormat.Format16bppRgb565 => 16,
            PixelFormat.Format16bppArgb1555 => 16,
            PixelFormat.Format24bppRgb => 24,
            PixelFormat.Format32bppRgb => 32,
            PixelFormat.Format32bppArgb => 32,
            PixelFormat.Format32bppPArgb => 32,
            PixelFormat.Format48bppRgb => 48,
            PixelFormat.Format64bppArgb => 64,
            PixelFormat.Format64bppPArgb => 64,
            _ => 0,// Unsupported format
        };
    }

    /// <summary>
    /// Bitmapのメモリ使用量を推定する
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    internal static double CalculateEstimatedMemoryUsage(Bitmap bitmap)
    {
        int bytesPerPixel = GetBitsPerPixel(bitmap.PixelFormat);

        int width = bitmap.Width;
        int height = bitmap.Height;

        long baseBytes = (long)width * height * bytesPerPixel / 8;

        double totalMB = baseBytes / 1024.0 / 1024.0;

        return totalMB;
    }

    /// <summary>
    /// Bitmapのコピーを生成する
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static unsafe Bitmap CopyBitmap(Bitmap source)
    {
        Rectangle rect = GetRectangle(source);
        Bitmap dest = new Bitmap(source.Width, source.Height, source.PixelFormat);

        BitmapData? srcData = null;
        BitmapData? dstData = null;

        try
        {
            srcData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            dstData = dest.LockBits(rect, ImageLockMode.WriteOnly, dest.PixelFormat);

            int height = source.Height;
            int stride = srcData.Stride;

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;

            for (int y = 0; y < height; y++)
            {
                Buffer.MemoryCopy(
                    srcPtr + (y * stride),
                    dstPtr + (y * stride),
                    stride, stride
                );
            }
        }
        finally
        {
            if (srcData != null) source.UnlockBits(srcData);
            if (dstData != null) dest.UnlockBits(dstData);
        }

        return dest;
    }
}
