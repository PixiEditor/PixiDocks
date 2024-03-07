using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using PixiDocks.Avalonia.Controls;

namespace PixiDocks.Avalonia;

public class PixiDockSimpleTheme : Styles
{
    public static ControlTheme DockableTemplate { get; private set; }
    public PixiDockSimpleTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
        DockableTemplate = this.Resources[typeof(Dockable)] as ControlTheme;
    }
}