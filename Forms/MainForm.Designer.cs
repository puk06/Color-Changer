﻿namespace ColorChanger.Forms
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
            makeButton = new Button();
            coloredPreviewBox = new PictureBox();
            previousRGBLabel = new Label();
            newRGBLabel = new Label();
            calculatedRGBLabel = new Label();
            selectMode = new CheckBox();
            backgroundColorBox = new PictureBox();
            backgroundColorLabel = new Label();
            undoButton = new Button();
            transparentMode = new CheckBox();
            balanceMode = new CheckBox();
            inverseMode = new CheckBox();
            menuToolBar = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            OpenFile = new ToolStripMenuItem();
            separator = new ToolStripSeparator();
            ImportColorSettings = new ToolStripMenuItem();
            ExportColorSettings = new ToolStripMenuItem();
            balanceModeSettingsButton = new ToolStripMenuItem();
            selectedAreaListButton = new ToolStripMenuItem();
            advancedColorSettingsButton = new ToolStripMenuItem();
            toolMenu = new ToolStripMenuItem();
            selectColorFromTexture = new ToolStripMenuItem();
            selectionPenTool = new ToolStripMenuItem();
            previewZoomTool = new ToolStripMenuItem();
            helpUseButton = new ToolStripMenuItem();
            aboutThisSoftware = new ToolStripMenuItem();
            donationButton = new ToolStripMenuItem();
            label5 = new Label();
            panel1 = new Panel();
            label17 = new Label();
            textureType = new Label();
            label16 = new Label();
            memoryUsage = new Label();
            label13 = new Label();
            label15 = new Label();
            textureResolution = new Label();
            label11 = new Label();
            versionLabel = new Label();
            label10 = new Label();
            label14 = new Label();
            advancedColorConfigStatus = new Label();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label1 = new Label();
            selectModePanel = new Panel();
            label12 = new Label();
            updateCheck = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)previewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)previousColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)newColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)coloredPreviewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)backgroundColorBox).BeginInit();
            menuToolBar.SuspendLayout();
            panel1.SuspendLayout();
            selectModePanel.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 15.75F);
            label2.Location = new Point(69, 520);
            label2.Name = "label2";
            label2.Size = new Size(114, 30);
            label2.TabIndex = 1;
            label2.Text = "変更前の色";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 15.75F);
            label3.Location = new Point(271, 520);
            label3.Name = "label3";
            label3.Size = new Size(114, 30);
            label3.TabIndex = 2;
            label3.Text = "変更後の色";
            // 
            // previewBox
            // 
            previewBox.BackColor = SystemColors.ActiveBorder;
            previewBox.Location = new Point(12, 59);
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(450, 450);
            previewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            previewBox.TabIndex = 4;
            previewBox.TabStop = false;
            previewBox.Paint += OnPaint;
            previewBox.MouseUp += PreviewBox_MouseUp;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 13F, FontStyle.Bold);
            label4.Location = new Point(12, 31);
            label4.Name = "label4";
            label4.Size = new Size(83, 25);
            label4.TabIndex = 5;
            label4.Text = "元ファイル";
            // 
            // previousColorBox
            // 
            previousColorBox.BackColor = SystemColors.ActiveBorder;
            previousColorBox.Location = new Point(90, 553);
            previousColorBox.Name = "previousColorBox";
            previousColorBox.Size = new Size(70, 70);
            previousColorBox.TabIndex = 6;
            previousColorBox.TabStop = false;
            // 
            // newColorBox
            // 
            newColorBox.BackColor = SystemColors.ActiveBorder;
            newColorBox.Location = new Point(294, 553);
            newColorBox.Name = "newColorBox";
            newColorBox.Size = new Size(70, 70);
            newColorBox.TabIndex = 8;
            newColorBox.TabStop = false;
            newColorBox.MouseDown += NewColorBox_MouseDown;
            // 
            // makeButton
            // 
            makeButton.BackColor = Color.Gray;
            makeButton.Dock = DockStyle.Bottom;
            makeButton.FlatAppearance.BorderSize = 0;
            makeButton.FlatStyle = FlatStyle.Flat;
            makeButton.Font = new Font("Yu Gothic UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 128);
            makeButton.ForeColor = SystemColors.Control;
            makeButton.Location = new Point(0, 579);
            makeButton.Name = "makeButton";
            makeButton.Size = new Size(287, 62);
            makeButton.TabIndex = 9;
            makeButton.Text = "作成";
            makeButton.UseVisualStyleBackColor = false;
            makeButton.Click += MakeButton_Click;
            // 
            // coloredPreviewBox
            // 
            coloredPreviewBox.BackColor = SystemColors.ActiveBorder;
            coloredPreviewBox.Location = new Point(486, 59);
            coloredPreviewBox.Name = "coloredPreviewBox";
            coloredPreviewBox.Size = new Size(450, 450);
            coloredPreviewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            coloredPreviewBox.TabIndex = 12;
            coloredPreviewBox.TabStop = false;
            coloredPreviewBox.Paint += OnPaint;
            coloredPreviewBox.MouseUp += PreviewBox_MouseUp;
            // 
            // previousRGBLabel
            // 
            previousRGBLabel.AutoSize = true;
            previousRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            previousRGBLabel.ForeColor = Color.White;
            previousRGBLabel.Location = new Point(88, 254);
            previousRGBLabel.Name = "previousRGBLabel";
            previousRGBLabel.Size = new Size(68, 28);
            previousRGBLabel.TabIndex = 19;
            previousRGBLabel.Text = "Empty";
            // 
            // newRGBLabel
            // 
            newRGBLabel.AutoSize = true;
            newRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            newRGBLabel.ForeColor = Color.White;
            newRGBLabel.Location = new Point(88, 282);
            newRGBLabel.Name = "newRGBLabel";
            newRGBLabel.Size = new Size(68, 28);
            newRGBLabel.TabIndex = 20;
            newRGBLabel.Text = "Empty";
            // 
            // calculatedRGBLabel
            // 
            calculatedRGBLabel.AutoSize = true;
            calculatedRGBLabel.Font = new Font("Yu Gothic UI", 15F);
            calculatedRGBLabel.ForeColor = Color.White;
            calculatedRGBLabel.Location = new Point(88, 310);
            calculatedRGBLabel.Name = "calculatedRGBLabel";
            calculatedRGBLabel.Size = new Size(68, 28);
            calculatedRGBLabel.TabIndex = 21;
            calculatedRGBLabel.Text = "Empty";
            // 
            // selectMode
            // 
            selectMode.AutoSize = true;
            selectMode.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold);
            selectMode.Location = new Point(486, 537);
            selectMode.Name = "selectMode";
            selectMode.Size = new Size(111, 29);
            selectMode.TabIndex = 23;
            selectMode.Text = "選択モード";
            selectMode.UseVisualStyleBackColor = true;
            selectMode.CheckedChanged += SelectMode_CheckedChanged;
            // 
            // backgroundColorBox
            // 
            backgroundColorBox.BackColor = SystemColors.ActiveBorder;
            backgroundColorBox.Location = new Point(184, 35);
            backgroundColorBox.Name = "backgroundColorBox";
            backgroundColorBox.Size = new Size(50, 50);
            backgroundColorBox.TabIndex = 24;
            backgroundColorBox.TabStop = false;
            // 
            // backgroundColorLabel
            // 
            backgroundColorLabel.AutoSize = true;
            backgroundColorLabel.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold);
            backgroundColorLabel.Location = new Point(175, 7);
            backgroundColorLabel.Name = "backgroundColorLabel";
            backgroundColorLabel.Size = new Size(69, 25);
            backgroundColorLabel.TabIndex = 25;
            backgroundColorLabel.Text = "背景色";
            // 
            // undoButton
            // 
            undoButton.Font = new Font("Yu Gothic UI", 12F);
            undoButton.Location = new Point(32, 40);
            undoButton.Name = "undoButton";
            undoButton.Size = new Size(99, 45);
            undoButton.TabIndex = 27;
            undoButton.Text = "戻る";
            undoButton.UseVisualStyleBackColor = true;
            undoButton.Click += UndoButton_Click;
            // 
            // transparentMode
            // 
            transparentMode.AutoSize = true;
            transparentMode.Font = new Font("Yu Gothic UI", 13F);
            transparentMode.ForeColor = Color.White;
            transparentMode.Location = new Point(18, 502);
            transparentMode.Name = "transparentMode";
            transparentMode.Size = new Size(177, 29);
            transparentMode.TabIndex = 28;
            transparentMode.Text = "透過画像作成モード";
            transparentMode.UseVisualStyleBackColor = true;
            // 
            // balanceMode
            // 
            balanceMode.AutoSize = true;
            balanceMode.Font = new Font("Yu Gothic UI", 13F);
            balanceMode.ForeColor = Color.White;
            balanceMode.Location = new Point(18, 420);
            balanceMode.Name = "balanceMode";
            balanceMode.Size = new Size(123, 29);
            balanceMode.TabIndex = 29;
            balanceMode.Text = "バランスモード";
            balanceMode.UseVisualStyleBackColor = true;
            balanceMode.CheckedChanged += BalanceMode_CheckedChanged;
            // 
            // inverseMode
            // 
            inverseMode.AutoSize = true;
            inverseMode.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold);
            inverseMode.Location = new Point(486, 575);
            inverseMode.Name = "inverseMode";
            inverseMode.Size = new Size(149, 29);
            inverseMode.TabIndex = 33;
            inverseMode.Text = "選択反転モード";
            inverseMode.UseVisualStyleBackColor = true;
            inverseMode.CheckedChanged += InverseMode_CheckedChanged;
            // 
            // menuToolBar
            // 
            menuToolBar.Font = new Font("Yu Gothic UI", 19F);
            menuToolBar.Items.AddRange(new ToolStripItem[] { fileMenu, balanceModeSettingsButton, selectedAreaListButton, advancedColorSettingsButton, toolMenu, helpUseButton, aboutThisSoftware, updateCheck, donationButton });
            menuToolBar.Location = new Point(0, 0);
            menuToolBar.Name = "menuToolBar";
            menuToolBar.Size = new Size(1256, 24);
            menuToolBar.TabIndex = 38;
            menuToolBar.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { OpenFile, separator, ImportColorSettings, ExportColorSettings });
            fileMenu.Font = new Font("Yu Gothic UI", 9F);
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(53, 20);
            fileMenu.Text = "ファイル";
            // 
            // OpenFile
            // 
            OpenFile.Name = "OpenFile";
            OpenFile.Size = new Size(186, 22);
            OpenFile.Text = "画像ファイルを開く";
            OpenFile.Click += OpenFile_Click;
            // 
            // separator
            // 
            separator.Name = "separator";
            separator.Size = new Size(183, 6);
            // 
            // ImportColorSettings
            // 
            ImportColorSettings.Name = "ImportColorSettings";
            ImportColorSettings.Size = new Size(186, 22);
            ImportColorSettings.Text = "設定ファイルを読み込む";
            ImportColorSettings.Click += ImportColorSettings_Click;
            // 
            // ExportColorSettings
            // 
            ExportColorSettings.Name = "ExportColorSettings";
            ExportColorSettings.Size = new Size(186, 22);
            ExportColorSettings.Text = "設定ファイルを出力する";
            ExportColorSettings.Click += ExportColorSettings_Click;
            // 
            // balanceModeSettingsButton
            // 
            balanceModeSettingsButton.Font = new Font("Yu Gothic UI", 9F);
            balanceModeSettingsButton.Name = "balanceModeSettingsButton";
            balanceModeSettingsButton.Size = new Size(114, 20);
            balanceModeSettingsButton.Text = "バランスモードの設定";
            balanceModeSettingsButton.Click += BalanceModeSettingsButton_Click;
            // 
            // selectedAreaListButton
            // 
            selectedAreaListButton.Font = new Font("Yu Gothic UI", 9F);
            selectedAreaListButton.Name = "selectedAreaListButton";
            selectedAreaListButton.Size = new Size(94, 20);
            selectedAreaListButton.Text = "選択エリアリスト";
            selectedAreaListButton.Click += SelectedAreaListButton_Click;
            // 
            // advancedColorSettingsButton
            // 
            advancedColorSettingsButton.Font = new Font("Yu Gothic UI", 9F);
            advancedColorSettingsButton.Name = "advancedColorSettingsButton";
            advancedColorSettingsButton.Size = new Size(89, 20);
            advancedColorSettingsButton.Text = "色の追加設定";
            advancedColorSettingsButton.Click += AdvancedColorSettingsButton_Click;
            // 
            // toolMenu
            // 
            toolMenu.DropDownItems.AddRange(new ToolStripItem[] { selectColorFromTexture, selectionPenTool, previewZoomTool });
            toolMenu.Font = new Font("Yu Gothic UI", 9F);
            toolMenu.Name = "toolMenu";
            toolMenu.Size = new Size(46, 20);
            toolMenu.Text = "ツール";
            // 
            // selectColorFromTexture
            // 
            selectColorFromTexture.Name = "selectColorFromTexture";
            selectColorFromTexture.Size = new Size(182, 22);
            selectColorFromTexture.Text = "テクスチャから色を選択";
            selectColorFromTexture.Click += SelectColorFromTexture_Click;
            // 
            // selectionPenTool
            // 
            selectionPenTool.Name = "selectionPenTool";
            selectionPenTool.Size = new Size(182, 22);
            selectionPenTool.Text = "選択ペンツール";
            selectionPenTool.Click += SelectionPenTool_Click;
            // 
            // previewZoomTool
            // 
            previewZoomTool.Name = "previewZoomTool";
            previewZoomTool.Size = new Size(182, 22);
            previewZoomTool.Text = "拡大プレビュー";
            previewZoomTool.Click += PreviewZoomTool_Click;
            // 
            // helpUseButton
            // 
            helpUseButton.Font = new Font("Yu Gothic UI", 9F);
            helpUseButton.Name = "helpUseButton";
            helpUseButton.Size = new Size(53, 20);
            helpUseButton.Text = "使い方";
            helpUseButton.Click += HelpUseButton_Click;
            // 
            // aboutThisSoftware
            // 
            aboutThisSoftware.Font = new Font("Yu Gothic UI", 9F);
            aboutThisSoftware.Name = "aboutThisSoftware";
            aboutThisSoftware.Size = new Size(99, 20);
            aboutThisSoftware.Text = "このソフトについて";
            aboutThisSoftware.Click += AboutThisSoftware_Click;
            // 
            // donationButton
            // 
            donationButton.Font = new Font("Yu Gothic UI", 9F);
            donationButton.Name = "donationButton";
            donationButton.Size = new Size(43, 20);
            donationButton.Text = "支援";
            donationButton.Click += DonationButton_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label5.ForeColor = Color.White;
            label5.Location = new Point(9, 469);
            label5.Name = "label5";
            label5.Size = new Size(183, 30);
            label5.TabIndex = 39;
            label5.Text = "画像生成オプション";
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.WindowFrame;
            panel1.Controls.Add(label17);
            panel1.Controls.Add(textureType);
            panel1.Controls.Add(label16);
            panel1.Controls.Add(memoryUsage);
            panel1.Controls.Add(label13);
            panel1.Controls.Add(label15);
            panel1.Controls.Add(textureResolution);
            panel1.Controls.Add(label11);
            panel1.Controls.Add(versionLabel);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(label14);
            panel1.Controls.Add(advancedColorConfigStatus);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(transparentMode);
            panel1.Controls.Add(balanceMode);
            panel1.Controls.Add(makeButton);
            panel1.Controls.Add(previousRGBLabel);
            panel1.Controls.Add(newRGBLabel);
            panel1.Controls.Add(calculatedRGBLabel);
            panel1.Location = new Point(969, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(287, 641);
            panel1.TabIndex = 40;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Yu Gothic UI", 15F);
            label17.ForeColor = Color.White;
            label17.Location = new Point(12, 175);
            label17.Name = "label17";
            label17.Size = new Size(104, 28);
            label17.TabIndex = 67;
            label17.Text = "フォーマット :";
            // 
            // textureType
            // 
            textureType.AutoSize = true;
            textureType.Font = new Font("Yu Gothic UI", 15F);
            textureType.ForeColor = Color.White;
            textureType.Location = new Point(111, 175);
            textureType.Name = "textureType";
            textureType.Size = new Size(68, 28);
            textureType.TabIndex = 66;
            textureType.Text = "Empty";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Yu Gothic UI", 15F);
            label16.ForeColor = Color.White;
            label16.Location = new Point(12, 147);
            label16.Name = "label16";
            label16.Size = new Size(124, 28);
            label16.TabIndex = 65;
            label16.Text = "メモリ使用量 :";
            // 
            // memoryUsage
            // 
            memoryUsage.AutoSize = true;
            memoryUsage.Font = new Font("Yu Gothic UI", 15F);
            memoryUsage.ForeColor = Color.White;
            memoryUsage.Location = new Point(131, 147);
            memoryUsage.Name = "memoryUsage";
            memoryUsage.Size = new Size(68, 28);
            memoryUsage.TabIndex = 64;
            memoryUsage.Text = "Empty";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label13.ForeColor = Color.White;
            label13.Location = new Point(9, 89);
            label13.Name = "label13";
            label13.Size = new Size(141, 30);
            label13.TabIndex = 63;
            label13.Text = "テクスチャ情報";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Yu Gothic UI", 15F);
            label15.ForeColor = Color.White;
            label15.Location = new Point(12, 119);
            label15.Name = "label15";
            label15.Size = new Size(81, 28);
            label15.TabIndex = 62;
            label15.Text = "解像度 :";
            // 
            // textureResolution
            // 
            textureResolution.AutoSize = true;
            textureResolution.Font = new Font("Yu Gothic UI", 15F);
            textureResolution.ForeColor = Color.White;
            textureResolution.Location = new Point(88, 119);
            textureResolution.Name = "textureResolution";
            textureResolution.Size = new Size(68, 28);
            textureResolution.TabIndex = 61;
            textureResolution.Text = "Empty";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Yu Gothic UI", 27F, FontStyle.Bold);
            label11.ForeColor = Color.White;
            label11.Location = new Point(9, 6);
            label11.Name = "label11";
            label11.Size = new Size(255, 48);
            label11.TabIndex = 51;
            label11.Text = "Color Changer";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Font = new Font("Yu Gothic UI", 14F, FontStyle.Bold);
            versionLabel.ForeColor = Color.White;
            versionLabel.Location = new Point(179, 54);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(96, 25);
            versionLabel.TabIndex = 52;
            versionLabel.Text = "v{version}";
            versionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label10.ForeColor = Color.White;
            label10.Location = new Point(9, 387);
            label10.Name = "label10";
            label10.Size = new Size(161, 30);
            label10.TabIndex = 60;
            label10.Text = "色変更オプション";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Yu Gothic UI", 15F);
            label14.ForeColor = Color.White;
            label14.Location = new Point(12, 338);
            label14.Name = "label14";
            label14.Size = new Size(101, 28);
            label14.TabIndex = 59;
            label14.Text = "追加設定 :";
            // 
            // advancedColorConfigStatus
            // 
            advancedColorConfigStatus.AutoSize = true;
            advancedColorConfigStatus.Font = new Font("Yu Gothic UI", 15F);
            advancedColorConfigStatus.ForeColor = Color.White;
            advancedColorConfigStatus.Location = new Point(108, 338);
            advancedColorConfigStatus.Name = "advancedColorConfigStatus";
            advancedColorConfigStatus.Size = new Size(52, 28);
            advancedColorConfigStatus.TabIndex = 58;
            advancedColorConfigStatus.Text = "無効";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Yu Gothic UI", 16F, FontStyle.Bold);
            label9.ForeColor = Color.White;
            label9.Location = new Point(9, 224);
            label9.Name = "label9";
            label9.Size = new Size(165, 30);
            label9.TabIndex = 43;
            label9.Text = "RGB色変更情報";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Yu Gothic UI", 15F);
            label8.ForeColor = Color.White;
            label8.Location = new Point(12, 310);
            label8.Name = "label8";
            label8.Size = new Size(81, 28);
            label8.TabIndex = 42;
            label8.Text = "計算後 :";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Yu Gothic UI", 15F);
            label7.ForeColor = Color.White;
            label7.Location = new Point(12, 282);
            label7.Name = "label7";
            label7.Size = new Size(81, 28);
            label7.TabIndex = 41;
            label7.Text = "変更後 :";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Yu Gothic UI", 15F);
            label6.ForeColor = Color.White;
            label6.Location = new Point(12, 254);
            label6.Name = "label6";
            label6.Size = new Size(81, 28);
            label6.TabIndex = 40;
            label6.Text = "変更前 :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 13F, FontStyle.Bold);
            label1.Location = new Point(486, 31);
            label1.Name = "label1";
            label1.Size = new Size(66, 25);
            label1.TabIndex = 41;
            label1.Text = "変更後";
            // 
            // selectModePanel
            // 
            selectModePanel.BackColor = Color.Gainsboro;
            selectModePanel.Controls.Add(label12);
            selectModePanel.Controls.Add(backgroundColorLabel);
            selectModePanel.Controls.Add(backgroundColorBox);
            selectModePanel.Controls.Add(undoButton);
            selectModePanel.Enabled = false;
            selectModePanel.Location = new Point(645, 525);
            selectModePanel.Name = "selectModePanel";
            selectModePanel.Size = new Size(291, 100);
            selectModePanel.TabIndex = 42;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 128);
            label12.Location = new Point(36, 12);
            label12.Name = "label12";
            label12.Size = new Size(92, 25);
            label12.TabIndex = 28;
            label12.Text = "選択モード";
            // 
            // updateCheck
            // 
            updateCheck.Font = new Font("Yu Gothic UI", 9F);
            updateCheck.Name = "updateCheck";
            updateCheck.Size = new Size(102, 20);
            updateCheck.Text = "アップデートチェック";
            updateCheck.Click += UpdateCheck_Click;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1256, 639);
            Controls.Add(selectModePanel);
            Controls.Add(label1);
            Controls.Add(panel1);
            Controls.Add(inverseMode);
            Controls.Add(selectMode);
            Controls.Add(coloredPreviewBox);
            Controls.Add(newColorBox);
            Controls.Add(previousColorBox);
            Controls.Add(label4);
            Controls.Add(previewBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(menuToolBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MainMenuStrip = menuToolBar;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Color Changer For Texture v{version}";
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            KeyDown += MainForm_KeyDown;
            ((System.ComponentModel.ISupportInitialize)previewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)previousColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)newColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)coloredPreviewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)backgroundColorBox).EndInit();
            menuToolBar.ResumeLayout(false);
            menuToolBar.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            selectModePanel.ResumeLayout(false);
            selectModePanel.PerformLayout();
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
        private Button makeButton;
        private PictureBox coloredPreviewBox;
        private Label previousRGBLabel;
        private Label newRGBLabel;
        private Label calculatedRGBLabel;
        private CheckBox selectMode;
        private PictureBox backgroundColorBox;
        private Label backgroundColorLabel;
        private Button undoButton;
        private CheckBox transparentMode;
        private CheckBox balanceMode;
        private CheckBox inverseMode;
        private MenuStrip menuToolBar;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem helpUseButton;
        private ToolStripMenuItem aboutThisSoftware;
        private Label label5;
        private Panel panel1;
        private ToolStripMenuItem balanceModeSettingsButton;
        private Label label6;
        private Label label7;
        private Label label9;
        private Label label8;
        private ToolStripMenuItem selectedAreaListButton;
        private Label label1;
        private Label label11;
        private Label versionLabel;
        private ToolStripMenuItem donationButton;
        private Panel selectModePanel;
        private Label label12;
        private ToolStripMenuItem advancedColorSettingsButton;
        private Label label14;
        private Label advancedColorConfigStatus;
        private Label label10;
        private Label label13;
        private Label label15;
        private Label textureResolution;
        private Label label16;
        private Label memoryUsage;
        private Label label17;
        private Label textureType;
        private ToolStripMenuItem toolMenu;
        private ToolStripMenuItem selectColorFromTexture;
        private ToolStripMenuItem OpenFile;
        private ToolStripMenuItem ImportColorSettings;
        private ToolStripMenuItem ExportColorSettings;
        private ToolStripMenuItem selectionPenTool;
        private ToolStripSeparator separator;
        private ToolStripMenuItem previewZoomTool;
        private ToolStripMenuItem updateCheck;
    }
}
