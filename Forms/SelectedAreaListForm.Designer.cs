namespace ColorChanger.Forms
{
    partial class SelectedAreaListForm
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
            selectedValuesList = new CheckedListBox();
            label1 = new Label();
            layerLabel = new Label();
            SuspendLayout();
            // 
            // selectedValuesList
            // 
            selectedValuesList.Font = new Font("Yu Gothic UI", 12F);
            selectedValuesList.FormattingEnabled = true;
            selectedValuesList.HorizontalScrollbar = true;
            selectedValuesList.Location = new Point(12, 74);
            selectedValuesList.Name = "selectedValuesList";
            selectedValuesList.Size = new Size(372, 388);
            selectedValuesList.TabIndex = 0;
            selectedValuesList.ItemCheck += SelectedValuesList_ItemCheck;
            selectedValuesList.SelectedIndexChanged += SelectedValuesList_SelectedIndexChanged;
            selectedValuesList.MouseDown += SelectedValuesList_MouseDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI Semibold", 16F, FontStyle.Bold);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(216, 30);
            label1.TabIndex = 1;
            label1.Text = "選択 / 消去エリア一覧";
            // 
            // layerLabel
            // 
            layerLabel.AutoSize = true;
            layerLabel.Font = new Font("Yu Gothic UI", 11F);
            layerLabel.Location = new Point(9, 52);
            layerLabel.Name = "layerLabel";
            layerLabel.Size = new Size(303, 20);
            layerLabel.TabIndex = 2;
            layerLabel.Text = "優先順位  |   タイプ   |   レイヤー名   /   ピクセル数";
            // 
            // SelectedAreaListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(396, 472);
            Controls.Add(layerLabel);
            Controls.Add(label1);
            Controls.Add(selectedValuesList);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SelectedAreaListForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Selected Area List by Color Changer";
            FormClosing += SelectedAreaListForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox selectedValuesList;
        private Label label1;
        private Label layerLabel;
    }
}