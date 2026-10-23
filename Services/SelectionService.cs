using System.Collections;
using System.Drawing.Imaging;
using ColorChanger.Models;

namespace ColorChanger.Services;

/// <summary>
/// 画像上の選択エリアの作成と座標変換を行う
/// </summary>
internal static class SelectionService
{
    private static readonly (int Dx, int Dy)[] Directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>
    /// 指定座標から背景色以外の部分を塗りつぶし（領域拡張）で選択する
    /// </summary>
    /// <returns>選択できなかった場合は null</returns>
    internal static BitArray? SelectByFloodFill(Bitmap image, Point start, Color background)
    {
        int width = image.Width;
        int height = image.Height;

        if (start.X < 0 || start.Y < 0 || start.X >= width || start.Y >= height) return null;

        using PixelLock pixelLock = new PixelLock(image, ImageLockMode.ReadOnly);
        Span<ColorPixel> pixels = pixelLock.Pixels;

        int startIndex = (start.Y * width) + start.X;
        if (pixels[startIndex].Equals(background)) return null;

        BitArray selected = new BitArray(width * height, false);
        Queue<(int X, int Y)> queue = new Queue<(int X, int Y)>();

        selected[startIndex] = true;
        queue.Enqueue((start.X, start.Y));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            foreach (var (dx, dy) in Directions)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                int nIndex = (ny * width) + nx;
                if (selected[nIndex]) continue;

                if (!pixels[nIndex].Equals(background))
                {
                    selected[nIndex] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return selected;
    }

    /// <summary>
    /// マスク画像から、選択タイプに一致するピクセルを選択する
    /// </summary>
    internal static BitArray SelectByMask(Bitmap image, ImageMaskSelectionType selectionType, Color selectionColor)
    {
        int totalPixels = image.Width * image.Height;
        BitArray selected = new BitArray(totalPixels, false);

        using PixelLock pixelLock = new PixelLock(image, ImageLockMode.ReadOnly);
        Span<ColorPixel> pixels = pixelLock.Pixels;

        for (int i = 0; i < totalPixels; i++)
        {
            selected[i] = selectionType switch
            {
                ImageMaskSelectionType.Black => pixels[i].Equals(Color.Black),
                ImageMaskSelectionType.White => pixels[i].Equals(Color.White),
                ImageMaskSelectionType.Opaque => pixels[i].A == 255,
                ImageMaskSelectionType.Opaque1 => !pixels[i].IsTransparent,
                ImageMaskSelectionType.Transparent => pixels[i].IsTransparent,
                ImageMaskSelectionType.CustomColor => pixels[i].Equals(selectionColor),
                _ => false,
            };
        }

        return selected;
    }

    /// <summary>
    /// 元画像座標の選択エリアを、プレビュー座標の選択エリアに変換する
    /// </summary>
    internal static BitArray ToPreviewSelection(BitArray selected, Size sourceSize, Size previewSize, bool inverse)
    {
        int previewWidth = previewSize.Width;
        int previewHeight = previewSize.Height;

        float ratioX = (float)sourceSize.Width / previewWidth;
        float ratioY = (float)sourceSize.Height / previewHeight;

        BitArray result = new BitArray(previewWidth * previewHeight, inverse);

        for (int y = 0; y < previewHeight; y++)
        {
            int sourceY = (int)(y * ratioY);
            if (sourceY >= sourceSize.Height) continue;

            for (int x = 0; x < previewWidth; x++)
            {
                int sourceX = (int)(x * ratioX);
                if (sourceX >= sourceSize.Width) continue;

                int sourceIndex = (sourceY * sourceSize.Width) + sourceX;
                if (!selected[sourceIndex]) continue;

                int previewIndex = (y * previewWidth) + x;
                result[previewIndex] = !result[previewIndex];
            }
        }

        return result;
    }

    /// <summary>
    /// プレビュー上に表示する、選択エリアの境界（斜線）のマスクを作成する
    /// </summary>
    internal static BitArray CreateOverlayMask(BitArray selected, int width, int height)
    {
        const int stripeInterval = 7;
        const int lineWidth = 2;

        BitArray result = new BitArray(selected.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                if (!selected[index])
                {
                    if ((x + y) % stripeInterval == 0)
                    {
                        result[index] = true;
                    }
                    continue;
                }

                if (IsEdgePixel(x, y, width, height, lineWidth) || !IsInnerPixel(selected, x, y, width, lineWidth))
                {
                    result[index] = true;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 表示座標を元画像の座標に変換する
    /// </summary>
    internal static Point ToSourceCoordinates(Point displayPoint, Size displaySize, Size sourceSize)
    {
        float ratioX = (float)sourceSize.Width / displaySize.Width;
        float ratioY = (float)sourceSize.Height / displaySize.Height;

        return new Point((int)(displayPoint.X * ratioX), (int)(displayPoint.Y * ratioY));
    }

    /// <summary>
    /// 選択エリアを、指定サイズに縮小したマップ（選択されている部分が true）に変換する
    /// </summary>
    internal static bool[,] BuildPreviewMap(BitArray selected, int width, int height, Size previewSize)
    {
        int targetWidth = previewSize.Width;
        int targetHeight = previewSize.Height;

        bool[,] previewMap = new bool[targetWidth, targetHeight];

        float scaleX = (float)width / targetWidth;
        float scaleY = (float)height / targetHeight;

        Parallel.For(0, targetHeight, ty =>
        {
            int startY = (int)(ty * scaleY);
            int endY = Math.Min((int)((ty + 1) * scaleY), height);

            for (int tx = 0; tx < targetWidth; tx++)
            {
                int startX = (int)(tx * scaleX);
                int endX = Math.Min((int)((tx + 1) * scaleX), width);

                bool isSelected = false;

                for (int y = startY; y < endY && !isSelected; y++)
                {
                    int rowStart = y * width;
                    for (int x = startX; x < endX; x++)
                    {
                        if (selected[rowStart + x])
                        {
                            isSelected = true;
                            break;
                        }
                    }
                }

                previewMap[tx, ty] = isSelected;
            }
        });

        return previewMap;
    }

    private static bool IsEdgePixel(int x, int y, int width, int height, int lineWidth)
        => x < lineWidth || x >= width - lineWidth || y < lineWidth || y >= height - lineWidth;

    private static bool IsInnerPixel(BitArray selected, int x, int y, int width, int lineWidth)
    {
        return selected[((y - lineWidth) * width) + x] &&
            selected[((y + lineWidth) * width) + x] &&
            selected[(y * width) + (x - lineWidth)] &&
            selected[(y * width) + (x + lineWidth)];
    }
}
