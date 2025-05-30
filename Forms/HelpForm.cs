namespace ColorChanger.Forms;

public partial class HelpForm : Form
{
    private static readonly string BasicUsageMessage = "このソフトの基本的な使い方:\n" +
        "1. 画像を画面内にドラッグ＆ドロップするか、「ファイルを開く」ボタンを押して画像を読み込んでください。\n" +
        "2. 変更前の色は、画像内をクリックまたはドラッグして選択します。\n" +
        "3. 変更後の色は、画面下部のグレーの枠をクリックして選択してください。\n" +
        "4. 「作成」ボタンを押すと、テクスチャの作成が始まります。";

    private static readonly string SelectionModeMessage = "選択モード:\n" +
        "- 選択モードでは、出力時に選択された部分のみ色が変更され、他の部分は変更されません。\n\n" +
        "1. 背景色を右クリックで設定します。\n" +
        "2. 変更したい部分を左クリックで選択してください（右側のプレビュー画像で、斜め線が入っていない部分が選択範囲となります。複数の選択も可能です）。\n" +
        "3. 選択が完了したら、「選択モード」のチェックを外してください。\n\n" +
        "「戻る」ボタンを押すと、最後に選択したエリアから順に選択を解除できます。\n" +
        "画面上部の「選択エリアリスト」ボタンから、選択されたエリアの有効／無効を切り替えられます。";

    private static readonly string InvertSelectionMessage = "選択反転モード:\n" +
        "- 選択された部分の色は変わらず、それ以外の部分のみ色が変わります。\n" +
        "- 透過画像作成モードでは、透過される部分が選択範囲の逆になります。\n\n" +
        "耳毛など、変えたくない部分が少ない場合に便利です。\n" +
        "選択モードだけでは、変えたくない部分以外をすべて選択する必要があるため、こちらのほうが効率的です。";

    private static readonly string TransparentModeMessage = "透過画像作成モード:\n" +
        "- 透過画像作成モードでは、選択した部分だけが残る透過画像を生成します。\n\n" +
        "1. 選択モードで、透過させたくない部分を選択します（複数選択可）。\n" +
        "2. 「透過モード」にチェックを入れてください。\n" +
        "3. 「作成」ボタンを押すと、選択された部分だけが残る透過画像が生成されます。";

    private static readonly string BalanceModeMessage = "バランスモード:\n" +
        "- 選択した色に近いほど強く、遠いほど弱く色を変更します。\n\n" +
        "オン／オフを切り替えて、どちらがより自然か確認してから色変換を実行するのがおすすめです。\n" +
        "画面上部の「バランスモードの設定」から、さまざまなパラメータを調整できます。\n" +
        "※「重り」の値は、変えたくない部分が変わってしまう場合は大きく、もっと変えたい場合は小さくしてください。0にすると通常モードと同じになります。";

    private static readonly string AdvancedColorSettingsMessage = "色の追加設定:\n" +
        "- ピクセル処理が終わったあとに反映される色の追加調整です。\n\n" +
        "変更できる値は、「明度」「コントラスト」「ガンマ補正」「露出」の4つです。\n" +
        "値を変更したらEnterやTabを押して、設定を更新してください。\n" +
        "値が変わっていた場合、各値の順に色の調整が適用されます。\n" +
        "※小数点以下の値（例：1.2など）の調整が想定されています。3などの大きな値の変化は想定されていません。";

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
