namespace ColorChanger.Models;

public class SerializableColor
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public SerializableColor() { }

    public SerializableColor(Color color)
    {
        R = color.R;
        G = color.G;
        B = color.B;
    }

    internal Color ToColor() => Color.FromArgb(R, G, B);
}