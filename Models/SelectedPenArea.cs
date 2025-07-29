using System.Collections;

namespace ColorChanger.Models;

internal class SelectedPenArea(BitArray selectedPoints, bool isEraser)
{
    internal BitArray SelectedPoints { get; private set; } = selectedPoints;
    internal bool IsEraser { get; private set; } = isEraser;
}

