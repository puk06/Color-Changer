namespace ColorChanger.Models;

internal class ColorDifference(Color previousColor, Color newColor)
{
    internal Color PreviousColor { get; private set; } = previousColor;
    internal int DiffR { get; private set; } = newColor.R - previousColor.R;
    internal int DiffG { get; private set; } = newColor.G - previousColor.G;
    internal int DiffB { get; private set; } = newColor.B - previousColor.B;

    public override string ToString()
    {
        string diffRStr = DiffR > 0 ? $"+{DiffR}" : DiffR.ToString();
        string diffGStr = DiffG > 0 ? $"+{DiffG}" : DiffG.ToString();
        string diffBStr = DiffB > 0 ? $"+{DiffB}" : DiffB.ToString();
        return $"{diffRStr}, {diffGStr}, {diffBStr}";
    }

    internal void Set(Color previousColor, Color newColor)
    {
        PreviousColor = previousColor;
        DiffR = newColor.R - previousColor.R;
        DiffG = newColor.G - previousColor.G;
        DiffB = newColor.B - previousColor.B;
    }
}
