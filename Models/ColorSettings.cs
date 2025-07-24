namespace ColorChanger.Models;

public class ColorSettings
{
    public SerializableColor PreviousColor { get; set; } = new SerializableColor();
    public SerializableColor NewColor { get; set; } = new SerializableColor();

    public bool BalanceModeEnabled { get; set; } = false;
    public bool TransparentModeEnabled { get; set; } = false;

    public BalanceModeConfiguration BalanceModeConfiguration { get; set; } = new BalanceModeConfiguration();
    public AdvancedColorConfiguration AdvancedColorConfiguration { get; set;} = new AdvancedColorConfiguration();
}
