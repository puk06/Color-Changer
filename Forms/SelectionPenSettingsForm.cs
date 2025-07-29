using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Forms;

internal partial class SelectionPenSettingsForm : Form
{
    private const string UNDO_BUTTON_TEXT = "元に戻す";

    internal bool PenEnaled => Visible && enablePen.Checked;

    /// <summary>
    /// 選択状況が確定した際に発生するイベント
    /// </summary>
    internal event EventHandler? SelectionConfirmed;

    /// <summary>
    /// 選択状況が１個前に戻った時に発生するイベント
    /// </summary>
    internal event EventHandler? SelectionReverted;

    private readonly List<BitArray> _selectedHistory = new List<BitArray>();

    private BitArray _totalSelectedArea = BitArrayUtils.GetEmpty();
    internal BitArray GetTotalSelectedArea => _totalSelectedArea;

    private bool _initialized = false;
    internal bool Initialized => _initialized;

    private int PenWidth => penWidth.Value;

    private int _width;
    private int _height;

    internal SelectionPenSettingsForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();

        RefleshPenWidthLabel();
        RefleshUndoButton();
    }

    /// <summary>
    /// 選択エリアを初期化します。
    /// </summary>
    /// <param name="bitmapSize"></param>
    internal void Initialize(Size bitmapSize)
    {
        _width = bitmapSize.Width;
        _height = bitmapSize.Height;

        _selectedHistory.Clear();
        _totalSelectedArea = new BitArray(_width * _height);
        RefleshUndoButton();

        _initialized = true;
    }

    private bool _endSelection = false;

    internal void EndSelection()
        => _endSelection = true;

    /// <summary>
    /// 渡されたPointから、選択エリアを作成します。
    /// </summary>
    /// <param name="center"></param>
    internal void SetSelectionArea(Point center)
    {
        if (!Initialized || PenWidth <= 0) return;

        if (_endSelection || _selectedHistory.Count == 0)
        {
            _selectedHistory.Add(new BitArray(_totalSelectedArea));
            RefleshUndoButton();

            _endSelection = false;
        }

        bool value = !eraserMode.Checked;

        int radius = PenWidth / 2;
        int radiusSquared = radius * radius;

        int minX = Math.Max(center.X - radius, 0);
        int maxX = Math.Min(center.X + radius, _width - 1);
        int minY = Math.Max(center.Y - radius, 0);
        int maxY = Math.Min(center.Y + radius, _height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - center.Y;
            int dy2 = dy * dy;
            int rowStart = y * _width;

            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - center.X;
                if ((dx * dx) + dy2 <= radiusSquared)
                {
                    _totalSelectedArea[rowStart + x] = value;
                }
            }
        }
    }

    /// <summary>
    /// 渡されたSizeに縮小した現在の選択エリアを計算、取得します。
    /// </summary>
    /// <param name="previewSize"></param>
    /// <returns></returns>
    internal bool[,] GeneratePreviewSelectionMap(Size previewSize)
    {
        int targetWidth = previewSize.Width;
        int targetHeight = previewSize.Height;

        bool[,] previewMap = new bool[targetWidth, targetHeight];

        float scaleX = (float)_width / targetWidth;
        float scaleY = (float)_height / targetHeight;

        ParallelUtils.ForEach2D(targetHeight, targetWidth, (tx, ty) =>
        {
            int startX = (int)(tx * scaleX);
            int endX = Math.Min((int)((tx + 1) * scaleX), _width);

            int startY = (int)(ty * scaleY);
            int endY = Math.Min((int)((ty + 1) * scaleY), _height);

            bool selected = false;

            for (int y = startY; y < endY && !selected; y++)
            {
                int rowStart = y * _width;
                for (int x = startX; x < endX; x++)
                {
                    int index = rowStart + x;
                    if (_totalSelectedArea[index])
                    {
                        selected = true;
                        break;
                    }
                }
            }

            previewMap[tx, ty] = selected;
        });

        return previewMap;
    }

    /// <summary>
    /// 選択エリアをリセットします。リセット後はInitializeの実行が必要になります。
    /// </summary>
    internal void Reset()
    {
        _width = 0;
        _height = 0;

        _selectedHistory.Clear();
        _totalSelectedArea = BitArrayUtils.GetEmpty();
        RefleshUndoButton();

        _initialized = false;
    }

    private void UndoSelection()
    {
        int lastIndex = _selectedHistory.Count - 1;
        if (lastIndex < 0) return;

        BitArray lastSelectedPoints = _selectedHistory[lastIndex];
        for (int i = 0; i < lastSelectedPoints.Count; i++)
        {
            _totalSelectedArea[i] = lastSelectedPoints[i];
        }

        _selectedHistory.RemoveAt(lastIndex);
        RefleshUndoButton();

        SelectionReverted?.Invoke(null, EventArgs.Empty);
    }

    private void RefleshUndoButton()
    {
        undo.Text = $"{UNDO_BUTTON_TEXT} - {_selectedHistory.Count}個の履歴";
        undo.Enabled = _selectedHistory.Count != 0;
    }

    #region イベントハンドラー
    private void PenWidth_ValueChanged(object sender, EventArgs e)
        => RefleshPenWidthLabel();

    private void RefleshPenWidthLabel()
        => penWidthLabel.Text = $"- {penWidth.Value}px";

    private void Undo_Click(object sender, EventArgs e)
        => UndoSelection();

    private void SelectionPenSettingsForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            UndoSelection();
        }
    }

    private void AddLayer_Click(object sender, EventArgs e)
        => SelectionConfirmed?.Invoke(false, EventArgs.Empty);

    private void AddEraserLayer_Click(object sender, EventArgs e)
        => SelectionConfirmed?.Invoke(true, EventArgs.Empty);

    private void CancelSelection_Click(object sender, EventArgs e)
    {
        bool result = FormUtils.ShowConfirm("現在のペンの選択状況を全て無視して、取り消しますか？");
        if (!result) return;

        SelectionConfirmed?.Invoke(null, EventArgs.Empty);
    }
    #endregion

    #region フォーム関連
    private void SelectionPenSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Visible = false;
        e.Cancel = true;

        SelectionConfirmed?.Invoke(null, EventArgs.Empty);
    }
    #endregion
}
