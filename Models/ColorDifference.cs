namespace ColorChanger.Models;

internal class ColorDifference(Color previousColor, Color newColor)
{
    internal Color PreviousColor { get; set; } = previousColor;
    internal int DiffR { get; set; } = newColor.R - previousColor.R;
    internal int DiffG { get; set; } = newColor.G - previousColor.G;
    internal int DiffB { get; set; } = newColor.B - previousColor.B;

    public override string ToString()
    {
        var diffRStr = DiffR > 0 ? "+" + DiffR : DiffR.ToString();
        var diffGStr = DiffG > 0 ? "+" + DiffG : DiffG.ToString();
        var diffBStr = DiffB > 0 ? "+" + DiffB : DiffB.ToString();

        return $"{diffRStr}, {diffGStr}, {diffBStr}";
    }
}
