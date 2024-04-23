using Avalonia;
using Avalonia.Controls.Presenters;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class DockableTabPresenter : ContentPresenter
{
    public static readonly StyledProperty<IDockable> DockableProperty = AvaloniaProperty.Register<DockableTabPresenter, IDockable>(
        nameof(Dockable));

    public IDockable Dockable
    {
        get => GetValue(DockableProperty);
        set => SetValue(DockableProperty, value);
    }

}