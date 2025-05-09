namespace ColorChanger.Models;

internal class ColorDifference(int diffR, int diffG, int diffB)
{
    internal int DiffR { get; set; } = diffR;
    internal int DiffG { get; set; } = diffG;
    internal int DiffB { get; set; } = diffB;
}
