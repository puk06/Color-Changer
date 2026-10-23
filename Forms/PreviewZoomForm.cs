using ColorChanger.Services;
using System.Drawing.Drawing2D;

namespace ColorChanger.Forms;

internal partial class PreviewZoomForm : HideOnCloseForm
{
    /// <summary>
    /// 画像上でマウスが押された（座標は画像のピクセル座標）
    /// </summary>
    internal event Action<Point, MouseEventArgs>? PreviewMouseDown;

    /// <summary>
    /// 画像上でマウスが動いた（座標は画像のピクセル座標）
    /// </summary>
    internal event Action<Point, MouseEventArgs>? PreviewMouseMoved;

    /// <summary>
    /// 画像上でマウスが離された（座標は画像のピクセル座標）
    /// </summary>
    internal event Action<Point, MouseEventArgs>? PreviewMouseUp;

    /// <summary>
    /// 表示モードが変わり、画像の再生成が必要になったときに発生するイベント
    /// </summary>
    internal event Action<int>? RequestImageUpdate;

    private static readonly SolidBrush _backgroundBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
    private static readonly Pen _redPen = new Pen(Color.Red, 1f);
    private static readonly Pen _borderPen = new Pen(Color.Gray, 1);

    private Image? _image;
    private bool _ownsImage = false;
    private float _zoom = 1.0f;
    private Point _panOffset = Point.Empty;
    private Point _mouseDownPosition;
    private Point _panStart;
    private bool _isPanning = false;
    private bool[,]? _selectionPenMap = null;

    internal PreviewZoomForm()
    {
        InitializeComponent();
        Icon = FormUtils.GetSoftwareIcon();
        SetStyle(ControlStyles.ResizeRedraw, true);

        previewImageMode.SelectedIndex = 0;
    }

    /// <summary>
    /// 画像をセットします。
    /// </summary>
    /// <param name="image">表示する画像</param>
    /// <param name="ownsImage">true の場合、差し替え時にこのフォームが画像を破棄する</param>
    internal void SetImage(Image image, bool ownsImage)
    {
        ReleaseImage();

        _image = image;
        _ownsImage = ownsImage;

        if (_panOffset == Point.Empty) CenterImage();
        Invalidate();
    }

    /// <summary>
    /// 表示中の画像を取り除きます。
    /// </summary>
    internal void ResetImage()
    {
        ReleaseImage();
        Invalidate();
    }

    internal void SetGraphics(bool[,]? selectionPenMap)
    {
        _selectionPenMap = selectionPenMap;
        Invalidate();
    }

    private void ReleaseImage()
    {
        if (_ownsImage) _image?.Dispose();

        _image = null;
        _ownsImage = false;
    }

    #region 処理関数
    private Point GetClickedPosition(Point clickedPoint)
    {
        if (_image == null) return Point.Empty;

        var imgX = (clickedPoint.X - _panOffset.X) / _zoom;
        var imgY = (clickedPoint.Y - _panOffset.Y) / _zoom;

        if (imgX >= 0 && imgY >= 0 && imgX < _image.Width && imgY < _image.Height)
        {
            return new Point((int)imgX, (int)imgY);
        }

        return Point.Empty;
    }

    private void DrawThumbnailPreview(Graphics g)
    {
        if (_image == null) return;

        const int margin = 10;
        const int thumbnailWidth = 160;
        const int thumbnailHeight = 120;

        var previewBox = new Rectangle(
            ClientSize.Width - thumbnailWidth - margin,
            ClientSize.Height - thumbnailHeight - margin,
            thumbnailWidth,
            thumbnailHeight
        );

        g.FillRectangle(_backgroundBrush, previewBox);

        var scaleX = (float)thumbnailWidth / _image.Width;
        var scaleY = (float)thumbnailHeight / _image.Height;
        var scale = Math.Min(scaleX, scaleY);

        var imgDrawWidth = (int)(_image.Width * scale);
        var imgDrawHeight = (int)(_image.Height * scale);

        var imgX = previewBox.X + ((thumbnailWidth - imgDrawWidth) / 2);
        var imgY = previewBox.Y + ((thumbnailHeight - imgDrawHeight) / 2);

        g.DrawImage(_image, imgX, imgY, imgDrawWidth, imgDrawHeight);

        var visibleX = -_panOffset.X / _zoom;
        var visibleY = (-_panOffset.Y + panel.Height) / _zoom;
        var visibleW = ClientSize.Width / _zoom;
        var visibleH = (ClientSize.Height - panel.Height) / _zoom;

        var clippedX = Math.Max(0, visibleX);
        var clippedY = Math.Max(0, visibleY);
        var clippedW = Math.Min(_image.Width - clippedX, visibleW - (clippedX - visibleX));
        var clippedH = Math.Min(_image.Height - clippedY, visibleH - (clippedY - visibleY));

        var redRect = new RectangleF(
            imgX + (clippedX * scale),
            imgY + (clippedY * scale),
            clippedW * scale,
            clippedH * scale
        );

        g.DrawRectangle(_redPen, redRect.X, redRect.Y, redRect.Width, redRect.Height);
        g.DrawRectangle(_borderPen, previewBox);
    }

