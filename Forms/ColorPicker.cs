namespace VRC_Color_Changer
{
    public partial class ColorPicker : Form
    {
        public Color SelectedColor { get; private set; }
        private Point clickedPoint;

        public ColorPicker(Color defaultColor)
        {
            InitializeComponent();

            SelectedColor = defaultColor == Color.Empty ? Color.White : defaultColor;
            UpdateSelectedColor(SelectedColor);
        }

        private void ColorPalleteBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (ColorPalleteBox.Image is not Bitmap bmp) return;
            if (e.Button != MouseButtons.Left) return;

            // Convert clicked coordinates to original image coordinates
            var originalCoords = GetOriginalCoordinates(e.Location, bmp.Size, ColorPalleteBox.Size);
            if (!IsValidCoordinate(originalCoords, bmp.Size)) return;

            // Get color from the original image
            UpdateSelectedColor(bmp.GetPixel(originalCoords.X, originalCoords.Y), true);

            clickedPoint = e.Location;
            ColorPalleteBox.Invalidate();
        }


        private void ColorPalleteBox_Paint(object sender, PaintEventArgs e)
        {
            if (ColorPalleteBox.Image is not Bitmap bmp) return;
            if (clickedPoint == Point.Empty) return;

            Color inverseColor = Color.FromArgb(255 - SelectedColor.R, 255 - SelectedColor.G, 255 - SelectedColor.B);
            Pen pen = new Pen(inverseColor, 2);

            e.Graphics.DrawLine(pen, clickedPoint.X - 5, clickedPoint.Y, clickedPoint.X + 5, clickedPoint.Y);
            e.Graphics.DrawLine(pen, clickedPoint.X, clickedPoint.Y - 5, clickedPoint.X, clickedPoint.Y + 5);
        }

        private void sliderChanged(object sender, EventArgs e)
        {
            UpdateSelectedColor(Color.FromArgb(RedBar.Value, GreenBar.Value, BlueBar.Value), true);
        }

        private void sliderEnd(object sender, EventArgs e)
        {
            UpdateSelectedColor(Color.FromArgb(RedBar.Value, GreenBar.Value, BlueBar.Value));
        }

        private void textChanged(object sender, EventArgs e) => updateColor();

        private void textKeyDown(object sender, KeyEventArgs e)
        {
            if (CheckKeyInput(e))
            {
                updateColor();
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void updateColor()
        {
            int r = ParseAndClamp(RedTextBox.Text);
            int g = ParseAndClamp(GreenTextBox.Text);
            int b = ParseAndClamp(BlueTextBox.Text);

            UpdateSelectedColor(Color.FromArgb(r, g, b));
        }

        private bool CheckKeyInput(KeyEventArgs e)
        {
            Keys keys = ModifierKeys;
            return e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab || (keys == Keys.Shift && e.KeyCode == Keys.Tab);
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            updateColor();
            Close();
        }

        private Point GetOriginalCoordinates(Point clickedPoint, Size originalSize, Size displaySize)
        {
            float ratioX = (float)originalSize.Width / displaySize.Width;
            float ratioY = (float)originalSize.Height / displaySize.Height;

            return new Point((int)(clickedPoint.X * ratioX), (int)(clickedPoint.Y * ratioY));
        }

        private bool IsValidCoordinate(Point coords, Size size)
        {
            return coords.X >= 0 && coords.X < size.Width && coords.Y >= 0 && coords.Y < size.Height;
        }

        private int ParseAndClamp(string value)
        {
            return Math.Min(255, Math.Max(0, int.TryParse(value, out int result) ? result : 0));
        }

        private void UpdateSelectedColor(Color color, bool noMoveMode = false)
        {
            SelectedColor = color;
            previewColorBox.BackColor = color;

            // Update UI elements
            RedBar.Value = color.R;
            GreenBar.Value = color.G;
            BlueBar.Value = color.B;

            RedTextBox.Text = color.R.ToString();
            GreenTextBox.Text = color.G.ToString();
            BlueTextBox.Text = color.B.ToString();

            colorCodeTextBox.Text = GetColorCodeFromColor(color);

            // Update color palette
            if (ColorPalleteBox.Image is not Bitmap bmp) return;
            if (noMoveMode) return;
            Point closestPoint = GetClosestColorPoint(color, bmp);
            clickedPoint = new Point((int)(closestPoint.X * (float)ColorPalleteBox.Width / bmp.Width), (int)(closestPoint.Y * (float)ColorPalleteBox.Height / bmp.Height));
            ColorPalleteBox.Invalidate();
        }

        private string GetColorCodeFromColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private Color GetColorFromColorCode(string colorCode)
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

        private Point GetClosestColorPoint(Color color, Bitmap bmp)
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

        private double GetColorDistance(Color color1, Color color2)
        {
            double r = Math.Pow(color1.R - color2.R, 2);
            double g = Math.Pow(color1.G - color2.G, 2);
            double b = Math.Pow(color1.B - color2.B, 2);
            return Math.Sqrt(r + g + b);
        }

        private void colorCodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (CheckKeyInput(e))
            {
                Color color = GetColorFromColorCode(colorCodeTextBox.Text);
                if (color != Color.Empty)
                {
                    UpdateSelectedColor(color);
                    SelectNextControl((Control)sender, true, true, true, true);
                }
            }
        }
    }
}
