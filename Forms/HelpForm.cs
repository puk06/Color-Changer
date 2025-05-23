namespace ColorChanger.Forms;

public partial class HelpForm : Form
{
    private static readonly string BasicUsageMessage = "このソフトの基本的な使い方:\n" +
        "1. 画像を画面内にドラッグ＆ドロップするか、「ファイルを開く」ボタンを押して画像を読み込んでください。\n" +
        "2. 変更前の色は、画像内をクリックまたはドラッグして選択します。\n" +
        "3. 変更後の色は、画面下部のグレーの枠をクリックして選択してください。\n" +
        "4. 「作成」ボタンを押すと、テクスチャの作成が始まります。";

    private static readonly string SelectionModeMessage = "選択モード:\n" +
        "- 選択モードでは、出力時に選択された部分のみ色が変更され、他の場所は変更されません。\n\n" +
        "1. 背景色を右クリックで設定します。\n" +
        "2. 変更したい部分を左クリックで選択してください（右側のプレビュー画像の斜め線が入っていない部分が選択範囲です。複数選択も可能です）。\n" +
        "3. 選択が完了したら、「選択モード」のチェックを外してください。\n\n" +
        "「戻る」ボタンを押すと、最後に選択したエリアから順に選択を解除できます。\n" +
        "画面上部の「選択エリアリスト」ボタンから、選択されたエリアの有効 / 無効を切り替えれます。";

    private static readonly string InvertSelectionMessage = "選択反転モード:\n" +
        "- 選択された部分の色は変わらず、それ以外の場所の色のみ変わります。\n" +
        "- 透過画像作成モードでは、透過する部分が選択部分と逆になります。\n\n" +
        "耳毛など、変えたくない部分が少ない場合に有効的です。\n" +
        "選択モードのみだと、変えたくない部分以外を全て選択した状態にしないといけないので便利です。";

    private static readonly string TransparentModeMessage = "透過画像作成モード:\n" +
        "- 透過画像作成モードでは、選択した部分だけが残る透過画像を生成します。\n\n" +
        "1. 選択モードで、透過させたくない部分を選択します（複数選択可）。\n" +
        "2. 「透過モード」にチェックを入れてください。\n" +
        "3. 「作成」ボタンを押すと、選択された部分だけが残る透過画像が生成されます。";

    private static readonly string BalanceModeMessage = "バランスモード:\n" +
        "- 選択した色に近いほど強く、遠いほど弱く色を変更します。\n\n" +
        "オン／オフを切り替えて、どちらがより自然か確認してから色変換を実行することをおすすめします。\n" +
        "画面上部の「バランスモードの設定」からさまざまな値などを調節することができます。\n" +
        "※「重り」は変えたくない部分が変わってしまう場合は値を大きく、もっと変えたい場合は小さくしてください。0にすると通常モードと同じになります。";

    private static readonly string AdvancedColorSettingsMessage = "色の追加設定:\n" +
        "- ピクセル処理が終わったあとに反映される色の追加設定のことです。\n\n" +
        "変更できる値は、「明度、コントラスト、ガンマ補正、露出」の4つです。\n" +
        "値を変更したらEnterを押すことで更新することができます。\n" +
        "値が変わっていた場合、値の順番に色の計算がされていきます。\n" +
        "※小数点以下の値の調整(1.2など)が想定されており、3などの大きな値の変化は想定されていません。";

    public HelpForm()
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
    #endregion

    #region フォーム関連
    private void HelpForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;
    }
    #endregion
}
