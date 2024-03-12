using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia;

public class PixiDockSimpleTheme : Styles
{
    public PixiDockSimpleTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);

        LayoutTree.TypeMap.Add(typeof(IDockable), typeof(Dockable));
        LayoutTree.TypeMap.Add(typeof(IDockableHost), typeof(DockableArea));
        LayoutTree.TypeMap.Add(typeof(IDockableTree), typeof(DockableTree));

        LayoutTree.TypeResolver.Add("DockableArea", typeof(IDockableHost));
        LayoutTree.TypeResolver.Add("DockableTree", typeof(IDockableTree));
    }
}