namespace PixiDocks.Core.Docking;

public class TabCustomizationSettings
{
    public object? Icon { get; set; }
    public bool ShowCloseButton { get; set; }

    public TabCustomizationSettings(object? icon = null, bool showCloseButton = false)
    {
        Icon = icon;
        ShowCloseButton = showCloseButton;
    }
}