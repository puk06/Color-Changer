namespace ColorChanger.Forms
{
    partial class ColorPickerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            redBar = new TrackBar();
            colorPaletteBox = new PictureBox();
            blueBar = new TrackBar();
            greenBar = new TrackBar();
            previewColorBox = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            selectButton = new Button();
            redTextBox = new TextBox();
            greenTextBox = new TextBox();
            blueTextBox = new TextBox();
            label4 = new Label();
            label5 = new Label();
            colorCodeTextBox = new TextBox();
            label6 = new Label();
            ((System.ComponentModel.ISupportInitialize)redBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)colorPaletteBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)blueBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)greenBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).BeginInit();
            SuspendLayout();
            // 
            // RedBar
            // 
            redBar.Location = new Point(55, 307);
            redBar.Maximum = 255;
            redBar.Name = "RedBar";
            redBar.Size = new Size(222, 45);
            redBar.TabIndex = 0;
            redBar.TickFrequency = 85;
            redBar.Scroll += HandleSliderChanged;
            redBar.MouseUp += HandleSliderEnd;
            // 
            // ColorPaletteBox
            // 
            colorPaletteBox.Image = Properties.Resources.RGB;
            colorPaletteBox.Location = new Point(26, 25);
            colorPaletteBox.Name = "ColorPaletteBox";
            colorPaletteBox.Size = new Size(315, 202);
            colorPaletteBox.SizeMode = PictureBoxSizeMode.StretchImage;
            colorPaletteBox.TabIndex = 1;
            colorPaletteBox.TabStop = false;
            colorPaletteBox.Paint += ColorPaletteBox_Paint;
            colorPaletteBox.MouseDown += (s, e) => ColorPaletteBox_MouseEvent(s, e, true);
            colorPaletteBox.MouseMove += (s, e) => ColorPaletteBox_MouseEvent(s, e, true);
            colorPaletteBox.MouseUp += (s, e) => ColorPaletteBox_MouseEvent(s, e, false);
            // 
            // BlueBar
            // 
            blueBar.Location = new Point(56, 409);
            blueBar.Maximum = 255;
            blueBar.Name = "BlueBar";
            blueBar.Size = new Size(221, 45);
            blueBar.TabIndex = 2;
            blueBar.TickFrequency = 85;
            blueBar.Scroll += HandleSliderChanged;
            blueBar.MouseUp += HandleSliderEnd;
            // 
            // GreenBar
            // 
            greenBar.Location = new Point(56, 358);
            greenBar.Maximum = 255;
            greenBar.Name = "GreenBar";
            greenBar.Size = new Size(221, 45);
            greenBar.TabIndex = 3;
            greenBar.TickFrequency = 85;
            greenBar.Scroll += HandleSliderChanged;
            greenBar.MouseUp += HandleSliderEnd;
            // 
            // previewColorBox
            // 
            previewColorBox.Location = new Point(284, 233);
            previewColorBox.Name = "previewColorBox";
            previewColorBox.Size = new Size(55, 55);
            previewColorBox.TabIndex = 5;
            previewColorBox.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 13F);
            label1.Location = new Point(24, 307);
            label1.Name = "label1";
            label1.Size = new Size(23, 25);
            label1.TabIndex = 6;
            label1.Text = "R";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 13F);
            label2.Location = new Point(24, 358);
            label2.Name = "label2";
            label2.Size = new Size(24, 25);
            label2.TabIndex = 7;
            label2.Text = "G";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 13F);
            label3.Location = new Point(24, 409);
            label3.Name = "label3";
            label3.Size = new Size(22, 25);
            label3.TabIndex = 8;
            label3.Text = "B";
            // 
            // selectButton
            // 
            selectButton.Location = new Point(240, 460);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(118, 33);
            selectButton.TabIndex = 9;
            selectButton.Text = "選択";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += SelectButton_Click;
            // 
            // RedTextBox
            // 
            redTextBox.Font = new Font("Yu Gothic UI", 11F);
            redTextBox.Location = new Point(283, 306);
            redTextBox.Name = "RedTextBox";
            redTextBox.Size = new Size(59, 27);
            redTextBox.TabIndex = 10;
            redTextBox.Text = "0";
            redTextBox.TextAlign = HorizontalAlignment.Center;
            redTextBox.KeyDown += HandleTextKeyDown;
            redTextBox.Leave += OnColorTextChanged;
            // 
            // GreenTextBox
            // 
            greenTextBox.Font = new Font("Yu Gothic UI", 11F);
            greenTextBox.Location = new Point(283, 357);
            greenTextBox.Name = "GreenTextBox";
            greenTextBox.Size = new Size(59, 27);
            greenTextBox.TabIndex = 11;
            greenTextBox.Text = "0";
            greenTextBox.TextAlign = HorizontalAlignment.Center;
            greenTextBox.KeyDown += HandleTextKeyDown;
            greenTextBox.Leave += OnColorTextChanged;
            // 
            // BlueTextBox
            // 
            blueTextBox.Font = new Font("Yu Gothic UI", 11F);
            blueTextBox.Location = new Point(283, 408);
            blueTextBox.Name = "BlueTextBox";
            blueTextBox.Size = new Size(59, 27);
            blueTextBox.TabIndex = 12;
            blueTextBox.Text = "0";
            blueTextBox.TextAlign = HorizontalAlignment.Center;
            blueTextBox.KeyDown += HandleTextKeyDown;
            blueTextBox.Leave += OnColorTextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 10F);
            label4.Location = new Point(26, 3);
            label4.Name = "label4";
            label4.Size = new Size(104, 19);
            label4.TabIndex = 13;
            label4.Text = "RGBカラーパレット";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Yu Gothic UI", 14F);
            label5.Location = new Point(64, 231);
            label5.Name = "label5";
            label5.Size = new Size(89, 25);
            label5.TabIndex = 14;
            label5.Text = "カラーコード";
            // 
            // colorCodeTextBox
            // 
            colorCodeTextBox.Font = new Font("Yu Gothic UI", 12F);
            colorCodeTextBox.Location = new Point(50, 259);
            colorCodeTextBox.Name = "colorCodeTextBox";
            colorCodeTextBox.Size = new Size(120, 29);
            colorCodeTextBox.TabIndex = 15;
            colorCodeTextBox.TextAlign = HorizontalAlignment.Center;
            colorCodeTextBox.KeyDown += ColorCodeTextBox_KeyDown;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Yu Gothic UI", 14F);
            label6.Location = new Point(202, 249);
            label6.Name = "label6";
            label6.Size = new Size(75, 25);
            label6.TabIndex = 16;
            label6.Text = "プレビュー";
            // 
            // ColorPicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(370, 506);
            Controls.Add(label6);
            Controls.Add(colorCodeTextBox);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(blueTextBox);
            Controls.Add(greenTextBox);
            Controls.Add(redTextBox);
            Controls.Add(selectButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(previewColorBox);
            Controls.Add(greenBar);
            Controls.Add(blueBar);
            Controls.Add(colorPaletteBox);
            Controls.Add(redBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColorPicker";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Color Picker by Color Changer";
            FormClosing += ColorPicker_FormClosing;
            ((System.ComponentModel.ISupportInitialize)redBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)colorPaletteBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)blueBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)greenBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar redBar;
        private PictureBox colorPaletteBox;
        private TrackBar blueBar;
        private TrackBar greenBar;
        private PictureBox previewColorBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button selectButton;
        private TextBox redTextBox;
        private TextBox greenTextBox;
        private TextBox blueTextBox;
        private Label label4;
        private Label label5;
        private TextBox colorCodeTextBox;
        private Label label6;
    }
}
