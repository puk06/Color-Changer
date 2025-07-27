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
            SuspendLayout();
            // 
            // selectedValuesList
            // 
            selectedValuesList.Font = new Font("Yu Gothic UI", 12F);
            selectedValuesList.FormattingEnabled = true;
            selectedValuesList.Location = new Point(12, 42);
            selectedValuesList.Name = "selectedValuesList";
            selectedValuesList.Size = new Size(433, 388);
            selectedValuesList.TabIndex = 0;
            selectedValuesList.ItemCheck += SelectedValuesList_ItemCheck;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 13F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(125, 25);
            label1.TabIndex = 1;
            label1.Text = "選択エリア一覧";
            // 
            // SelectedAreaListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(457, 450);
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
    }
}