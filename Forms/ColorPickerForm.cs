using ColorChanger.Utils;
using System.Diagnostics;

namespace ColorChanger.Forms;

internal partial class ColorPickerForm : Form
{
    private const int COLOR_UPDATE_DEBOUNCE_MS = 14;

    /// <summary>
    /// 色が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ColorChanged;

    /// <summary>
    /// 現在選択されている色
    /// </summary>
    internal Color SelectedColor { get; private set; } = Color.Empty;

    private Color _initialColor = Color.Empty;
    private Point _clickedPoint = Point.Empty;
    private readonly Stopwatch _updateDebounceStopwatch = Stopwatch.StartNew();

    internal ColorPickerForm()
    {
        InitializeComponent();
        UpdateSelectedColor(SelectedColor);
    }

    #region 色設定関連
    /// <summary>
    /// 色を設定する
    /// </summary>
    /// <param name="newColor"></param>
    /// <param name="skipUpdate"></param>
    internal void SetColor(Color newColor, bool skipUpdate = false)
    {
        SelectedColor = newColor == Color.Empty ? Color.White : newColor;
        if (!skipUpdate) UpdateSelectedColor(SelectedColor);
    }

    /// <summary>
    /// デフォルトの色を設定する
    /// </summary>
    /// <param name="initialColor"></param>
    internal void SetInitialColor(Color initialColor)
        => _initialColor = initialColor;

    /// <summary>
    /// 色を選択する
    /// </summary>
    /// <param name="e"></param>
    /// <param name="suppressMove"></param>
    private void HandleColorSelection(MouseEventArgs e, bool suppressMove)
    {
        if (e.Button != MouseButtons.Left) return;
        if (colorPaletteBox.Image is not Bitmap bmp) return;

        if (suppressMove && _updateDebounceStopwatch.ElapsedMilliseconds <= COLOR_UPDATE_DEBOUNCE_MS) return;
        _updateDebounceStopwatch.Restart();

        Point originalCoords = BitmapUtils.GetOriginalCoordinates(e.Location, bmp.Size, colorPaletteBox.Size);
        if (!BitmapUtils.IsValidCoordinate(originalCoords, bmp.Size)) return;

        Color color = bmp.GetPixel(originalCoords.X, originalCoords.Y);

        UpdateSelectedColor(color, suppressMove);

        _clickedPoint = e.Location;
        colorPaletteBox.Invalidate();
    }

    /// <summary>
    /// 色を更新する
    /// </summary>
    /// <param name="color"></param>
    /// <param name="suppressMove"></param>
    private void UpdateSelectedColor(Color color, bool suppressMove = false)
    {
        SelectedColor = color;

        if (!suppressMove) NotifyColorChanged();

        previewColorBox.BackColor = color;

        redBar.Value = color.R;
        greenBar.Value = color.G;
        blueBar.Value = color.B;

        redTextBox.Text = color.R.ToString();
        greenTextBox.Text = color.G.ToString();
        blueTextBox.Text = color.B.ToString();

        colorCodeTextBox.Text = ColorUtils.GetColorCodeFromColor(color);

        if (suppressMove || colorPaletteBox.Image is not Bitmap bitmap) return;

        Point closestPoint = ColorUtils.GetClosestColorPoint(color, bitmap);

        float scaleX = (float)colorPaletteBox.Width / bitmap.Width;
        float scaleY = (float)colorPaletteBox.Height / bitmap.Height;

        _clickedPoint = new Point(
            (int)(closestPoint.X * scaleX),
            (int)(closestPoint.Y * scaleY)
        );

        colorPaletteBox.Invalidate();
    }

    /// <summary>
    /// 色をテキストボックスから取得して設定する
    /// </summary>
    private void SetColorFromTextFields()
    {
        int r = MathUtils.ParseAndClamp(redTextBox.Text);
        int g = MathUtils.ParseAndClamp(greenTextBox.Text);
        int b = MathUtils.ParseAndClamp(blueTextBox.Text);

        UpdateSelectedColor(Color.FromArgb(r, g, b));
    }
    #endregion

    #region イベントハンドラー
    private void ColorPaletteBox_Paint(object sender, PaintEventArgs e)
    {
        if (colorPaletteBox.Image is not Bitmap) return;
        if (_clickedPoint == Point.Empty) return;

        Color inverseColor = Color.FromArgb(255 - SelectedColor.R, 255 - SelectedColor.G, 255 - SelectedColor.B);

        using Pen pen = new Pen(inverseColor, 2);
        e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
        e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
    }

    private void ColorPaletteBox_MouseEvent(MouseEventArgs e, bool suppressMove)
        => HandleColorSelection(e, suppressMove);

    private void HandleSliderChanged(object sender, EventArgs e) =>
        UpdateSelectedColor(Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value), true);

    private void HandleSliderEnd(object sender, EventArgs e) =>
        UpdateSelectedColor(Color.FromArgb(redBar.Value, greenBar.Value, blueBar.Value));

    private void HandleTextKeyDown(object sender, KeyEventArgs e)
    {
        if (!FormUtils.IsNavigationKey(e)) return;

        SetColorFromTextFields();
        SelectNextControl((Control)sender, true, true, true, true);
    }

    private void ColorCodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (!FormUtils.IsNavigationKey(e)) return;

        Color color = ColorUtils.GetColorFromColorCode(colorCodeTextBox.Text);
        if (color == Color.Empty) return;

        UpdateSelectedColor(color);
        SelectNextControl((Control)sender, true, true, true, true);
    }

    private void OnColorTextChanged(object sender, EventArgs e)
        => SetColorFromTextFields();

    private void ResetButton_Click(object sender, EventArgs e)
        => UpdateSelectedColor(_initialColor);

    private void SelectButton_Click(object sender, EventArgs e)
    {
        SetColorFromTextFields();
        Hide();
    }

    private void NotifyColorChanged()
        => ColorChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void ColorPicker_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
