using ColorChanger.Services;

namespace ColorChanger.Forms;

public partial class ColorPalette : UserControl
{
    public event EventHandler<Color>? ColorSelected;

    private readonly Throttle _updateThrottle = new Throttle(30);
    private int _clickedHueY = 1;
    private Color _hueColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;
    private Color _selectedColor = Color.Empty;

    public ColorPalette()
    {
        InitializeComponent();

        hueSlider.Image = GenerateHueSlider(hueSlider.Size);
        UpdateHueColor(1);
    }

    /// <summary>
    /// 渡された色をカラーパレット内で選択します。
    /// </summary>
    /// <param name="target"></param>
    public void SelectColor(Color target)
    {
        ColorService.RGBtoHSV(target, out double targetHue, out double _, out double _);

        int hueY = (int)((targetHue / 360.0) * (hueSlider.Height - 1));
        hueY = Math.Clamp(hueY, 0, hueSlider.Height - 1);

        UpdateHueColor(hueY);

        if (colorMap.Image is not Bitmap colorMapBitmap) return;
        _clickedPoint = ColorService.FindClosestPoint(colorMapBitmap, target);

        colorMap.Invalidate();
        hueSlider.Invalidate();
    }

    #region 色選択関連
    private void HandleHueColorSelection(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (!_updateThrottle.TryPass()) return;

        UpdateHueColor(e.Y);

        if (_clickedPoint != Point.Empty) UpdateColor(_clickedPoint, false);
        hueSlider.Invalidate();
    }

    private void HandleColorMapSelection(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (!_updateThrottle.TryPass()) return;

        UpdateColor(e.Location);
    }

    private void UpdateHueColor(int y)
    {
        _clickedHueY = y;
        _hueColor = CalculateHueColor(_clickedHueY, hueSlider.Height);
        UpdateBitmapImage();
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
                Color mixed = ColorService.InterpolateColor(targetColor, s, v);
                bmp.SetPixel(x, y, mixed);
            }
        }

        return bmp;
    }

    private void UpdateBitmapImage()
    {
        colorMap.Image?.Dispose();
        colorMap.Image = null;
        colorMap.Image = GenerateColorGradient(_hueColor, colorMap.Width, colorMap.Height);
    }
    #endregion

    #region イベントハンドラー
    private void ColorMap_Paint(object sender, PaintEventArgs e)
    {
        if (_clickedPoint == Point.Empty) return;

        FormUtils.DrawCrosshair(e.Graphics, _clickedPoint, _selectedColor);
    }

    private void HueSlider_Paint(object sender, PaintEventArgs e)
    {
        Color inverseColor = ColorService.InverseColor(_hueColor);

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, 0, _clickedHueY, hueSlider.Width, _clickedHueY);
    }
    #endregion

    #region 内部計算
    private static Color CalculateHueColor(int y, int barHeight)
    {
        double saturation = (double)y / (barHeight - 1);
        double hue = saturation * 360.0;

        return ColorService.ColorFromHSV(hue, 1.0f, 1.0f);
    }
    #endregion
}
