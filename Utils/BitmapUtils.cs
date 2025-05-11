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
    internal static unsafe (int x, int y)[] GetSelectedArea(Point clickedLocation, Bitmap rawImage, Color backgroundColor)
    {
        int width = rawImage.Width;
        int height = rawImage.Height;
        int totalPixels = width * height;

        var selected = new BitArray(totalPixels);
        var queue = new Queue<PixelPoint>();

        var rect = new Rectangle(0, 0, width, height);
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
                return [];
            }

            int startIndex = PixelUtils.GetPixelIndex(startX, startY, width);
            ColorPixel targetPixel = pixels[startIndex];

            if (targetPixel.Equals(backgroundColor))
            {
                return [];
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

            var innerPoints = new List<(int x, int y)>();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = PixelUtils.GetPixelIndex(x, y, width);
                    if (!selected[index]) continue;

                    if (selected[PixelUtils.GetPixelIndex(x, y - 1, width)] &&
                        selected[PixelUtils.GetPixelIndex(x, y + 1, width)] &&
                        selected[PixelUtils.GetPixelIndex(x - 1, y, width)] &&
                        selected[PixelUtils.GetPixelIndex(x + 1, y, width)])
                    {
                        innerPoints.Add((x, y));
                    }
                }
            }

            return innerPoints.ToArray();
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
    internal static (int x, int y)[] ConvertSelectedAreaToPreviewBox((int x, int y)[] selectedArea, Bitmap sourceImage, PictureBox previewBox)
    {
        int previewHeight = previewBox.Height;
        int previewWidth = previewBox.Width;

        float ratioX = (float)sourceImage.Width / previewWidth;
        float ratioY = (float)sourceImage.Height / previewHeight;

        var scaledSelectedArea = selectedArea.Select(point => ((int)(point.x / ratioX), (int)(point.y / ratioY))).ToArray();
        return DeleteInnerSelectedArea(scaledSelectedArea);
    }

    /// <summary>
    /// 選択範囲の内側の点を削除する
    /// </summary>
    /// <param name="selectedArea"></param>
    /// <returns></returns>
    private static (int x, int y)[] DeleteInnerSelectedArea((int x, int y)[] selectedArea)
    {
        HashSet<(int x, int y)> selectedPoints = new(selectedArea);
        HashSet<(int x, int y)> innerPoints = new();

        foreach (var (x, y) in selectedArea)
        {
            if (selectedPoints.Contains((x - 2, y)) && selectedPoints.Contains((x + 2, y)) && selectedPoints.Contains((x, y - 2)) && selectedPoints.Contains((x, y + 2)))
            {
                innerPoints.Add((x, y));
            }
        }

        return selectedArea.Except(innerPoints).ToArray();
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
        pictureBox.Image = new Bitmap(bitmapImage);
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
    internal static int GetSpanLength(BitmapData? bitmapData)
    {
        if (bitmapData == null) return 0;
        return (bitmapData.Stride * bitmapData.Height) / COLOR_PIXEL_SIZE;
    }
}
