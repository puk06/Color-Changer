namespace ColorChanger.Forms
{
    partial class AdvancedColorSettingsForm
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
            label2 = new Label();
            brightness = new TextBox();
            contrast = new TextBox();
            label3 = new Label();
            gamma = new TextBox();
            label4 = new Label();
            exposure = new TextBox();
            label5 = new Label();
            settingsPanel = new Panel();
            resetButton = new Button();
            settingsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 128);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(136, 30);
            label1.TabIndex = 0;
            label1.Text = "色の追加設定";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 14F);
            label2.Location = new Point(3, 12);
            label2.Name = "label2";
            label2.Size = new Size(50, 25);
            label2.TabIndex = 1;
            label2.Text = "明度";
            // 
            // brightness
            // 
            brightness.Font = new Font("Yu Gothic UI", 14F);
            brightness.Location = new Point(166, 9);
            brightness.Name = "brightness";
            brightness.Size = new Size(112, 32);
            brightness.TabIndex = 2;
            brightness.TextAlign = HorizontalAlignment.Center;
            brightness.KeyDown += HandleKeyDown;
            brightness.Leave += OnValueTextChanged;
            // 
            // contrast
            // 
            contrast.Font = new Font("Yu Gothic UI", 14F);
            contrast.Location = new Point(166, 47);
            contrast.Name = "contrast";
            contrast.Size = new Size(112, 32);
            contrast.TabIndex = 4;
            contrast.TextAlign = HorizontalAlignment.Center;
            contrast.KeyDown += HandleKeyDown;
            contrast.Leave += OnValueTextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 14F);
            label3.Location = new Point(3, 50);
            label3.Name = "label3";
            label3.Size = new Size(92, 25);
            label3.TabIndex = 3;
            label3.Text = "コントラスト";
            // 
            // gamma
            // 
            gamma.Font = new Font("Yu Gothic UI", 14F);
            gamma.Location = new Point(166, 85);
            gamma.Name = "gamma";
            gamma.Size = new Size(112, 32);
            gamma.TabIndex = 10;
            gamma.TextAlign = HorizontalAlignment.Center;
            gamma.KeyDown += HandleKeyDown;
            gamma.Leave += OnValueTextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 14F);
            label4.Location = new Point(3, 88);
            label4.Name = "label4";
            label4.Size = new Size(93, 25);
            label4.TabIndex = 9;
            label4.Text = "ガンマ補正";
            // 
            // exposure
            // 
            exposure.Font = new Font("Yu Gothic UI", 14F);
            exposure.Location = new Point(166, 123);
            exposure.Name = "exposure";
            exposure.Size = new Size(112, 32);
            exposure.TabIndex = 12;
            exposure.TextAlign = HorizontalAlignment.Center;
            exposure.KeyDown += HandleKeyDown;
            exposure.Leave += OnValueTextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Yu Gothic UI", 14F);
            label5.Location = new Point(3, 126);
            label5.Name = "label5";
            label5.Size = new Size(50, 25);
            label5.TabIndex = 11;
            label5.Text = "露出";
            // 
            // settingsPanel
            // 
            settingsPanel.BackColor = Color.Gainsboro;
            settingsPanel.Controls.Add(brightness);
            settingsPanel.Controls.Add(exposure);
            settingsPanel.Controls.Add(label2);
            settingsPanel.Controls.Add(label5);
            settingsPanel.Controls.Add(label3);
            settingsPanel.Controls.Add(gamma);
            settingsPanel.Controls.Add(contrast);
            settingsPanel.Controls.Add(label4);
            settingsPanel.Location = new Point(12, 42);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new Size(290, 298);
            settingsPanel.TabIndex = 13;
            // 
            // resetButton
            // 
            resetButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 128);
            resetButton.Location = new Point(12, 346);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(290, 45);
            resetButton.TabIndex = 13;
            resetButton.Text = "リセット";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += ResetButton_Click;
            // 
            // AdvancedColorSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(314, 394);
            Controls.Add(resetButton);
            Controls.Add(settingsPanel);
            Controls.Add(label1);
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AdvancedColorSettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Advanced Color Settings by Color Changer";
            FormClosing += AdvancedColorSettingsForm_FormClosing;
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox brightness;
        private TextBox contrast;
        private Label label3;
        private TextBox gamma;
        private Label label4;
        private TextBox exposure;
        private Label label5;
        private Panel settingsPanel;
        private Button resetButton;
    }
}