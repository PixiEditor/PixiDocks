using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

public class Dockable : ContentControl, IDockable
{
    public IDockableHost? Host { get; set; }

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<Dockable, string>(
        nameof(Id));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Dockable, string>(
        nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }
}