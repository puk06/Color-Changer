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
            label1 = new Label();
            previewImageMode = new ComboBox();
            refleshButton = new Button();
            panel1 = new Panel();
            viewReset = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Yu Gothic UI", 12F);
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(75, 21);
            label1.TabIndex = 0;
            label1.Text = "画像モード";
            // 
            // previewImageMode
            // 
            previewImageMode.DropDownStyle = ComboBoxStyle.DropDownList;
            previewImageMode.Font = new Font("Yu Gothic UI", 12F);
            previewImageMode.FormattingEnabled = true;
            previewImageMode.Items.AddRange(new object[] { "元画像", "変更後" });
            previewImageMode.Location = new Point(84, 6);
            previewImageMode.Name = "previewImageMode";
            previewImageMode.Size = new Size(121, 29);
            previewImageMode.TabIndex = 1;
            previewImageMode.SelectedIndexChanged += PreviewImageMode_SelectedIndexChanged;
            // 
            // refleshButton
            // 
            refleshButton.Font = new Font("Yu Gothic UI", 11F);
            refleshButton.Location = new Point(211, 7);
            refleshButton.Name = "refleshButton";
            refleshButton.Size = new Size(75, 27);
            refleshButton.TabIndex = 2;
            refleshButton.Text = "更新";
            refleshButton.UseVisualStyleBackColor = true;
            refleshButton.Click += RefleshButton_Click;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(viewReset);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(refleshButton);
            panel1.Controls.Add(previewImageMode);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(434, 43);
            panel1.TabIndex = 3;
            // 
            // viewReset
            // 
            viewReset.Font = new Font("Yu Gothic UI", 10F);
            viewReset.Location = new Point(292, 7);
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
            ClientSize = new Size(434, 411);
            Controls.Add(panel1);
            DoubleBuffered = true;
            Name = "PreviewZoomForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preview Zoom By Color Changer";
            FormClosing += PreviewZoomForm_FormClosing;
            Paint += PreviewZoomForm_Paint;
            MouseDown += PreviewZoomForm_MouseDown;
            MouseMove += PreviewZoomForm_MouseMove;
            MouseUp += PreviewZoomForm_MouseUp;
            MouseWheel += ImageZoomForm_MouseWheel;
            Resize += PreviewZoomForm_Resize;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private ComboBox previewImageMode;
        private Button refleshButton;
        private Panel panel1;
        private Button viewReset;
    }
}