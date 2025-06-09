namespace ColorChanger.Models;

internal class ColorDifference(Color previousColor, Color newColor)
{
    internal Color PreviousColor { get; private set; } = previousColor;
    internal Color NewColor { get; private set; } = newColor;
    internal int DiffR { get; private set; } = newColor.R - previousColor.R;
    internal int DiffG { get; private set; } = newColor.G - previousColor.G;
    internal int DiffB { get; private set; } = newColor.B - previousColor.B;

    /// <summary>
    /// ColorDifferenceを与えられた値を基に更新する
    /// </summary>
    /// <param name="previousColor"></param>
    /// <param name="newColor"></param>
    internal void Set(Color previousColor, Color newColor)
    {
        PreviousColor = previousColor;
        NewColor = newColor;
        DiffR = newColor.R - previousColor.R;
        DiffG = newColor.G - previousColor.G;
        DiffB = newColor.B - previousColor.B;
    }

    public override string ToString()
    {
        string diffRStr = FormatDiff(DiffR);
        string diffGStr = FormatDiff(DiffG);
        string diffBStr = FormatDiff(DiffB);
        
        return $"{diffRStr}, {diffGStr}, {diffBStr}";
    }

    private static string FormatDiff(int diff)
        => diff > 0 ? $"+{diff}" : diff.ToString();
}
