namespace ColorChanger.Forms
{
    partial class SelectionPenSettingsForm
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
            label1 = new Label();
            enablePen = new CheckBox();
            penWidth = new TrackBar();
            label2 = new Label();
            eraserMode = new CheckBox();
            penWidthLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)penWidth).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI Semibold", 17F, FontStyle.Bold);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(149, 31);
            label1.TabIndex = 0;
            label1.Text = "選択ペンツール";
            // 
            // enablePen
            // 
            enablePen.AutoSize = true;
            enablePen.Font = new Font("Yu Gothic UI", 13F);
            enablePen.Location = new Point(12, 55);
            enablePen.Name = "enablePen";
            enablePen.Size = new Size(164, 29);
            enablePen.TabIndex = 1;
            enablePen.Text = "選択ペンの有効化";
            enablePen.UseVisualStyleBackColor = true;
            // 
            // penWidth
            // 
            penWidth.Location = new Point(10, 115);
            penWidth.Maximum = 500;
            penWidth.Name = "penWidth";
            penWidth.Size = new Size(339, 45);
            penWidth.TabIndex = 2;
            penWidth.TickFrequency = 50;
            penWidth.Value = 100;
            penWidth.ValueChanged += penWidth_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 13F);
            label2.Location = new Point(10, 87);
            label2.Name = "label2";
            label2.Size = new Size(85, 25);
            label2.TabIndex = 3;
            label2.Text = "ペンの太さ";
            // 
            // eraserMode
            // 
            eraserMode.AutoSize = true;
            eraserMode.Font = new Font("Yu Gothic UI", 13F);
            eraserMode.Location = new Point(205, 55);
            eraserMode.Name = "eraserMode";
            eraserMode.Size = new Size(126, 29);
            eraserMode.TabIndex = 5;
            eraserMode.Text = "消しゴムモード";
            eraserMode.UseVisualStyleBackColor = true;
            // 
            // penWidthLabel
            // 
            penWidthLabel.AutoSize = true;
            penWidthLabel.Font = new Font("Yu Gothic UI", 13F);
            penWidthLabel.Location = new Point(92, 87);
            penWidthLabel.Name = "penWidthLabel";
            penWidthLabel.Size = new Size(53, 25);
            penWidthLabel.TabIndex = 6;
            penWidthLabel.Text = "- 0px";
            // 
            // SelectionPenSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(363, 157);
            Controls.Add(penWidthLabel);
            Controls.Add(eraserMode);
            Controls.Add(label2);
            Controls.Add(penWidth);
            Controls.Add(enablePen);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SelectionPenSettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Selection Pen By Color Changer";
            ((System.ComponentModel.ISupportInitialize)penWidth).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private CheckBox enablePen;
        private TrackBar penWidth;
        private Label label2;
        private CheckBox eraserMode;
        private Label penWidthLabel;
    }
}