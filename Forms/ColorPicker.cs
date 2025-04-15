using static VRC_Color_Changer.Classes.Helper;

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
            if (ColorPalleteBox.Image is not Bitmap) return;
            if (e.Button != MouseButtons.Left) return;

            // Convert clicked coordinates to original image coordinates  
            var originalCoords = GetOriginalCoordinates(e.Location, ((Bitmap)ColorPalleteBox.Image).Size, ColorPalleteBox.Size);
            if (!IsValidCoordinate(originalCoords, ((Bitmap)ColorPalleteBox.Image).Size)) return;

            // Get color from the original image  
            UpdateSelectedColor(((Bitmap)ColorPalleteBox.Image).GetPixel(originalCoords.X, originalCoords.Y), true);

            clickedPoint = e.Location;
            ColorPalleteBox.Invalidate();
        }

        private void ColorPalleteBox_Paint(object sender, PaintEventArgs e)
        {
            if (ColorPalleteBox.Image is not Bitmap) return;
            if (clickedPoint == Point.Empty) return;

            Color inverseColor = Color.FromArgb(255 - SelectedColor.R, 255 - SelectedColor.G, 255 - SelectedColor.B);
            Pen pen = new(inverseColor, 2);

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

        private void selectButton_Click(object sender, EventArgs e)
        {
            updateColor();
            Close();
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
            if (ColorPalleteBox.Image is not Bitmap) return;
            if (noMoveMode) return;
            Point closestPoint = GetClosestColorPoint(color, (Bitmap)ColorPalleteBox.Image);
            clickedPoint = new Point((int)(closestPoint.X * (float)ColorPalleteBox.Width / ((Bitmap)ColorPalleteBox.Image).Width), (int)(closestPoint.Y * (float)ColorPalleteBox.Height / ((Bitmap)ColorPalleteBox.Image).Height));
            ColorPalleteBox.Invalidate();
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

        private static bool CheckKeyInput(KeyEventArgs e)
        {
            Keys keys = ModifierKeys;
            return e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab || (keys == Keys.Shift && e.KeyCode == Keys.Tab);
        }
    }
}
