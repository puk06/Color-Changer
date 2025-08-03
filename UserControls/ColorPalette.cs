using ColorChanger.Utils;
using System.Diagnostics;

namespace ColorChanger.UserControls;

internal partial class ColorPalette : UserControl
{
    private long _colorUpdateDebounceTime = 14;

    internal event EventHandler<Color>? ColorSelected;

    private readonly Stopwatch _updateDebounceStopwatch = Stopwatch.StartNew();
    private int _clickedHueY = 1;
    private Point _clickedPoint = Point.Empty;
    private Color _selectedColor = Color.Empty;

    internal ColorPalette()
    {
        InitializeComponent();

        hueSlider.Image = GenerateHueSlider(hueSlider.Size);
        colorMap.Image = GenerateColorGradient(CalculateHueColor(_clickedHueY, hueSlider.Height), colorMap.Width, colorMap.Height);
    }

    /// <summary>
    /// 渡された色をカラーパレット内で選択します。
    /// </summary>
    /// <param name="target"></param>
    internal void SelectColor(Color target)
    {
        ColorUtils.RGBtoHSV(target, out double targetHue, out double _, out double _);

        int hueY = (int)((targetHue / 360.0) * (hueSlider.Height - 1));
        hueY = Math.Clamp(hueY, 0, hueSlider.Height - 1);

        _clickedHueY = hueY;
        UpdateBitmapImage(_clickedHueY);

        if (colorMap.Image is not Bitmap colorMapBitmap) return;
        _clickedPoint = BitmapUtils.GetClosestPoint(colorMapBitmap, target);

        colorMap.Invalidate();
        hueSlider.Invalidate();
    }

    #region 色選択関連
    private void HandleHueColorSelection(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (_updateDebounceStopwatch.ElapsedMilliseconds <= _colorUpdateDebounceTime) return;
        _updateDebounceStopwatch.Restart();

        _clickedHueY = e.Y;
        UpdateBitmapImage(_clickedHueY);

        if (_clickedPoint != Point.Empty) UpdateColor(_clickedPoint, false);
        hueSlider.Invalidate();
    }

    private void HandleColorMapSelection(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (_updateDebounceStopwatch.ElapsedMilliseconds <= _colorUpdateDebounceTime) return;
        _updateDebounceStopwatch.Restart();

        UpdateColor(e.Location);
    }

    private void UpdateColor(Point pointLocation, bool updateLocation = true)
    {
        if (colorMap.Image is not Bitmap colorMapBitmap) return;
        if (pointLocation.X < 0 || pointLocation.Y < 0 || pointLocation.X >= colorMapBitmap.Width || pointLocation.Y >= colorMapBitmap.Height) return;

        _selectedColor = colorMapBitmap.GetPixel(pointLocation.X, pointLocation.Y);
        if (updateLocation) _clickedPoint = pointLocation;
        ColorSelected?.Invoke(this, _selectedColor);

        colorMap.Invalidate();
    }
    #endregion

    #region カラーパレット生成関連
    private static Bitmap GenerateHueSlider(Size size)
    {
        Bitmap bmp = new Bitmap(size.Width, size.Height);

        for (int y = 0; y < size.Height; y++)
        {
            Color color = CalculateHueColor(y, size.Height);

            for (int x = 0; x < size.Width; x++)
            {
                bmp.SetPixel(x, y, color);
            }
        }

        return bmp;
    }

    private static Bitmap GenerateColorGradient(Color targetColor, int width, int height)
    {
        Bitmap bmp = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            float v = 1.0f - ((float)y / (height - 1));
            for (int x = 0; x < width; x++)
            {
                float s = (float)x / (width - 1);
                Color mixed = ColorUtils.InterpolateColor(targetColor, s, v);
                bmp.SetPixel(x, y, mixed);
            }
        }

        return bmp;
    }

    private void UpdateBitmapImage(int hueY)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Color hueColor = CalculateHueColor(hueY, hueSlider.Height);

        colorMap.Image?.Dispose();
        colorMap.Image = null;
        colorMap.Image = GenerateColorGradient(hueColor, colorMap.Width, colorMap.Height);

        _colorUpdateDebounceTime = stopwatch.ElapsedMilliseconds + 10;
    }
    #endregion

    #region イベントハンドラー
    private void ColorMap_Paint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;

        Color inverseColor = ColorUtils.InverseColor(_selectedColor);

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
        e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
    }

    private void HueSlider_Paint(object sender, PaintEventArgs e)
    {
        Color inverseColor = ColorUtils.InverseColor(CalculateHueColor(_clickedHueY, hueSlider.Height));

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, 0, _clickedHueY, hueSlider.Width, _clickedHueY);
    }
    #endregion

    #region 内部計算
    private static Color CalculateHueColor(int y, int barHeight)
    {
        double saturation = (double)y / (barHeight - 1);
        double hue = saturation * 360.0;

        return ColorUtils.ColorFromHSV(hue, 1.0f, 1.0f);
    }
    #endregion
}
