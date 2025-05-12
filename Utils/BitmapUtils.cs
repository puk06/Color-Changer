using System.Collections;
using System.Drawing.Imaging;

namespace ColorChanger.Utils;

internal class BitmapUtils
{
    const int COLOR_PIXEL_SIZE = 4; // 32ビットARGB形式のピクセルサイズ

    /// <summary>
    /// 選択範囲を取得する
    /// </summary>
    /// <param name="clickedLocation"></param>
    /// <param name="rawImage"></param>
    /// <param name="backgroundColor"></param>
    /// <returns></returns>
    internal static unsafe BitArray GetSelectedArea(BitArray selectedPoints, Point clickedLocation, Bitmap rawImage, Color backgroundColor)
    {
        int width = rawImage.Width;
        int height = rawImage.Height;
        int totalPixels = width * height;

        BitArray selected = selectedPoints.Length == totalPixels
            ? selectedPoints
            : new(totalPixels, false);

        var queue = new Queue<PixelPoint>();

        var rect = GetRectangle(rawImage);
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
                return BitArrayUtils.GetEmpty();
            }

            int startIndex = PixelUtils.GetPixelIndex(startX, startY, width);
            ColorPixel targetPixel = pixels[startIndex];

            if (targetPixel.Equals(backgroundColor))
            {
                return BitArrayUtils.GetEmpty();
            }

            selected[startIndex] = true;
            queue.Enqueue(new PixelPoint(startX, startY));

            ReadOnlySpan<int> dx = [-1, 1, 0, 0];
            ReadOnlySpan<int> dy = [0, 0, -1, 1];

            while (queue.Count > 0)
            {
                var point = queue.Dequeue();
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
    /// <returns></returns>
    internal static BitArray ConvertSelectedAreaToPreviewBox(BitArray selectedArea, Bitmap sourceImage, PictureBox previewBox)
    {
        int previewHeight = previewBox.Height;
        int previewWidth = previewBox.Width;
        int totalPixels = previewWidth * previewHeight;

        float ratioX = (float)sourceImage.Width / previewWidth;
        float ratioY = (float)sourceImage.Height / previewHeight;

        BitArray bitArray = new BitArray(totalPixels, false);

        for (int y = 0; y < previewHeight; y++)
        {
            for (int x = 0; x < previewWidth; x++)
            {
                int originalX = (int)(x * ratioX);
                int originalY = (int)(y * ratioY);
                if (originalX >= sourceImage.Width || originalY >= sourceImage.Height) continue;

                int originalIndex = PixelUtils.GetPixelIndex(originalX, originalY, sourceImage.Width);
                if (selectedArea[originalIndex])
                {
                    int previewIndex = PixelUtils.GetPixelIndex(x, y, previewWidth);
                    bitArray[previewIndex] = true;
                }
            }
        }

        BitArray outerSelectedArea = DeleteInnerSelectedArea(bitArray, previewWidth, previewHeight);

        return outerSelectedArea;
    }

    /// <summary>
    /// 選択範囲の内側の点を削除する
    /// </summary>
    /// <param name="selectedArea"></param>
    /// <returns></returns>
    private static BitArray DeleteInnerSelectedArea(BitArray selectedArea, int width, int height)
    {
        BitArray result = new BitArray(selectedArea.Length, false);

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int index = PixelUtils.GetPixelIndex(x, y, width);

                if (!selectedArea[index]) continue;

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

                bool isInner =
                    selectedArea[upperIndex] &&
                    selectedArea[lowerIndex] &&
                    selectedArea[leftIndex] &&
                    selectedArea[rightIndex];

                if (!isInner) continue;
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
        var size = bitmap.Size;
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
    internal static void SetImage(PictureBox pictureBox, Bitmap bitmapImage, bool disposeImage = true)
    {
        ResetImage(pictureBox, false);
        pictureBox.Image = disposeImage ? new Bitmap(bitmapImage) : bitmapImage;
        pictureBox.Invalidate();
        if (disposeImage) bitmapImage.Dispose();
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
    /// <returns></returns>
    internal static BitmapData? LockBitmap(Bitmap? bmp, Rectangle rect, int mode)
    {
        ImageLockMode lockMode = mode switch
        {
            1 => ImageLockMode.ReadOnly,
            2 => ImageLockMode.WriteOnly,
            3 => ImageLockMode.ReadWrite,
            _ => ImageLockMode.ReadWrite
        };

        return bmp?.LockBits(rect, lockMode, PixelFormat.Format32bppArgb);
    }

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
        return inverseMode ? new Bitmap(bitmap) : null;
    }

    /// <summary>
    /// 透明モード用のBitmapをコピーを生成する
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="transMode"></param>
    /// <param name="InverseMode"></param>
    /// <returns></returns>
    internal static Bitmap? CreateTransparentBitmap(Bitmap bitmap, bool transMode, bool InverseMode)
    {
        if (bitmap == null) return null;
        if (transMode && !InverseMode)
        {
            var bmp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            return bmp;
        }

        return null;
    }
}
