﻿using ColorChanger.Models;
using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class AdvancedColorSettingsForm : Form
{
    /// <summary>
    /// 設定値が変更されたときに発生するイベント
    /// </summary>
    internal event EventHandler? ConfigurationChanged;

    private readonly AdvancedColorConfiguration _configuration = new AdvancedColorConfiguration();

    /// <summary>
    /// 現在の色の設定値
    /// </summary>
    internal AdvancedColorConfiguration Configuration
    {
        get
        {
            SetConfigurationFromInputs();
            ApplyConfigurationToInputs(_configuration);
            return _configuration;
        }
    }

    internal AdvancedColorSettingsForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        ApplyConfigurationToInputs(_configuration);
    }

    /// <summary>
    /// 渡されたConfigからデータを読み込みます。
    /// </summary>
    /// <param name="advancedColorConfiguration"></param>
    internal void LoadSettings(AdvancedColorConfiguration advancedColorConfiguration)
    {
        ApplyConfigurationToInputs(advancedColorConfiguration);
        NotifyConfigurationChanged();
    }

    /// <summary>
    /// フォームの中身をリセットします。
    /// </summary>
    internal void Reset()
    {
        brightness.Text = "1.00";
        contrast.Text = "1.00";
        gamma.Text = "1.00";
        exposure.Text = "0.00";
        transparency.Text = "0.00";

        SetConfigurationFromInputs();

        NotifyConfigurationChanged();
    }

    #region Configuration関連
    private void SetConfigurationFromInputs()
    {
        _configuration.Brightness = MathUtils.ParseDoubleOrDefault(brightness.Text, 1.0);
        _configuration.Contrast = MathUtils.ParseDoubleOrDefault(contrast.Text, 1.0);
        _configuration.Gamma = MathUtils.ParseDoubleOrDefault(gamma.Text, 1.0);
        _configuration.Exposure = MathUtils.ParseDoubleOrDefault(exposure.Text);
        _configuration.Transparency = MathUtils.ParseDoubleOrDefault(transparency.Text);
    }

    private void ApplyConfigurationToInputs(AdvancedColorConfiguration config)
    {
        brightness.Text = config.Brightness.ToString("F2");
        contrast.Text = config.Contrast.ToString("F2");
        gamma.Text = config.Gamma.ToString("F2");
        exposure.Text = config.Exposure.ToString("F2");
        transparency.Text = config.Transparency.ToString("F2");
    }
    #endregion

    #region イベントハンドラー
    private void ResetButton_Click(object sender, EventArgs e)
        => Reset();

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (!FormUtils.IsNavigationKey(e)) return;

        NotifyConfigurationChanged();
    }

    private void OnValueTextChanged(object sender, EventArgs e)
        => NotifyConfigurationChanged();

    private void NotifyConfigurationChanged()
        => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region フォーム関連
    private void AdvancedColorSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
