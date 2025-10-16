namespace ColorChanger.Forms
{
    partial class SelectAreaFromImageMaskForm
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
            label2 = new Label();
            SelectArea = new Button();
            OpenFile = new Button();
            previewBox = new PictureBox();
            selectionType = new ComboBox();
            selectedColorLabel = new Label();
            selectedColorBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)previewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)selectedColorBox).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label2.ForeColor = Color.DimGray;
            label2.Location = new Point(438, 84);
            label2.Name = "label2";
            label2.Size = new Size(107, 30);
            label2.TabIndex = 82;
            label2.Text = "選択タイプ";
            // 
            // SelectArea
            // 
            SelectArea.Font = new Font("Yu Gothic UI", 14F);
            SelectArea.Location = new Point(444, 381);
            SelectArea.Name = "SelectArea";
            SelectArea.Size = new Size(263, 51);
            SelectArea.TabIndex = 78;
            SelectArea.Text = "選択";
            SelectArea.UseVisualStyleBackColor = true;
            SelectArea.Click += SelectArea_Click;
            // 
            // OpenFile
            // 
            OpenFile.Font = new Font("Yu Gothic UI", 13F);
            OpenFile.Location = new Point(444, 12);
            OpenFile.Name = "OpenFile";
            OpenFile.Size = new Size(257, 45);
            OpenFile.TabIndex = 76;
            OpenFile.Text = "ファイルを開く";
            OpenFile.UseVisualStyleBackColor = true;
            OpenFile.Click += OpenFile_Click;
            // 
            // previewBox
            // 
            previewBox.BackColor = SystemColors.ActiveBorder;
            previewBox.Location = new Point(12, 12);
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(420, 420);
            previewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            previewBox.TabIndex = 73;
            previewBox.TabStop = false;
            previewBox.MouseDown += SelectColor;
            previewBox.MouseMove += SelectColor;
            // 
            // selectionType
            // 
            selectionType.DropDownStyle = ComboBoxStyle.DropDownList;
            selectionType.Font = new Font("Yu Gothic UI", 13F);
            selectionType.FormattingEnabled = true;
            selectionType.Items.AddRange(new object[] { "黒", "白", "不透明 (A = 255)", "不透明 (A ≠ 0)", "透明 (A = 0)", "指定色" });
            selectionType.Location = new Point(444, 117);
            selectionType.Name = "selectionType";
            selectionType.Size = new Size(257, 31);
            selectionType.TabIndex = 83;
            selectionType.SelectedIndexChanged += SelectionType_SelectedIndexChanged;
            // 
            // selectedColorLabel
            // 
            selectedColorLabel.AutoSize = true;
            selectedColorLabel.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            selectedColorLabel.ForeColor = Color.DimGray;
            selectedColorLabel.Location = new Point(438, 201);
            selectedColorLabel.Name = "selectedColorLabel";
            selectedColorLabel.Size = new Size(133, 30);
            selectedColorLabel.TabIndex = 85;
            selectedColorLabel.Text = "選択された色";
            // 
            // selectedColorBox
            // 
            selectedColorBox.BackColor = SystemColors.ActiveBorder;
            selectedColorBox.Location = new Point(444, 234);
            selectedColorBox.Name = "selectedColorBox";
            selectedColorBox.Size = new Size(257, 70);
            selectedColorBox.TabIndex = 84;
            selectedColorBox.TabStop = false;
            // 
            // SelectAreaFromImageMaskForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 445);
            Controls.Add(selectedColorLabel);
            Controls.Add(selectedColorBox);
            Controls.Add(selectionType);
            Controls.Add(label2);
            Controls.Add(SelectArea);
            Controls.Add(OpenFile);
            Controls.Add(previewBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SelectAreaFromImageMaskForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Select Area From Image Mask by Color Changer";
            FormClosing += ImageMaskForm_FormClosing;
            DragDrop += SelectAreaFromImageMaskForm_DragDrop;
            DragEnter += SelectAreaFromImageMaskForm_DragEnter;
            ((System.ComponentModel.ISupportInitialize)previewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)selectedColorBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label2;
        private Button SelectArea;
        private Button OpenFile;
        private PictureBox previewBox;
        private ComboBox selectionType;
        private Label selectedColorLabel;
        private PictureBox selectedColorBox;
    }
}