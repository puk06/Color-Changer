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
            int[] dx = [-1, 1, 0, 0];
            int[] dy = [0, 0, -1, 1];

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

        public static double CalculateWeight(double MinDistance, double distance, double graphWeight)
        {
            if (MinDistance == -1) return 1; // 交差しない場合は重みを1にする (同じ色のときに現れる)
            return Math.Pow(1 - (distance / MinDistance), graphWeight);
        }

        public static double CalculateMinDistance(int base_r, int base_g, int base_b, int target_r, int target_g, int target_b)
        {
            // 方向ベクトル
            int dx = target_r - base_r;
            int dy = target_g - base_g;
            int dz = target_b - base_b;

            // 無限線分を RGB 空間の壁に交差する点まで伸ばす（各軸で）
            List<double> t_values = new List<double>();

            // 各チャンネルの0と255の壁について、t値（ベクトル方向に進むスカラー量）を求める
            if (dx != 0)
            {
                t_values.Add((0 - base_r) / (double)dx);
                t_values.Add((255 - base_r) / (double)dx);
            }
            if (dy != 0)
            {
                t_values.Add((0 - base_g) / (double)dy);
                t_values.Add((255 - base_g) / (double)dy);
            }
            if (dz != 0)
            {
                t_values.Add((0 - base_b) / (double)dz);
                t_values.Add((255 - base_b) / (double)dz);
            }

            // 最小正の t を探す（延長線上、前方方向）
            double minPositiveT = double.MaxValue;
            foreach (var t in t_values)
            {
                if (t > 0)
                {
                    double x = base_r + t * dx;
                    double y = base_g + t * dy;
                    double z = base_b + t * dz;

                    // 点がRGB空間内にあるか（各成分が0〜255の間）
                    if (x >= 0 && x <= 255 && y >= 0 && y <= 255 && z >= 0 && z <= 255)
                    {
                        if (t < minPositiveT)
                            minPositiveT = t;
                    }
                }
            }

            // 最短距離 = ベクトルの長さ * t
            if (minPositiveT != double.MaxValue)
            {
                double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                return minPositiveT * length;
            }

            // 交差しない場合 (同じ色のときに現れる)
            return -1;
        }

        public static bool IsValidCoordinate(Point coords, Size size)
        {
            return coords.X >= 0 && coords.X < size.Width && coords.Y >= 0 && coords.Y < size.Height;
        }

        public static int ParseAndClamp(string value)
        {
            return Math.Min(255, Math.Max(0, int.TryParse(value, out int result) ? result : 0));
        }


        public static string GetColorCodeFromColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color GetColorFromColorCode(string colorCode)
        {
            try
            {
                if (colorCode.Length != 7) return Color.Empty;
                return ColorTranslator.FromHtml(colorCode);
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static Point GetOriginalCoordinates(Point clickedPoint, Size originalSize, Size displaySize)
        {
            float ratioX = (float)originalSize.Width / displaySize.Width;
            float ratioY = (float)originalSize.Height / displaySize.Height;

            return new Point((int)(clickedPoint.X * ratioX), (int)(clickedPoint.Y * ratioY));
        }

        public static double GetColorDistance(Color color1, Color color2)
        {
            double r = Math.Pow(color1.R - color2.R, 2);
            double g = Math.Pow(color1.G - color2.G, 2);
            double b = Math.Pow(color1.B - color2.B, 2);
            return Math.Sqrt(r + g + b);
        }

        public static Point GetClosestColorPoint(Color color, Bitmap bmp)
        {
            Point closestPoint = Point.Empty;
            double closestDistance = double.MaxValue;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color currentColor = bmp.GetPixel(x, y);
                    double distance = GetColorDistance(color, currentColor);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = new Point(x, y);
                    }
                }
            }
            return closestPoint;
        }
    }
}
