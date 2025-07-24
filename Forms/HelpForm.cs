namespace ColorChanger.Forms;

internal partial class HelpForm : Form
{
    private static readonly string BasicUsageMessage = "このソフトの基本的な使い方:\n" +
        "- 変更前の色と変更後の色の差を全体に適用する、シンプルな色改変のやり方です。\n\n" +
        "1. 画像を画面内にドラッグ＆ドロップするか、画面上部の「ファイル」メニューから「画像ファイルを開く」ボタンを押して画像を読み込んでください。\n" +
        "2. 変更前の色は、画像内をクリックまたはドラッグして選択します。\n" +
        "3. 変更後の色は、画面下部のグレーの枠をクリックして選択してください。\n" +
        "4. 「作成」ボタンを押すと、テクスチャの作成が開始されます。";

    private static readonly string SelectionModeMessage = "選択モード:\n" +
        "- 出力時に、選択された部分のみが処理対象となり、それ以外は処理されません。\n\n" +
        "1. 背景色を右クリックで設定します。\n" +
        "2. 変更したい部分を左クリックで選択してください（右側のプレビュー画像で、斜線のない部分が選択範囲となります。複数選択も可能です）。\n" +
        "3. 選択が完了したら、「選択モード」のチェックを外してください。\n\n" +
        "「戻る」ボタンを押すと、直前に選択したエリアから順に選択を解除できます。\n" +
        "画面上部の「選択エリアリスト」ボタンから、選択されたエリアの有効／無効を切り替えることができます。";

    private static readonly string InvertSelectionMessage = "選択反転モード:\n" +
        "- 選択された部分は処理されず、それ以外の部分のみが処理対象となります。\n" +
        "- 透過画像作成モードでは、透過される部分が選択範囲の逆になります。\n\n" +
        "耳毛など、変更したくない部分が少ない場合に便利です。\n" +
        "選択モードだけでは、変更したくない部分以外をすべて選択する必要があるため、こちらの方が効率的です。";

    private static readonly string TransparentModeMessage = "透過画像作成モード:\n" +
        "- このモードでは、選択した部分のみが残る透過画像を生成します。\n\n" +
        "1. 選択モードで、透過させたくない部分を選択します。\n" +
        "2. 画面右下の「透過画像作成モード」にチェックを入れてください。\n" +
        "3. 「作成」ボタンを押すと、選択された部分のみが残る透過画像が生成されます。";

    private static readonly string BalanceModeMessage = "バランスモード:\n" +
        "- 色変更の計算式を、テクスチャ改変に適した形式に切り替えます。\n\n" +
        "画面上部の「バランスモードの設定」から、計算式や各種パラメータを調整できます。\n" +
        "オン／オフの切り替えや、計算式バージョンの変更によって、色が自然に見えるか確認してから色変換を実行することをおすすめします。\n" +
        "※計算式V1、V2における「重り」の値は、変更したくない部分が変化してしまう場合には大きく、もっと変更したい場合には小さくしてください。0にすると通常モードと同じになります。";

    private static readonly string AdvancedColorSettingsMessage = "色の追加設定:\n" +
        "- ピクセル処理の後に適用される、色の追加調整です。\n\n" +
        "変更可能な項目は、「明度」「コントラスト」「ガンマ補正」「露出」「透明度」の5つです。\n" +
        "値を変更した後、Enter または Tab キーを押して設定を確定してください。\n" +
        "値が変更されていた場合、それぞれの順番に従って色の調整が適用されます。\n" +
        "※調整値は小数点以下（例：1.2など）を想定しています。3.0など大きすぎる値は、予期しない結果になる場合があります。";

    private static readonly string SelectColorFromTextureMessage = "他のテクスチャ画像から色を選択:\n" +
        "- 追加のテクスチャ画像を読み込み、その画像内で選択した色を変更後の色として適用する機能です。\n\n" +
        "1. 通常通り画像を読み込み、あらかじめ変更前の色を設定しておいてください。\n" +
        "2. 画面上部の「ツール」メニューから「テクスチャから色を選択」をクリックします。\n" +
        "3. 表示されたウィンドウに画像をドラッグ＆ドロップするか、画面上部の「ファイル」メニューから「画像ファイルを開く」ボタンをクリックして画像を読み込みます。\n" +
        "4. 画像内をクリックまたはドラッグして色を選択します。\n" +
        "5. 「この色を適用」ボタンをクリックすると、現在選択している色が変更後の色として設定されます。";

    private static readonly string ImportOrExportColorSettingsMessage = "色変更情報を保存する / 読み込む:\n" +
        "- 色変更情報をファイルとして保存したり、保存済みの設定を読み込んだりできます。\n" +
        "    - 誰かと色変更情報を共有したいとき\n" +
        "    - 後から同じ設定を再現したいとき（バックアップとして）\n" +
        "- こんなときに便利です\n\n" +
        "保存方法: \n" +
        "1. 変更前の色、変更後の色、バランスモードなどを設定します。\n" +
        "2. Ctrl + S もしくは、画面上部の「ファイル」メニューから「設定ファイルを出力」を選びます。\n" +
        "3. 保存先とファイル名を指定して保存してください。\n\n" +
        "読み込み方法: \n" +
        "- 設定ファイルをソフトウィンドウにドラッグ＆ドロップ\n" +
        "- Ctrl + O もしくは、画面上部の「ファイル」メニューから「設定ファイルを読み込む」を選んで開いてください。";

    internal HelpForm()
    {
        InitializeComponent();
    }

    #region イベントハンドラー
    private void Button1_Click(object sender, EventArgs e)
    {
        descriptionText.Text = BasicUsageMessage;
    }

    private void Button2_Click(object sender, EventArgs e)
    {
        descriptionText.Text = SelectionModeMessage;
    }

    private void Button3_Click(object sender, EventArgs e)
    {
        descriptionText.Text = InvertSelectionMessage;
    }

    private void Button4_Click(object sender, EventArgs e)
    {
        descriptionText.Text = TransparentModeMessage;
    }

    private void Button5_Click(object sender, EventArgs e)
    {
        descriptionText.Text = BalanceModeMessage;
    }

    private void Button6_Click(object sender, EventArgs e)
    {
        descriptionText.Text = AdvancedColorSettingsMessage;
    }

    private void Button7_Click(object sender, EventArgs e)
    {
        descriptionText.Text = SelectColorFromTextureMessage;
    }

    private void Button8_Click(object sender, EventArgs e)
    {
        descriptionText.Text = ImportOrExportColorSettingsMessage;
    }
    #endregion

    #region フォーム関連
    private void HelpForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
