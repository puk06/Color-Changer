using ColorChanger.Utils;
using System.Collections;

namespace ColorChanger.Forms;

internal partial class SelectionPenSettingsForm : Form
{
    internal bool PenEnaled => enablePen.Checked;
    internal bool IsEraser => eraserMode.Checked;

    internal event EventHandler? SelectionConfirmed;

    private BitArray _currentSelectedArea = BitArrayUtils.GetEmpty();
    internal bool Initialized => _currentSelectedArea.Count != 0;
    internal BitArray GetCurrentSelectedArea => _currentSelectedArea;

    private int PenWidth => penWidth.Value;

    private int _width;
    private int _height;

    internal SelectionPenSettingsForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();
        RefleshPenWidthLabel();
    }

    /// <summary>
    /// 選択エリアを初期化します。
    /// </summary>
    /// <param name="bitmapSize"></param>
    internal void Initialize(Size bitmapSize)
    {
        _width = bitmapSize.Width;
        _height = bitmapSize.Height;
        _currentSelectedArea = new BitArray(_width * _height);
    }

    /// <summary>
    /// 渡されたPointから、選択エリアを作成します。
    /// </summary>
    /// <param name="center"></param>
    internal void SetSelectionArea(Point center)
    {
        if (!Initialized || PenWidth <= 0) return;

        bool value = !eraserMode.Checked;

        int radius = PenWidth / 2;
        int startX = Math.Max(center.X - radius, 0);
        int endX = Math.Min(center.X + radius, _width - 1);
        int startY = Math.Max(center.Y - radius, 0);
        int endY = Math.Min(center.Y + radius, _height - 1);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                int dx = x - center.X;
                int dy = y - center.Y;

                if ((dx * dx) + (dy * dy) <= radius * radius)
                {
                    int index = PixelUtils.GetPixelIndex(x, y, _width);
                    _currentSelectedArea[index] = value;
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

        for (int ty = 0; ty < targetHeight; ty++)
        {
            for (int tx = 0; tx < targetWidth; tx++)
            {
                int startX = (int)(tx * scaleX);
                int startY = (int)(ty * scaleY);

                int endX = Math.Min((int)((tx + 1) * scaleX), _width);
                int endY = Math.Min((int)((ty + 1) * scaleY), _height);

                bool selected = false;

                for (int y = startY; y < endY && !selected; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        int index = PixelUtils.GetPixelIndex(x, y, _width);
                        if (_currentSelectedArea[index])
                        {
                            selected = true;
                            break;
                        }
                    }
                }

                previewMap[tx, ty] = selected;
            }
        }

        return previewMap;
    }

    /// <summary>
    /// 選択エリアをリセットします。リセット後はInitializeの実行が必要になります。
    /// </summary>
    internal void Reset()
    {
        _width = 0;
        _height = 0;
        _currentSelectedArea = BitArrayUtils.GetEmpty();
    }

    #region イベントハンドラー
    private void PenWidth_ValueChanged(object sender, EventArgs e)
        => RefleshPenWidthLabel();

    private void RefleshPenWidthLabel()
        => penWidthLabel.Text = $"- {penWidth.Value}px";

    private void AddLayer_Click(object sender, EventArgs e)
        => SelectionConfirmed?.Invoke(false, EventArgs.Empty);

    private void AddEraserLayer_Click(object sender, EventArgs e)
        => SelectionConfirmed?.Invoke(true, EventArgs.Empty);

    private void CancelSelection_Click(object sender, EventArgs e)
        => SelectionConfirmed?.Invoke(null, EventArgs.Empty);
    #endregion
}
