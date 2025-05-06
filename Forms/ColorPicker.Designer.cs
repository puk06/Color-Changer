namespace ColorChanger
{
    partial class ColorPicker
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
            RedBar = new TrackBar();
            ColorPalleteBox = new PictureBox();
            BlueBar = new TrackBar();
            GreenBar = new TrackBar();
            previewColorBox = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            selectButton = new Button();
            RedTextBox = new TextBox();
            GreenTextBox = new TextBox();
            BlueTextBox = new TextBox();
            label4 = new Label();
            label5 = new Label();
            colorCodeTextBox = new TextBox();
            label6 = new Label();
            ((System.ComponentModel.ISupportInitialize)RedBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ColorPalleteBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BlueBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GreenBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).BeginInit();
            SuspendLayout();
            // 
            // RedBar
            // 
            RedBar.Location = new Point(55, 307);
            RedBar.Maximum = 255;
            RedBar.Name = "RedBar";
            RedBar.Size = new Size(222, 45);
            RedBar.TabIndex = 0;
            RedBar.TickFrequency = 85;
            RedBar.Scroll += SliderChanged;
            RedBar.MouseUp += SliderEnd;
            // 
            // ColorPalleteBox
            // 
            ColorPalleteBox.Image = Properties.Resources.RGB;
            ColorPalleteBox.Location = new Point(26, 25);
            ColorPalleteBox.Name = "ColorPalleteBox";
            ColorPalleteBox.Size = new Size(315, 202);
            ColorPalleteBox.SizeMode = PictureBoxSizeMode.StretchImage;
            ColorPalleteBox.TabIndex = 1;
            ColorPalleteBox.TabStop = false;
            ColorPalleteBox.Paint += ColorPaletteBox_Paint;
            ColorPalleteBox.MouseDown += ColorPalleteBox_MouseMove;
            ColorPalleteBox.MouseMove += ColorPalleteBox_MouseMove;
            // 
            // BlueBar
            // 
            BlueBar.Location = new Point(56, 409);
            BlueBar.Maximum = 255;
            BlueBar.Name = "BlueBar";
            BlueBar.Size = new Size(221, 45);
            BlueBar.TabIndex = 2;
            BlueBar.TickFrequency = 85;
            BlueBar.Scroll += SliderChanged;
            BlueBar.MouseUp += SliderEnd;
            // 
            // GreenBar
            // 
            GreenBar.Location = new Point(56, 358);
            GreenBar.Maximum = 255;
            GreenBar.Name = "GreenBar";
            GreenBar.Size = new Size(221, 45);
            GreenBar.TabIndex = 3;
            GreenBar.TickFrequency = 85;
            GreenBar.Scroll += SliderChanged;
            GreenBar.MouseUp += SliderEnd;
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
            RedTextBox.Font = new Font("Yu Gothic UI", 11F);
            RedTextBox.Location = new Point(283, 306);
            RedTextBox.Name = "RedTextBox";
            RedTextBox.Size = new Size(59, 27);
            RedTextBox.TabIndex = 10;
            RedTextBox.Text = "0";
            RedTextBox.TextAlign = HorizontalAlignment.Center;
            RedTextBox.KeyDown += TextKeyDown;
            RedTextBox.Leave += OnColorTextChanged;
            // 
            // GreenTextBox
            // 
            GreenTextBox.Font = new Font("Yu Gothic UI", 11F);
            GreenTextBox.Location = new Point(283, 357);
            GreenTextBox.Name = "GreenTextBox";
            GreenTextBox.Size = new Size(59, 27);
            GreenTextBox.TabIndex = 11;
            GreenTextBox.Text = "0";
            GreenTextBox.TextAlign = HorizontalAlignment.Center;
            GreenTextBox.KeyDown += TextKeyDown;
            GreenTextBox.Leave += OnColorTextChanged;
            // 
            // BlueTextBox
            // 
            BlueTextBox.Font = new Font("Yu Gothic UI", 11F);
            BlueTextBox.Location = new Point(283, 408);
            BlueTextBox.Name = "BlueTextBox";
            BlueTextBox.Size = new Size(59, 27);
            BlueTextBox.TabIndex = 12;
            BlueTextBox.Text = "0";
            BlueTextBox.TextAlign = HorizontalAlignment.Center;
            BlueTextBox.KeyDown += TextKeyDown;
            BlueTextBox.Leave += OnColorTextChanged;
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
            Controls.Add(BlueTextBox);
            Controls.Add(GreenTextBox);
            Controls.Add(RedTextBox);
            Controls.Add(selectButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(previewColorBox);
            Controls.Add(GreenBar);
            Controls.Add(BlueBar);
            Controls.Add(ColorPalleteBox);
            Controls.Add(RedBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColorPicker";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Color Picker by Color Changer";
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            ((System.ComponentModel.ISupportInitialize)RedBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)ColorPalleteBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)BlueBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)GreenBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar RedBar;
        private PictureBox ColorPalleteBox;
        private TrackBar BlueBar;
        private TrackBar GreenBar;
        private PictureBox previewColorBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button selectButton;
        private TextBox RedTextBox;
        private TextBox GreenTextBox;
        private TextBox BlueTextBox;
        private Label label4;
        private Label label5;
        private TextBox colorCodeTextBox;
        private Label label6;
    }
}
