namespace ColorChanger.Forms
{
    partial class HelpForm
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            descriptionText = new Label();
            label1 = new Label();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Font = new Font("Yu Gothic UI", 12F);
            button1.Location = new Point(12, 9);
            button1.Name = "button1";
            button1.Size = new Size(149, 50);
            button1.TabIndex = 0;
            button1.Text = "基本的な使い方";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // button2
            // 
            button2.Font = new Font("Yu Gothic UI", 12F);
            button2.Location = new Point(12, 65);
            button2.Name = "button2";
            button2.Size = new Size(149, 50);
            button2.TabIndex = 1;
            button2.Text = "選択モード";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            // 
            // button3
            // 
            button3.Font = new Font("Yu Gothic UI", 12F);
            button3.Location = new Point(12, 121);
            button3.Name = "button3";
            button3.Size = new Size(149, 50);
            button3.TabIndex = 2;
            button3.Text = "選択反転モード";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3_Click;
            // 
            // button4
            // 
            button4.Font = new Font("Yu Gothic UI", 12F);
            button4.Location = new Point(12, 177);
            button4.Name = "button4";
            button4.Size = new Size(149, 50);
            button4.TabIndex = 3;
            button4.Text = "透過画像作成モード";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4_Click;
            // 
            // button5
            // 
            button5.Font = new Font("Yu Gothic UI", 12F);
            button5.Location = new Point(12, 233);
            button5.Name = "button5";
            button5.Size = new Size(149, 50);
            button5.TabIndex = 4;
            button5.Text = "バランスモード";
            button5.UseVisualStyleBackColor = true;
            button5.Click += Button5_Click;
            // 
            // descriptionText
            // 
            descriptionText.Font = new Font("Yu Gothic UI", 13F);
            descriptionText.Location = new Point(167, 46);
            descriptionText.Name = "descriptionText";
            descriptionText.Size = new Size(624, 529);
            descriptionText.TabIndex = 5;
            descriptionText.Text = "左の欄から知りたい機能のボタンを押すことで説明を表示することが出来ます。";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label1.Location = new Point(167, 9);
            label1.Name = "label1";
            label1.Size = new Size(119, 30);
            label1.TabIndex = 6;
            label1.Text = "機能の説明";
            // 
            // button6
            // 
            button6.Font = new Font("Yu Gothic UI", 12F);
            button6.Location = new Point(12, 289);
            button6.Name = "button6";
            button6.Size = new Size(149, 50);
            button6.TabIndex = 7;
            button6.Text = "色の追加設定";
            button6.UseVisualStyleBackColor = true;
            button6.Click += Button6_Click;
            // 
            // button7
            // 
            button7.Font = new Font("Yu Gothic UI", 12F);
            button7.Location = new Point(12, 345);
            button7.Name = "button7";
            button7.Size = new Size(149, 50);
            button7.TabIndex = 8;
            button7.Text = "テクスチャから色選択";
            button7.UseVisualStyleBackColor = true;
            button7.Click += Button7_Click;
            // 
            // button8
            // 
            button8.Font = new Font("Yu Gothic UI", 10F);
            button8.Location = new Point(12, 401);
            button8.Name = "button8";
            button8.Size = new Size(149, 50);
            button8.TabIndex = 9;
            button8.Text = "設定の読み込み / 保存";
            button8.UseVisualStyleBackColor = true;
            button8.Click += Button8_Click;
            // 
            // HelpForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(803, 584);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(label1);
            Controls.Add(descriptionText);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = new Icon(new MemoryStream(Properties.Resources.AppIcon));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HelpForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "How to use Color Changer";
            FormClosing += HelpForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Label descriptionText;
        private Label label1;
        private Button button6;
        private Button button7;
        private Button button8;
    }
}