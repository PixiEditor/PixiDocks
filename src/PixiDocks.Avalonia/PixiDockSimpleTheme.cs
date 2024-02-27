using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace PixiDocks.Avalonia;

public class PixiDockSimpleTheme : Styles
{
    public PixiDockSimpleTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}