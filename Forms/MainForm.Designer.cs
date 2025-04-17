namespace VRC_Color_Changer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label2 = new Label();
            label3 = new Label();
            previewBox = new PictureBox();
            label4 = new Label();
            previousColorBox = new PictureBox();
            newColorBox = new PictureBox();
            MakeButton = new Button();
            coloredPreviewBox = new PictureBox();
            label1 = new Label();
            helpUseButton = new Button();
            previousRGBLabel = new Label();
            newRGBLabel = new Label();
            calculatedRGBLabel = new Label();
            openFile = new Button();
            selectMode = new CheckBox();
            backgroundColorBox = new PictureBox();
            backgroundColorLabel = new Label();
            UndoButton = new Button();
            transMode = new CheckBox();
            balanceMode = new CheckBox();
            weightLabel = new Label();
            weightText = new TextBox();
            ((System.ComponentModel.ISupportInitialize)previewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previousColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)newColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)coloredPreviewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)backgroundColorBox).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 13F);
            label2.Location = new Point(12, 447);
            label2.Name = "label2";
            label2.Size = new Size(99, 25);
            label2.TabIndex = 1;
            label2.Text = "変更前の色";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 13F);
            label3.Location = new Point(210, 447);
            label3.Name = "label3";
            label3.Size = new Size(99, 25);
            label3.TabIndex = 2;
            label3.Text = "変更後の色";
            // 
            // previewBox
            // 
            previewBox.BackColor = SystemColors.ActiveBorder;
            previewBox.Location = new Point(12, 44);
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(392, 362);
            previewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            previewBox.TabIndex = 4;
            previewBox.TabStop = false;
            previewBox.Paint += OnPaint;
            previewBox.MouseDown += SetPreviousColor;
            previewBox.MouseMove += SetPreviousColor;
            previewBox.MouseUp += PreviewBox_MouseUp;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 13F);
            label4.Location = new Point(12, 16);
            label4.Name = "label4";
            label4.Size = new Size(356, 25);
            label4.TabIndex = 5;
            label4.Text = "ファイルプレビュー (左が元のファイル、右が変更後)";
            // 
            // previousColorBox
            // 
            previousColorBox.BackColor = SystemColors.ActiveBorder;
            previousColorBox.Location = new Point(117, 432);
            previousColorBox.Name = "previousColorBox";
            previousColorBox.Size = new Size(60, 60);
            previousColorBox.TabIndex = 6;
            previousColorBox.TabStop = false;
            // 
            // newColorBox
            // 
            newColorBox.BackColor = SystemColors.ActiveBorder;
            newColorBox.Location = new Point(315, 432);
            newColorBox.Name = "newColorBox";
            newColorBox.Size = new Size(60, 60);
            newColorBox.TabIndex = 8;
            newColorBox.TabStop = false;
            newColorBox.MouseDown += NewColorBox_MouseDown;
            // 
            // MakeButton
            // 
            MakeButton.Font = new Font("Yu Gothic UI", 15F);
            MakeButton.Location = new Point(687, 432);
            MakeButton.Name = "MakeButton";
            MakeButton.Size = new Size(142, 48);
            MakeButton.TabIndex = 9;
            MakeButton.Text = "作成";
            MakeButton.UseVisualStyleBackColor = true;
            MakeButton.Click += MakeButton_Click;
            // 
            // coloredPreviewBox
            // 
            coloredPreviewBox.BackColor = SystemColors.ActiveBorder;
            coloredPreviewBox.Location = new Point(437, 44);
            coloredPreviewBox.Name = "coloredPreviewBox";
            coloredPreviewBox.Size = new Size(392, 362);
            coloredPreviewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            coloredPreviewBox.TabIndex = 12;
            coloredPreviewBox.TabStop = false;
            coloredPreviewBox.Paint += OnPaint;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 13F);
            label1.Location = new Point(407, 218);
            label1.Name = "label1";
            label1.Size = new Size(30, 25);
            label1.TabIndex = 13;
            label1.Text = "→";
            // 
            // helpUseButton
            // 
            helpUseButton.Location = new Point(700, 3);
            helpUseButton.Name = "helpUseButton";
            helpUseButton.Size = new Size(129, 35);
            helpUseButton.TabIndex = 18;
            helpUseButton.Text = "使い方";
            helpUseButton.UseVisualStyleBackColor = true;
            helpUseButton.Click += HelpUseButton_Click;
            // 
            // previousRGBLabel
            // 
            previousRGBLabel.AutoSize = true;
            previousRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            previousRGBLabel.Location = new Point(12, 501);
            previousRGBLabel.Name = "previousRGBLabel";
            previousRGBLabel.Size = new Size(0, 28);
            previousRGBLabel.TabIndex = 19;
            // 
            // newRGBLabel
            // 
            newRGBLabel.AutoSize = true;
            newRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            newRGBLabel.Location = new Point(210, 501);
            newRGBLabel.Name = "newRGBLabel";
            newRGBLabel.Size = new Size(0, 28);
            newRGBLabel.TabIndex = 20;
            // 
            // calculatedRGBLabel
            // 
            calculatedRGBLabel.AutoSize = true;
            calculatedRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            calculatedRGBLabel.Location = new Point(12, 533);
            calculatedRGBLabel.Name = "calculatedRGBLabel";
            calculatedRGBLabel.Size = new Size(0, 28);
            calculatedRGBLabel.TabIndex = 21;
            // 
            // openFile
            // 
            openFile.Location = new Point(550, 3);
            openFile.Name = "openFile";
            openFile.Size = new Size(129, 35);
            openFile.TabIndex = 22;
            openFile.Text = "ファイルを開く";
            openFile.UseVisualStyleBackColor = true;
            openFile.Click += OpenFile_Click;
            // 
            // selectMode
            // 
            selectMode.AutoSize = true;
            selectMode.Font = new Font("Yu Gothic UI", 13F);
            selectMode.Location = new Point(429, 422);
            selectMode.Name = "selectMode";
            selectMode.Size = new Size(105, 29);
            selectMode.TabIndex = 23;
            selectMode.Text = "選択モード";
            selectMode.UseVisualStyleBackColor = true;
            selectMode.CheckedChanged += SelectMode_CheckedChanged;
            // 
            // backgroundColorBox
            // 
            backgroundColorBox.BackColor = SystemColors.ActiveBorder;
            backgroundColorBox.Enabled = false;
            backgroundColorBox.Location = new Point(429, 457);
            backgroundColorBox.Name = "backgroundColorBox";
            backgroundColorBox.Size = new Size(40, 40);
            backgroundColorBox.TabIndex = 24;
            backgroundColorBox.TabStop = false;
            // 
            // backgroundColorLabel
            // 
            backgroundColorLabel.AutoSize = true;
            backgroundColorLabel.Enabled = false;
            backgroundColorLabel.Font = new Font("Yu Gothic UI", 12F);
            backgroundColorLabel.Location = new Point(473, 466);
            backgroundColorLabel.Name = "backgroundColorLabel";
            backgroundColorLabel.Size = new Size(58, 21);
            backgroundColorLabel.TabIndex = 25;
            backgroundColorLabel.Text = "背景色";
            // 
            // UndoButton
            // 
            UndoButton.Enabled = false;
            UndoButton.Location = new Point(537, 461);
            UndoButton.Name = "UndoButton";
            UndoButton.Size = new Size(85, 34);
            UndoButton.TabIndex = 27;
            UndoButton.Text = "戻る";
            UndoButton.UseVisualStyleBackColor = true;
            UndoButton.Click += UndoButton_Click;
            // 
            // transMode
            // 
            transMode.AutoSize = true;
            transMode.Font = new Font("Yu Gothic UI", 13F);
            transMode.Location = new Point(652, 480);
            transMode.Name = "transMode";
            transMode.Size = new Size(177, 29);
            transMode.TabIndex = 28;
            transMode.Text = "透過画像作成モード";
            transMode.UseVisualStyleBackColor = true;
            // 
            // balanceMode
            // 
            balanceMode.AutoSize = true;
            balanceMode.Font = new Font("Yu Gothic UI", 13F);
            balanceMode.Location = new Point(652, 514);
            balanceMode.Name = "balanceMode";
            balanceMode.Size = new Size(123, 29);
            balanceMode.TabIndex = 29;
            balanceMode.Text = "バランスモード";
            balanceMode.UseVisualStyleBackColor = true;
            balanceMode.CheckedChanged += BalanceMode_CheckedChanged;
            // 
            // weightLabel
            // 
            weightLabel.AutoSize = true;
            weightLabel.Font = new Font("Yu Gothic UI", 13F);
            weightLabel.Location = new Point(407, 516);
            weightLabel.Name = "weightLabel";
            weightLabel.Size = new Size(153, 25);
            weightLabel.TabIndex = 31;
            weightLabel.Text = "バランスモードの重み";
            // 
            // weightText
            // 
            weightText.Font = new Font("Yu Gothic UI", 12F);
            weightText.Location = new Point(563, 514);
            weightText.Name = "weightText";
            weightText.Size = new Size(79, 29);
            weightText.TabIndex = 32;
            weightText.Text = "1.00";
            weightText.TextAlign = HorizontalAlignment.Center;
            weightText.KeyDown += WeightText_KeyDown;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(846, 570);
            Controls.Add(weightText);
            Controls.Add(weightLabel);
            Controls.Add(balanceMode);
            Controls.Add(transMode);
            Controls.Add(UndoButton);
            Controls.Add(backgroundColorLabel);
            Controls.Add(backgroundColorBox);
            Controls.Add(selectMode);
            Controls.Add(openFile);
            Controls.Add(calculatedRGBLabel);
            Controls.Add(newRGBLabel);
            Controls.Add(previousRGBLabel);
            Controls.Add(helpUseButton);
            Controls.Add(label1);
            Controls.Add(coloredPreviewBox);
            Controls.Add(MakeButton);
            Controls.Add(newColorBox);
            Controls.Add(previousColorBox);
            Controls.Add(label4);
            Controls.Add(previewBox);
            Controls.Add(label3);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "VRC Color Changer";
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            ((System.ComponentModel.ISupportInitialize)previewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)previousColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)newColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)coloredPreviewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)backgroundColorBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private Label label3;
        private PictureBox previewBox;
        private Label label4;
        private PictureBox previousColorBox;
        private PictureBox newColorBox;
        private Button MakeButton;
        private PictureBox coloredPreviewBox;
        private Label label1;
        private Button helpUseButton;
        private Label previousRGBLabel;
        private Label newRGBLabel;
        private Label calculatedRGBLabel;
        private Button openFile;
        private CheckBox selectMode;
        private PictureBox backgroundColorBox;
        private Label backgroundColorLabel;
        private Button UndoButton;
        private CheckBox transMode;
        private CheckBox balanceMode;
        private Label weightLabel;
        private TextBox weightText;
    }
}
