namespace ColorChanger.Utils;

internal static class SelectionUtils
{
    private static readonly SolidBrush _selectionOverlayBrush = new SolidBrush(Color.FromArgb(150, 255, 0, 0));

    internal static void SetSelectionPreviewMap(Graphics graphics, bool[,] previewMap)
    {
        int width = previewMap.GetLength(0);
        int height = previewMap.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (previewMap[x, y])
                {
                    graphics.FillRectangle(_selectionOverlayBrush, x, y, 1, 1);
                }
            }
        }
    }
}
