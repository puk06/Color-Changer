using System.Drawing.Imaging;

namespace ColorChanger.Utils
{
    internal class BitmapUtils
    {
        /// <summary>
        /// 選択範囲を取得する
        /// </summary>
        /// <param name="clickedLocation"></param>
        /// <param name="rawImage"></param>
        /// <param name="backgroundColor"></param>
        /// <returns></returns>
        internal static unsafe (int x, int y)[] GetSelectedArea(Point clickedLocation, Bitmap rawImage, Color backgroundColor)
        {
            var selectedPoints = new HashSet<(int x, int y)>();
            var queue = new Queue<(int x, int y)>();

            int width = rawImage.Width;
            int height = rawImage.Height;

            var rawImageRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = rawImage.LockBits(rawImageRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int stride = bmpData.Stride;
            byte* scan0 = (byte*)bmpData.Scan0;

            // 指定位置の色取得（クリック地点）
            int startX = clickedLocation.X;
            int startY = clickedLocation.Y;
            byte* startPixel = scan0 + startY * stride + startX * 4;

            byte targetB = startPixel[0];
            byte targetG = startPixel[1];
            byte targetR = startPixel[2];
            byte targetA = startPixel[3];

            // 背景色と一致していたらスキップ
            if (targetR == backgroundColor.R &&
                targetG == backgroundColor.G &&
                targetB == backgroundColor.B &&
                targetA == backgroundColor.A)
            {
                rawImage.UnlockBits(bmpData);
                return Array.Empty<(int x, int y)>();
            }

            queue.Enqueue((startX, startY));
            selectedPoints.Add((startX, startY));

            int[] dx = [-1, 1, 0, 0];
            int[] dy = [0, 0, -1, 1];

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (selectedPoints.Contains((nx, ny))) continue;

                    byte* p = scan0 + ny * stride + nx * 4;
                    byte b = p[0], g = p[1], r = p[2], a = p[3];

                    // 背景色と違うなら追加
                    if (!(r == backgroundColor.R && g == backgroundColor.G && b == backgroundColor.B && a == backgroundColor.A))
                    {
                        selectedPoints.Add((nx, ny));
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            // 外周点を抽出（内側に全方向に隣接がある点）
            var outerPoints = new HashSet<(int x, int y)>();
            foreach (var (x, y) in selectedPoints)
            {
                if (selectedPoints.Contains((x - 1, y)) &&
                    selectedPoints.Contains((x + 1, y)) &&
                    selectedPoints.Contains((x, y - 1)) &&
                    selectedPoints.Contains((x, y + 1)))
                {
                    outerPoints.Add((x, y));
                }
            }

            rawImage.UnlockBits(bmpData);
            return outerPoints.ToArray();
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
            var selectedPoints = new HashSet<(int x, int y)>(selectedArea);
            var innerPoints = new HashSet<(int x, int y)>();

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
        internal static bool IsValidCoordinate(Point coords, Size size) => coords.X >= 0 && coords.X < size.Width && coords.Y >= 0 && coords.Y < size.Height;

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
    }
}
