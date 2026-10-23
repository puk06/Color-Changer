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
        var width = image.Width;
        var height = image.Height;

        if (start.X < 0 || start.Y < 0 || start.X >= width || start.Y >= height) return null;

        using var pixelLock = new PixelLock(image, ImageLockMode.ReadOnly);
        var pixels = pixelLock.Pixels;

        var startIndex = (start.Y * width) + start.X;
        if (pixels[startIndex].Equals(background)) return null;

        var selected = new BitArray(width * height, false);
        var queue = new Queue<(int X, int Y)>();

        selected[startIndex] = true;
        queue.Enqueue((start.X, start.Y));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            foreach (var (dx, dy) in Directions)
            {
                var nx = x + dx;
                var ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                var nIndex = (ny * width) + nx;
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
        var totalPixels = image.Width * image.Height;
        var selected = new BitArray(totalPixels, false);

        using var pixelLock = new PixelLock(image, ImageLockMode.ReadOnly);
        var pixels = pixelLock.Pixels;

        for (var i = 0; i < totalPixels; i++)
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
        var previewWidth = previewSize.Width;
        var previewHeight = previewSize.Height;

        var ratioX = (float)sourceSize.Width / previewWidth;
        var ratioY = (float)sourceSize.Height / previewHeight;

        var result = new BitArray(previewWidth * previewHeight, inverse);

        for (var y = 0; y < previewHeight; y++)
        {
            var sourceY = (int)(y * ratioY);
            if (sourceY >= sourceSize.Height) continue;

            for (var x = 0; x < previewWidth; x++)
            {
                var sourceX = (int)(x * ratioX);
                if (sourceX >= sourceSize.Width) continue;

                var sourceIndex = (sourceY * sourceSize.Width) + sourceX;
                if (!selected[sourceIndex]) continue;

                var previewIndex = (y * previewWidth) + x;
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

        var result = new BitArray(selected.Length);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x;

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
        var ratioX = (float)sourceSize.Width / displaySize.Width;
        var ratioY = (float)sourceSize.Height / displaySize.Height;

        return new Point((int)(displayPoint.X * ratioX), (int)(displayPoint.Y * ratioY));
    }

    /// <summary>
    /// 選択エリアを、指定サイズに縮小したマップ（選択されている部分が true）に変換する
    /// </summary>
    internal static bool[,] BuildPreviewMap(BitArray selected, int width, int height, Size previewSize)
    {
        var targetWidth = previewSize.Width;
        var targetHeight = previewSize.Height;

        var previewMap = new bool[targetWidth, targetHeight];

        var scaleX = (float)width / targetWidth;
        var scaleY = (float)height / targetHeight;

        Parallel.For(0, targetHeight, ty =>
        {
            var startY = (int)(ty * scaleY);
            var endY = Math.Min((int)((ty + 1) * scaleY), height);

            for (var tx = 0; tx < targetWidth; tx++)
            {
                var startX = (int)(tx * scaleX);
                var endX = Math.Min((int)((tx + 1) * scaleX), width);

                var isSelected = false;

                for (var y = startY; y < endY && !isSelected; y++)
                {
                    var rowStart = y * width;
                    for (var x = startX; x < endX; x++)
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
