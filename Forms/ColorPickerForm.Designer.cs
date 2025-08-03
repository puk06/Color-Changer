using ColorChanger.UserControls;

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
            label5 = new Label();
            colorCodeTextBox = new TextBox();
            label6 = new Label();
            resetButton = new Button();
            label4 = new Label();
            colorPalette = new ColorPalette();
            ((System.ComponentModel.ISupportInitialize)redBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)blueBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)greenBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).BeginInit();
            SuspendLayout();
            // 
            // redBar
            // 
            redBar.AutoSize = false;
            redBar.Location = new Point(55, 319);
            redBar.Maximum = 255;
            redBar.Name = "redBar";
            redBar.Size = new Size(238, 35);
            redBar.TabIndex = 0;
            redBar.TickFrequency = 85;
            redBar.Scroll += HandleSliderChanged;
            redBar.MouseUp += HandleSliderEnd;
            // 
            // blueBar
            // 
            blueBar.AutoSize = false;
            blueBar.Location = new Point(55, 401);
            blueBar.Maximum = 255;
            blueBar.Name = "blueBar";
            blueBar.Size = new Size(238, 35);
            blueBar.TabIndex = 2;
            blueBar.TickFrequency = 85;
            blueBar.Scroll += HandleSliderChanged;
            blueBar.MouseUp += HandleSliderEnd;
            // 
            // greenBar
            // 
            greenBar.AutoSize = false;
            greenBar.Location = new Point(56, 360);
            greenBar.Maximum = 255;
            greenBar.Name = "greenBar";
            greenBar.Size = new Size(237, 35);
            greenBar.TabIndex = 3;
            greenBar.TickFrequency = 85;
            greenBar.Scroll += HandleSliderChanged;
            greenBar.MouseUp += HandleSliderEnd;
            // 
            // previewColorBox
            // 
            previewColorBox.BackColor = SystemColors.ActiveBorder;
            previewColorBox.Location = new Point(278, 244);
            previewColorBox.Name = "previewColorBox";
            previewColorBox.Size = new Size(55, 55);
            previewColorBox.TabIndex = 5;
            previewColorBox.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 13F);
            label1.ForeColor = SystemColors.ControlDarkDark;
            label1.Location = new Point(4, 320);
            label1.Name = "label1";
            label1.Size = new Size(43, 25);
            label1.TabIndex = 6;
            label1.Text = "Red";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 13F);
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(4, 359);
            label2.Name = "label2";
            label2.Size = new Size(58, 25);
            label2.TabIndex = 7;
            label2.Text = "Green";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 13F);
            label3.ForeColor = SystemColors.ControlDarkDark;
            label3.Location = new Point(4, 399);
            label3.Name = "label3";
            label3.Size = new Size(45, 25);
            label3.TabIndex = 8;
            label3.Text = "Blue";
            // 
            // selectButton
            // 
            selectButton.Font = new Font("Yu Gothic UI", 11F);
            selectButton.Location = new Point(240, 452);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(118, 43);
            selectButton.TabIndex = 9;
            selectButton.Text = "選択";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += SelectButton_Click;
            // 
            // redTextBox
            // 
            redTextBox.Font = new Font("Yu Gothic UI", 11F);
            redTextBox.Location = new Point(299, 319);
            redTextBox.Name = "redTextBox";
            redTextBox.Size = new Size(59, 27);
            redTextBox.TabIndex = 10;
            redTextBox.Text = "0";
            redTextBox.TextAlign = HorizontalAlignment.Center;
            redTextBox.KeyDown += HandleTextKeyDown;
            redTextBox.Leave += OnColorTextChanged;
            // 
            // greenTextBox
            // 
            greenTextBox.Font = new Font("Yu Gothic UI", 11F);
            greenTextBox.Location = new Point(299, 358);
            greenTextBox.Name = "greenTextBox";
            greenTextBox.Size = new Size(59, 27);
            greenTextBox.TabIndex = 11;
            greenTextBox.Text = "0";
            greenTextBox.TextAlign = HorizontalAlignment.Center;
            greenTextBox.KeyDown += HandleTextKeyDown;
            greenTextBox.Leave += OnColorTextChanged;
            // 
            // blueTextBox
            // 
            blueTextBox.Font = new Font("Yu Gothic UI", 11F);
            blueTextBox.Location = new Point(299, 398);
            blueTextBox.Name = "blueTextBox";
            blueTextBox.Size = new Size(59, 27);
            blueTextBox.TabIndex = 12;
            blueTextBox.Text = "0";
            blueTextBox.TextAlign = HorizontalAlignment.Center;
            blueTextBox.KeyDown += HandleTextKeyDown;
            blueTextBox.Leave += OnColorTextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Yu Gothic UI", 14F);
            label5.Location = new Point(58, 242);
            label5.Name = "label5";
            label5.Size = new Size(89, 25);
            label5.TabIndex = 14;
            label5.Text = "カラーコード";
            // 
            // colorCodeTextBox
            // 
            colorCodeTextBox.Font = new Font("Yu Gothic UI", 12F);
            colorCodeTextBox.Location = new Point(44, 270);
            colorCodeTextBox.Name = "colorCodeTextBox";
            colorCodeTextBox.PlaceholderText = "#000000";
            colorCodeTextBox.Size = new Size(120, 29);
            colorCodeTextBox.TabIndex = 15;
            colorCodeTextBox.TextAlign = HorizontalAlignment.Center;
            colorCodeTextBox.KeyDown += ColorCodeTextBox_KeyDown;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Yu Gothic UI", 14F);
            label6.Location = new Point(196, 260);
            label6.Name = "label6";
            label6.Size = new Size(75, 25);
            label6.TabIndex = 16;
            label6.Text = "プレビュー";
            // 
            // resetButton
            // 
            resetButton.Font = new Font("Yu Gothic UI", 11F);
            resetButton.Location = new Point(12, 452);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(118, 43);
            resetButton.TabIndex = 17;
            resetButton.Text = "リセット";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += ResetButton_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 128);
            label4.Location = new Point(11, 9);
            label4.Name = "label4";
            label4.Size = new Size(91, 21);
            label4.TabIndex = 19;
            label4.Text = "カラーパレット";
            // 
            // colorPalette
            // 
            colorPalette.BackColor = SystemColors.ActiveBorder;
            colorPalette.Location = new Point(11, 33);
            colorPalette.Name = "colorPalette";
            colorPalette.Size = new Size(347, 202);
            colorPalette.TabIndex = 18;
            // 
            // ColorPickerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(370, 507);
            Controls.Add(label4);
            Controls.Add(colorPalette);
            Controls.Add(resetButton);
            Controls.Add(label6);
            Controls.Add(colorCodeTextBox);
            Controls.Add(label5);
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
            Controls.Add(redBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColorPickerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Color Picker by Color Changer";
            FormClosing += ColorPicker_FormClosing;
            ((System.ComponentModel.ISupportInitialize)redBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)blueBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)greenBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)previewColorBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar redBar;
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
        private Label label5;
        private TextBox colorCodeTextBox;
        private Label label6;
        private Button resetButton;
        private UserControls.ColorPalette colorPalette;
        private Label label4;
    }
}
