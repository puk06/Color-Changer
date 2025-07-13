namespace ColorChanger.Forms
{
    partial class SelectColorFromTextureForm
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
            previewBox = new PictureBox();
            OpenFile = new Button();
            ApplyColor = new Button();
            RGBText = new TextBox();
            ColorCodeText = new TextBox();
            selectedColorBox = new PictureBox();
            label2 = new Label();
            label15 = new Label();
            label1 = new Label();
            label13 = new Label();
            ((System.ComponentModel.ISupportInitialize)previewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)selectedColorBox).BeginInit();
            SuspendLayout();
            // 
            // previewBox
            // 
            previewBox.BackColor = SystemColors.ActiveBorder;
            previewBox.Location = new Point(12, 12);
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(420, 420);
            previewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            previewBox.TabIndex = 5;
            previewBox.TabStop = false;
            previewBox.Paint += OnPaint;
            previewBox.MouseDown += SelectPreviousColor;
            previewBox.MouseMove += SelectPreviousColor;
            // 
            // OpenFile
            // 
            OpenFile.Font = new Font("Yu Gothic UI", 13F);
            OpenFile.Location = new Point(444, 12);
            OpenFile.Name = "OpenFile";
            OpenFile.Size = new Size(257, 45);
            OpenFile.TabIndex = 66;
            OpenFile.Text = "ファイルを開く";
            OpenFile.UseVisualStyleBackColor = true;
            OpenFile.Click += OpenFile_Click;
            // 
            // ApplyColor
            // 
            ApplyColor.Font = new Font("Yu Gothic UI", 14F);
            ApplyColor.Location = new Point(444, 381);
            ApplyColor.Name = "ApplyColor";
            ApplyColor.Size = new Size(263, 51);
            ApplyColor.TabIndex = 68;
            ApplyColor.Text = "この色を適用";
            ApplyColor.UseVisualStyleBackColor = true;
            ApplyColor.Click += ApplyColor_Click;
            // 
            // RGBText
            // 
            RGBText.Font = new Font("Yu Gothic UI", 13F);
            RGBText.ForeColor = Color.DimGray;
            RGBText.Location = new Point(505, 261);
            RGBText.Name = "RGBText";
            RGBText.ReadOnly = true;
            RGBText.Size = new Size(196, 31);
            RGBText.TabIndex = 69;
            RGBText.Text = "Empty";
            // 
            // ColorCodeText
            // 
            ColorCodeText.Font = new Font("Yu Gothic UI", 13F);
            ColorCodeText.ForeColor = Color.DimGray;
            ColorCodeText.Location = new Point(551, 296);
            ColorCodeText.Name = "ColorCodeText";
            ColorCodeText.ReadOnly = true;
            ColorCodeText.Size = new Size(150, 31);
            ColorCodeText.TabIndex = 70;
            ColorCodeText.Text = "Empty";
            // 
            // selectedColorBox
            // 
            selectedColorBox.BackColor = SystemColors.ActiveBorder;
            selectedColorBox.Location = new Point(444, 117);
            selectedColorBox.Name = "selectedColorBox";
            selectedColorBox.Size = new Size(257, 70);
            selectedColorBox.TabIndex = 71;
            selectedColorBox.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label2.ForeColor = Color.DimGray;
            label2.Location = new Point(438, 84);
            label2.Name = "label2";
            label2.Size = new Size(133, 30);
            label2.TabIndex = 72;
            label2.Text = "選択された色";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Yu Gothic UI", 15F);
            label15.ForeColor = Color.DimGray;
            label15.Location = new Point(441, 259);
            label15.Name = "label15";
            label15.Size = new Size(58, 28);
            label15.TabIndex = 64;
            label15.Text = "RGB :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 15F);
            label1.ForeColor = Color.DimGray;
            label1.Location = new Point(441, 294);
            label1.Name = "label1";
            label1.Size = new Size(104, 28);
            label1.TabIndex = 67;
            label1.Text = "カラーコード :";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label13.ForeColor = Color.DimGray;
            label13.Location = new Point(438, 229);
            label13.Name = "label13";
            label13.Size = new Size(79, 30);
            label13.TabIndex = 65;
            label13.Text = "色情報";
            // 
            // SelectColorFromTextureForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 445);
            Controls.Add(label2);
            Controls.Add(selectedColorBox);
            Controls.Add(ColorCodeText);
            Controls.Add(RGBText);
            Controls.Add(ApplyColor);
            Controls.Add(label1);
            Controls.Add(OpenFile);
            Controls.Add(label13);
            Controls.Add(label15);
            Controls.Add(previewBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            MaximizeBox = false;
            Name = "SelectColorFromTextureForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Select Color From Texture by Color Changer";
            DragDrop += SelectColorFromTextureForm_DragDrop;
            DragEnter += SelectColorFromTextureForm_DragEnter;
            ((System.ComponentModel.ISupportInitialize)previewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)selectedColorBox).EndInit();
            FormClosing += SelectColorFromTextureForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox previewBox;
        private Button OpenFile;
        private Button ApplyColor;
        private TextBox RGBText;
        private TextBox ColorCodeText;
        private PictureBox selectedColorBox;
        private Label label2;
        private Label label15;
        private Label label1;
        private Label label13;
    }
}