    private void CenterImage()
    {
        if (_image == null) return;

        _zoom = 1.0f;

        var x = (Size.Width - (int)(_image.Width * _zoom)) / 2;
        var y = ((Size.Height - panel.Height - (int)(_image.Height * _zoom)) / 2) + panel.Height;

        _panOffset = new Point(x, y);
    }
    #endregion

    #region イベントハンドラ
    private void ImageZoomForm_MouseWheel(object? sender, MouseEventArgs e)
    {
        var oldZoom = _zoom;

        if (e.Delta > 0) _zoom *= 1.1f;
        else _zoom /= 1.1f;

        var mousePos = e.Location;
        var deltaZoom = _zoom / oldZoom;

        _panOffset.X = (int)(mousePos.X - (deltaZoom * (mousePos.X - _panOffset.X)));
        _panOffset.Y = (int)(mousePos.Y - (deltaZoom * (mousePos.Y - _panOffset.Y)));

        Invalidate();
    }

    private void PreviewZoomForm_Paint(object sender, PaintEventArgs e)
    {
        if (_image == null) return;

        e.Graphics.Clear(BackColor);
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        e.Graphics.TranslateTransform(_panOffset.X, _panOffset.Y);
        e.Graphics.ScaleTransform(_zoom, _zoom);
        e.Graphics.DrawImage(_image, 0, 0);

        if (_selectionPenMap != null) FormUtils.DrawSelectionMap(e.Graphics, _selectionPenMap);

        e.Graphics.ResetTransform();

        DrawThumbnailPreview(e.Graphics);
    }

    private void PreviewZoomForm_MouseMove(object sender, MouseEventArgs e)
    {
        if (ModifierKeys.HasFlag(Keys.Control))
        {
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && _isPanning)
            {
                _panOffset = new Point(
                    _panStart.X + (e.X - _mouseDownPosition.X),
                    _panStart.Y + (e.Y - _mouseDownPosition.Y)
                );

                Invalidate();
            }
        }
        else
        {
            PreviewMouseMoved?.Invoke(GetClickedPosition(e.Location), e);
        }
    }

    private void PreviewZoomForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (ModifierKeys.HasFlag(Keys.Control))
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                _isPanning = true;
                _mouseDownPosition = e.Location;
                _panStart = _panOffset;
            }
        }
        else
        {
            PreviewMouseDown?.Invoke(GetClickedPosition(e.Location), e);
        }
    }

    private void PreviewZoomForm_MouseUp(object sender, MouseEventArgs e)
    {
        if (ModifierKeys.HasFlag(Keys.Control) || _isPanning)
        {
            _isPanning = false;
        }
        else
        {
            PreviewMouseUp?.Invoke(GetClickedPosition(e.Location), e);
        }
    }

    private void PreviewImageMode_SelectedIndexChanged(object sender, EventArgs e)
        => RequestUpdate();

    private void PreviewZoomForm_Resize(object sender, EventArgs e)
        => Invalidate();

    private void RefleshButton_Click(object sender, EventArgs e)
        => RequestUpdate();

    private void ViewReset_Click(object sender, EventArgs e)
    {
        CenterImage();
        Invalidate();
    }

    private void HowToUse_Click(object sender, EventArgs e)
        => FormUtils.ShowInfo(
            "操作方法\n\n" +
            "クリック：\n" +
            "　→ メイン画面の操作が可能です。\n色選択、オブジェクト選択、ペンなどに使うことができます。\n\n" +
            "Ctrl + クリック：\n" +
            "　→ 表示中の画像をドラッグで移動\n\n" +
            "マウスホイール：\n" +
            "　→ 画像の拡大 / 縮小"
        );

    private void RequestUpdate()
        => RequestImageUpdate?.Invoke(previewImageMode.SelectedIndex);
    #endregion
}
