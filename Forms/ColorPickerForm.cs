using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class ColorPickerForm : Form
{
    /// <summary>
    /// 色が変更されたときに発生するイベント
    /// </summary>
    internal EventHandler? ColorChanged;

    /// <summary>
    /// 現在選択されている色
    /// </summary>
    internal Color SelectedColor { get; private set; } = Color.Empty;

    private Point _clickedPoint = Point.Empty;

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
    internal void SetColor(Color newColor)
    {
        SelectedColor = newColor == Color.Empty ? Color.White : newColor;
        UpdateSelectedColor(SelectedColor);
    }

    /// <summary>
    /// 色を選択する
    /// </summary>
    /// <param name="e"></param>
    /// <param name="disableMove"></param>
    private void HandleColorSelection(MouseEventArgs e, bool disableMove)
    {
        if (colorPaletteBox.Image is not Bitmap bmp) return;
        if (e.Button != MouseButtons.Left) return;

        var originalCoords = BitmapUtils.GetOriginalCoordinates(e.Location, bmp.Size, colorPaletteBox.Size);
        if (!BitmapUtils.IsValidCoordinate(originalCoords, bmp.Size)) return;

        var color = bmp.GetPixel(originalCoords.X, originalCoords.Y);
        UpdateSelectedColor(color, disableMove);

        _clickedPoint = e.Location;
        colorPaletteBox.Invalidate();
    }

    /// <summary>
    /// 色を更新する
    /// </summary>
    /// <param name="color"></param>
    /// <param name="disableMove"></param>
    private void UpdateSelectedColor(Color color, bool disableMove = false)
    {
        SelectedColor = color;
        if (!disableMove) NotifyColorChanged();
        previewColorBox.BackColor = color;

        // Update UI
        redBar.Value = color.R;
        greenBar.Value = color.G;
        blueBar.Value = color.B;

        redTextBox.Text = color.R.ToString();
        greenTextBox.Text = color.G.ToString();
        blueTextBox.Text = color.B.ToString();

        colorCodeTextBox.Text = ColorUtils.GetColorCodeFromColor(color);

        if (colorPaletteBox.Image is not Bitmap || disableMove) return;

        Point closestPoint = ColorUtils.GetClosestColorPoint(color, (Bitmap)colorPaletteBox.Image);
        _clickedPoint = new Point(
            (int)(closestPoint.X * (float)colorPaletteBox.Width / ((Bitmap)colorPaletteBox.Image).Width),
            (int)(closestPoint.Y * (float)colorPaletteBox.Height / ((Bitmap)colorPaletteBox.Image).Height)
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

    private void ColorPaletteBox_MouseEvent(object _, MouseEventArgs e, bool disableMove)
        => HandleColorSelection(e, disableMove);

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