namespace VRC_Color_Changer.Classes
{
    public class Helper
    {
        public static (int x, int y)[] GetSelectedArea(Point clickedLocation, Bitmap rawImage, Color backgroundColor)
        {
            var selectedPoints = new HashSet<(int x, int y)>(); // 探索済みの点を保持（重複防止）
            var queue = new Queue<(int x, int y)>(); // 探索用のキュー

            int width = rawImage.Width;
            int height = rawImage.Height;

            // クリック地点の色を取得
            Color targetColor = rawImage.GetPixel(clickedLocation.X, clickedLocation.Y);

            // もしクリック地点が背景色と同じなら何も選択しない
            if (targetColor == backgroundColor)
                return Array.Empty<(int x, int y)>();

            // 探索開始
            queue.Enqueue((clickedLocation.X, clickedLocation.Y));
            selectedPoints.Add((clickedLocation.X, clickedLocation.Y));

            // 4方向移動用のオフセット
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                // 4方向をチェック
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue; // 範囲外
                    if (selectedPoints.Contains((nx, ny))) continue; // すでに探索済み
                    if (rawImage.GetPixel(nx, ny) != backgroundColor)
                    {
                        selectedPoints.Add((nx, ny));
                        queue.Enqueue((nx, ny)); // 次の探索対象としてキューに追加
                    }
                }
            }

            var outerPoints = new HashSet<(int x, int y)>();
            foreach (var (x, y) in selectedPoints)
            {
                if (selectedPoints.Contains((x - 1, y)) && selectedPoints.Contains((x + 1, y)) && selectedPoints.Contains((x, y - 1)) && selectedPoints.Contains((x, y + 1)))
                {
                    outerPoints.Add((x, y));
                }
            }

            return outerPoints.ToArray();
        }

        public static (int x, int y)[] ConvertSelectedAreaToPreviewBox((int x, int y)[] selectedArea, Bitmap rawImage, PictureBox previewBox)
        {
            int boxHeight = previewBox.Height;
            int boxWidth = previewBox.Width;
            float ratioX = (float)rawImage.Width / boxWidth;
            float ratioY = (float)rawImage.Height / boxHeight;
            var tempArea = selectedArea.Select(point => ((int)(point.x / ratioX), (int)(point.y / ratioY))).ToArray();
            return DeleteInnerSelectedArea(tempArea);
        }

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
    }
}
