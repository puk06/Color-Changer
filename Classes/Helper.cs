using System.Drawing.Imaging;

namespace ColorChanger.Classes;

public static class Helper
{
    /// <summary>
    /// 選択範囲を取得する
    /// </summary>
    /// <param name="clickedLocation"></param>
    /// <param name="rawImage"></param>
    /// <param name="backgroundColor"></param>
    /// <returns></returns>
    public static unsafe (int x, int y)[] GetSelectedArea(Point clickedLocation, Bitmap rawImage, Color backgroundColor)
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
    /// <param name="sourceImage"></param>
    /// <param name="previewBox"></param>
    /// <returns></returns>
    public static (int x, int y)[] ConvertSelectedAreaToPreviewBox((int x, int y)[] selectedArea, Bitmap sourceImage, PictureBox previewBox)
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
    /// 重みから色の変化率を計算する
    /// </summary>
    /// <param name="hasIntersection"></param>
    /// <param name="IntersectionDistance"></param>
    /// <param name="distance"></param>
    /// <param name="graphWeight"></param>
    /// <returns></returns>
    public static double CalculateColorChangeRate(bool hasIntersection, double IntersectionDistance, double distance, double graphWeight)
    {
        if (!hasIntersection || IntersectionDistance == 0) return 1; // 交差しない場合は変化率を1にする
        return Math.Pow(1 - (distance / IntersectionDistance), graphWeight);
    }

    /// <summary>
    /// RGB空間の無限線分と壁（0〜255）との交差点を求める
    /// </summary>
    /// <param name="base_r"></param>
    /// <param name="base_g"></param>
    /// <param name="base_b"></param>
    /// <param name="target_r"></param>
    /// <param name="target_g"></param>
    /// <param name="target_b"></param>
    /// <returns></returns>
    public static (bool hasIntersection, double IntersectionDistance) GetRGBIntersectionDistance(int base_r, int base_g, int base_b, int target_r, int target_g, int target_b)
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
            double length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            return (true, minPositiveT * length);
        }

        // 交差しない場合
        return (false, -1);
    }

    /// <summary>
    /// 座標が範囲内かどうかを判定する
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static bool IsValidCoordinate(Point coords, Size size)
    {
        return coords.X >= 0 && coords.X < size.Width && coords.Y >= 0 && coords.Y < size.Height;
    }

    /// <summary>
    /// 数値をパースして0〜255にクランプする
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int ParseAndClamp(string value)
    {
        return Math.Min(255, Math.Max(0, int.TryParse(value, out int result) ? result : 0));
    }

    /// <summary>
    /// RGB値からカラーコードを取得する
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static string GetColorCodeFromColor(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// カラーコードからRGB値を取得する
    /// </summary>
    /// <param name="colorCode"></param>
    /// <returns></returns>
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

    /// <summary>
    /// クリックした座標を元の画像の座標に変換する
    /// </summary>
    /// <param name="clickedPoint"></param>
    /// <param name="originalSize"></param>
    /// <param name="displaySize"></param>
    /// <returns></returns>
    public static Point GetOriginalCoordinates(Point clickedPoint, Size originalSize, Size displaySize)
    {
        float ratioX = (float)originalSize.Width / displaySize.Width;
        float ratioY = (float)originalSize.Height / displaySize.Height;

        return new Point((int)(clickedPoint.X * ratioX), (int)(clickedPoint.Y * ratioY));
    }

    /// <summary>
    /// 2つの色の距離を計算する
    /// </summary>
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns></returns>
    public static double GetColorDistance(Color color1, Color color2)
    {
        double r = Math.Pow(color1.R - color2.R, 2);
        double g = Math.Pow(color1.G - color2.G, 2);
        double b = Math.Pow(color1.B - color2.B, 2);

        return Math.Sqrt(r + g + b);
    }

    /// <summary>
    /// 指定した色に最も近い色の座標を取得する
    /// </summary>
    /// <param name="color"></param>
    /// <param name="bmp"></param>
    /// <returns></returns>
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