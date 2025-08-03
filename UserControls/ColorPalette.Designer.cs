namespace ColorChanger.UserControls
{
    partial class ColorPalette
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            colorMap = new PictureBox();
            hueSlider = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)colorMap).BeginInit();
            ((System.ComponentModel.ISupportInitialize)hueSlider).BeginInit();
            SuspendLayout();
            // 
            // colorMap
            // 
            colorMap.BackColor = Color.White;
            colorMap.Location = new Point(0, 0);
            colorMap.Name = "colorMap";
            colorMap.Size = new Size(298, 202);
            colorMap.TabIndex = 0;
            colorMap.TabStop = false;
            colorMap.Paint += ColorMap_Paint;
            colorMap.MouseDown += HandleColorMapSelection;
            colorMap.MouseMove += HandleColorMapSelection;
            colorMap.MouseUp += HandleColorMapSelection;
            // 
            // hueSlider
            // 
            hueSlider.BackColor = Color.White;
            hueSlider.Location = new Point(317, 0);
            hueSlider.Name = "hueSlider";
            hueSlider.Size = new Size(30, 202);
            hueSlider.TabIndex = 1;
            hueSlider.TabStop = false;
            hueSlider.Paint += HueSlider_Paint;
            hueSlider.MouseDown += HandleHueColorSelection;
            hueSlider.MouseMove += HandleHueColorSelection;
            hueSlider.MouseUp += HandleHueColorSelection;
            // 
            // ColorPalette
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            Controls.Add(hueSlider);
            Controls.Add(colorMap);
            Name = "ColorPalette";
            Size = new Size(347, 202);
            ((System.ComponentModel.ISupportInitialize)colorMap).EndInit();
            ((System.ComponentModel.ISupportInitialize)hueSlider).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox colorMap;
        private PictureBox hueSlider;
    }
}
