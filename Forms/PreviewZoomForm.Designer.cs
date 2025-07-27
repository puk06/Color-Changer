namespace ColorChanger.Forms
{
    partial class PreviewZoomForm
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
            previewImageMode = new ComboBox();
            refleshButton = new Button();
            panel = new Panel();
            howToUse = new Button();
            viewReset = new Button();
            panel.SuspendLayout();
            SuspendLayout();
            // 
            // previewImageMode
            // 
            previewImageMode.DropDownStyle = ComboBoxStyle.DropDownList;
            previewImageMode.Font = new Font("Yu Gothic UI", 12F);
            previewImageMode.FormattingEnabled = true;
            previewImageMode.Items.AddRange(new object[] { "元画像", "変更後" });
            previewImageMode.Location = new Point(6, 6);
            previewImageMode.Name = "previewImageMode";
            previewImageMode.Size = new Size(121, 29);
            previewImageMode.TabIndex = 1;
            previewImageMode.SelectedIndexChanged += PreviewImageMode_SelectedIndexChanged;
            // 
            // refleshButton
            // 
            refleshButton.Font = new Font("Yu Gothic UI", 11F);
            refleshButton.Location = new Point(133, 7);
            refleshButton.Name = "refleshButton";
            refleshButton.Size = new Size(75, 27);
            refleshButton.TabIndex = 2;
            refleshButton.Text = "更新";
            refleshButton.UseVisualStyleBackColor = true;
            refleshButton.Click += RefleshButton_Click;
            // 
            // panel
            // 
            panel.BackColor = SystemColors.Control;
            panel.Controls.Add(howToUse);
            panel.Controls.Add(viewReset);
            panel.Controls.Add(refleshButton);
            panel.Controls.Add(previewImageMode);
            panel.Dock = DockStyle.Top;
            panel.Location = new Point(0, 0);
            panel.Name = "panel1";
            panel.Size = new Size(434, 43);
            panel.TabIndex = 3;
            // 
            // howToUse
            // 
            howToUse.Font = new Font("Yu Gothic UI", 10F);
            howToUse.Location = new Point(320, 7);
            howToUse.Name = "howToUse";
            howToUse.Size = new Size(102, 27);
            howToUse.TabIndex = 4;
            howToUse.Text = "操作方法";
            howToUse.UseVisualStyleBackColor = true;
            howToUse.Click += HowToUse_Click;
            // 
            // viewReset
            // 
            viewReset.Font = new Font("Yu Gothic UI", 10F);
            viewReset.Location = new Point(214, 6);
            viewReset.Name = "viewReset";
            viewReset.Size = new Size(102, 27);
            viewReset.TabIndex = 3;
            viewReset.Text = "ビューリセット";
            viewReset.UseVisualStyleBackColor = true;
            viewReset.Click += ViewReset_Click;
            // 
            // PreviewZoomForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Size = new Size(800, 800);
            Controls.Add(panel);
            DoubleBuffered = true;
            Name = "PreviewZoomForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Zoom Preview By Color Changer";
            FormClosing += PreviewZoomForm_FormClosing;
            Paint += PreviewZoomForm_Paint;
            MouseDown += PreviewZoomForm_MouseDown;
            MouseMove += PreviewZoomForm_MouseMove;
            MouseUp += PreviewZoomForm_MouseUp;
            MouseWheel += ImageZoomForm_MouseWheel;
            Resize += PreviewZoomForm_Resize;
            panel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private ComboBox previewImageMode;
        private Button refleshButton;
        private Panel panel;
        private Button viewReset;
        private Button howToUse;
    }
}