namespace ColorChanger.Utils;

internal static class SelectionUtils
{
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
                    graphics.FillRectangle(Brushes.Red, x, y, 1, 1);
                }
            }
        }
    }
}
