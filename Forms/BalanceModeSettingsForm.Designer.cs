using ColorChanger.Properties;

namespace ColorChanger.Forms
{
    partial class BalanceModeSettingsForm
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
            balanceModeComboBox = new ComboBox();
            balanceModeDescription = new Label();
            balanceModeSettingsTab = new TabControl();
            v1Settings = new TabPage();
            v1minValue = new TextBox();
            label5 = new Label();
            v1weight = new TextBox();
            weightLabel = new Label();
            v2Settings = new TabPage();
            v2includeOutside = new CheckBox();
            v2radiusBar = new TrackBar();
            v2minValue = new TextBox();
            label4 = new Label();
            v2weight = new TextBox();
            label3 = new Label();
            v2radius = new TextBox();
            label2 = new Label();
            v3Settings = new TabPage();
            label9 = new Label();
            gradientPreview = new PictureBox();
            label8 = new Label();
            v3gradientEnd = new TrackBar();
            label7 = new Label();
            v3gradientStart = new TrackBar();
            v3gradientColor = new PictureBox();
            label6 = new Label();
            balanceModeSettingsTab.SuspendLayout();
            v1Settings.SuspendLayout();
            v2Settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)v2radiusBar).BeginInit();
            v3Settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gradientPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientEnd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientColor).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 12F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(139, 21);
            label1.TabIndex = 0;
            label1.Text = "バランスモード計算式";
            // 
            // balanceModeComboBox
            // 
            balanceModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            balanceModeComboBox.Font = new Font("Yu Gothic UI", 10F);
            balanceModeComboBox.FormattingEnabled = true;
            balanceModeComboBox.Items.AddRange(new object[] { "v1", "v2", "v3" });
            balanceModeComboBox.Location = new Point(169, 9);
            balanceModeComboBox.Name = "balanceModeComboBox";
            balanceModeComboBox.Size = new Size(121, 25);
            balanceModeComboBox.TabIndex = 1;
            balanceModeComboBox.SelectedIndexChanged += BalanceModeComboBox_SelectedIndexChanged;
            // 
            // balanceModeDescription
            // 
            balanceModeDescription.Font = new Font("Yu Gothic UI", 11F);
            balanceModeDescription.Location = new Point(12, 46);
            balanceModeDescription.Name = "balanceModeDescription";
            balanceModeDescription.Size = new Size(278, 330);
            balanceModeDescription.TabIndex = 2;
            balanceModeDescription.Text = "モードの説明: ";
            // 
            // balanceModeSettingsTab
            // 
            balanceModeSettingsTab.Controls.Add(v1Settings);
            balanceModeSettingsTab.Controls.Add(v2Settings);
            balanceModeSettingsTab.Controls.Add(v3Settings);
            balanceModeSettingsTab.Font = new Font("Yu Gothic UI", 11F);
            balanceModeSettingsTab.Location = new Point(296, 9);
            balanceModeSettingsTab.Name = "balanceModeSettingsTab";
            balanceModeSettingsTab.SelectedIndex = 0;
            balanceModeSettingsTab.Size = new Size(277, 367);
            balanceModeSettingsTab.TabIndex = 3;
            // 
            // v1Settings
            // 
            v1Settings.Controls.Add(v1minValue);
            v1Settings.Controls.Add(label5);
            v1Settings.Controls.Add(v1weight);
            v1Settings.Controls.Add(weightLabel);
            v1Settings.Location = new Point(4, 29);
            v1Settings.Name = "v1Settings";
            v1Settings.Padding = new Padding(3);
            v1Settings.Size = new Size(269, 334);
            v1Settings.TabIndex = 0;
            v1Settings.Text = "v1";
            v1Settings.UseVisualStyleBackColor = true;
            // 
            // v1minValue
            // 
            v1minValue.Location = new Point(161, 38);
            v1minValue.Name = "v1minValue";
            v1minValue.Size = new Size(102, 27);
            v1minValue.TabIndex = 7;
            v1minValue.KeyDown += HandleKeyDown;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 41);
            label5.Name = "label5";
            label5.Size = new Size(111, 20);
            label5.TabIndex = 6;
            label5.Text = "変化率の最低値";
            // 
            // v1weight
            // 
            v1weight.Location = new Point(161, 5);
            v1weight.Name = "v1weight";
            v1weight.Size = new Size(102, 27);
            v1weight.TabIndex = 1;
            v1weight.KeyDown += HandleKeyDown;
            // 
            // weightLabel
            // 
            weightLabel.AutoSize = true;
            weightLabel.Location = new Point(6, 8);
            weightLabel.Name = "weightLabel";
            weightLabel.Size = new Size(126, 20);
            weightLabel.TabIndex = 0;
            weightLabel.Text = "変化率グラフの重み";
            // 
            // v2Settings
            // 
            v2Settings.Controls.Add(v2includeOutside);
            v2Settings.Controls.Add(v2radiusBar);
            v2Settings.Controls.Add(v2minValue);
            v2Settings.Controls.Add(label4);
            v2Settings.Controls.Add(v2weight);
            v2Settings.Controls.Add(label3);
            v2Settings.Controls.Add(v2radius);
            v2Settings.Controls.Add(label2);
            v2Settings.Location = new Point(4, 29);
            v2Settings.Name = "v2Settings";
            v2Settings.Padding = new Padding(3);
            v2Settings.Size = new Size(269, 334);
            v2Settings.TabIndex = 1;
            v2Settings.Text = "v2";
            v2Settings.UseVisualStyleBackColor = true;
            // 
            // v2includeOutside
            // 
            v2includeOutside.AutoSize = true;
            v2includeOutside.Location = new Point(6, 129);
            v2includeOutside.Name = "v2includeOutside";
            v2includeOutside.Size = new Size(205, 24);
            v2includeOutside.TabIndex = 7;
            v2includeOutside.Text = "範囲外にも最低値を適用する";
            v2includeOutside.UseVisualStyleBackColor = true;
            v2includeOutside.CheckedChanged += V2includeOutside_CheckedChanged;
            // 
            // v2radiusBar
            // 
            v2radiusBar.AutoSize = false;
            v2radiusBar.Location = new Point(6, 37);
            v2radiusBar.Name = "v2radiusBar";
            v2radiusBar.Size = new Size(257, 26);
            v2radiusBar.TabIndex = 6;
            v2radiusBar.MouseUp += V2radiusBar_MouseUp;
            // 
            // v2minValue
            // 
            v2minValue.Location = new Point(161, 96);
            v2minValue.Name = "v2minValue";
            v2minValue.Size = new Size(102, 27);
            v2minValue.TabIndex = 5;
            v2minValue.KeyDown += HandleKeyDown;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 99);
            label4.Name = "label4";
            label4.Size = new Size(111, 20);
            label4.TabIndex = 4;
            label4.Text = "変化率の最低値";
            // 
            // v2weight
            // 
            v2weight.Location = new Point(161, 63);
            v2weight.Name = "v2weight";
            v2weight.Size = new Size(102, 27);
            v2weight.TabIndex = 3;
            v2weight.KeyDown += HandleKeyDown;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 66);
            label3.Name = "label3";
            label3.Size = new Size(126, 20);
            label3.TabIndex = 2;
            label3.Text = "変化率グラフの重み";
            // 
            // v2radius
            // 
            v2radius.Location = new Point(161, 5);
            v2radius.Name = "v2radius";
            v2radius.Size = new Size(102, 27);
            v2radius.TabIndex = 1;
            v2radius.KeyDown += HandleKeyDown;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 8);
            label2.Name = "label2";
            label2.Size = new Size(123, 20);
            label2.TabIndex = 0;
            label2.Text = "球の半径の最大値";
            // 
            // v3Settings
            // 
            v3Settings.BackColor = Color.Transparent;
            v3Settings.Controls.Add(label9);
            v3Settings.Controls.Add(gradientPreview);
            v3Settings.Controls.Add(label8);
            v3Settings.Controls.Add(v3gradientEnd);
            v3Settings.Controls.Add(label7);
            v3Settings.Controls.Add(v3gradientStart);
            v3Settings.Controls.Add(v3gradientColor);
            v3Settings.Controls.Add(label6);
            v3Settings.Location = new Point(4, 29);
            v3Settings.Name = "v3Settings";
            v3Settings.Size = new Size(269, 334);
            v3Settings.TabIndex = 2;
            v3Settings.Text = "v3";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 179);
            label9.Name = "label9";
            label9.Size = new Size(136, 20);
            label9.TabIndex = 12;
            label9.Text = "グラデーションプレビュー";
            // 
            // gradientPreview
            // 
            gradientPreview.Location = new Point(6, 202);
            gradientPreview.Name = "gradientPreview";
            gradientPreview.Size = new Size(257, 40);
            gradientPreview.TabIndex = 11;
            gradientPreview.TabStop = false;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 112);
            label8.Name = "label8";
            label8.Size = new Size(145, 20);
            label8.TabIndex = 10;
            label8.Text = "グラデーション終了位置";
            // 
            // v3gradientEnd
            // 
            v3gradientEnd.AutoSize = false;
            v3gradientEnd.Location = new Point(6, 135);
            v3gradientEnd.Maximum = 100;
            v3gradientEnd.Name = "v3gradientEnd";
            v3gradientEnd.Size = new Size(257, 26);
            v3gradientEnd.TabIndex = 9;
            v3gradientEnd.TickFrequency = 10;
            v3gradientEnd.Value = 100;
            v3gradientEnd.MouseUp += V3gradient_MouseUp;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 45);
            label7.Name = "label7";
            label7.Size = new Size(145, 20);
            label7.TabIndex = 8;
            label7.Text = "グラデーション開始位置";
            // 
            // v3gradientStart
            // 
            v3gradientStart.AutoSize = false;
            v3gradientStart.Location = new Point(6, 68);
            v3gradientStart.Maximum = 100;
            v3gradientStart.Name = "v3gradientStart";
            v3gradientStart.Size = new Size(257, 26);
            v3gradientStart.TabIndex = 7;
            v3gradientStart.TickFrequency = 10;
            v3gradientStart.MouseUp += V3gradient_MouseUp;
            // 
            // v3gradientColor
            // 
            v3gradientColor.BackColor = Color.White;
            v3gradientColor.BorderStyle = BorderStyle.Fixed3D;
            v3gradientColor.Location = new Point(161, 5);
            v3gradientColor.Name = "v3gradientColor";
            v3gradientColor.Size = new Size(102, 27);
            v3gradientColor.TabIndex = 4;
            v3gradientColor.TabStop = false;
            v3gradientColor.Click += V3GradientColor_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 8);
            label6.Name = "label6";
            label6.Size = new Size(142, 20);
            label6.TabIndex = 2;
            label6.Text = "グラデーションの終了色";
            // 
            // BalanceModeSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(583, 385);
            Controls.Add(balanceModeSettingsTab);
            Controls.Add(balanceModeDescription);
            Controls.Add(balanceModeComboBox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BalanceModeSettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Balance Mode Settings by Color Changer";
            FormClosing += BalanceModeSettings_FormClosing;
            balanceModeSettingsTab.ResumeLayout(false);
            v1Settings.ResumeLayout(false);
            v1Settings.PerformLayout();
            v2Settings.ResumeLayout(false);
            v2Settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)v2radiusBar).EndInit();
            v3Settings.ResumeLayout(false);
            v3Settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gradientPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientEnd).EndInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)v3gradientColor).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox balanceModeComboBox;
        private Label balanceModeDescription;
        private TabControl balanceModeSettingsTab;
        private TabPage v1Settings;
        private TextBox v1weight;
        private Label weightLabel;
        private TabPage v2Settings;
        private Label label2;
        private Label label4;
        private TextBox v2weight;
        private Label label3;
        private TextBox v1minValue;
        private Label label5;
        private TextBox v2minValue;
        private TrackBar v2radiusBar;
        private TextBox v2radius;
        private CheckBox v2includeOutside;
        private TabPage v3Settings;
        private PictureBox v3gradientColor;
        private Label label6;
        private Label label7;
        private TrackBar v3gradientStart;
        private Label label9;
        private PictureBox gradientPreview;
        private Label label8;
        private TrackBar v3gradientEnd;
    }
}