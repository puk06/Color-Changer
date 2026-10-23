using System.Drawing.Imaging;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 32bppArgb の Bitmap をロックし、ピクセル配列として扱う。Dispose でロックを解除する。
/// </summary>
internal sealed class PixelLock : IDisposable
{
    private readonly Bitmap _bitmap;
    private readonly BitmapData _data;
    private readonly int _pixelCount;

    internal PixelLock(Bitmap bitmap, ImageLockMode mode)
    {
        _bitmap = bitmap;
        _pixelCount = bitmap.Width * bitmap.Height;
        _data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), mode, PixelFormat.Format32bppArgb);
    }

    internal unsafe Span<ColorPixel> Pixels => new Span<ColorPixel>((void*)_data.Scan0, _pixelCount);

    public void Dispose()
        => _bitmap.UnlockBits(_data);
}
