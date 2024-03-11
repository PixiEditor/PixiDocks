using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using PixiDocks.Core;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

public class Dockable : ContentControl, IDockable
{
    public IDockableHost? Host { get; set; }

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<Dockable, string>(
        nameof(Id));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Dockable, string>(
        nameof(Title));

    public static readonly StyledProperty<Control> DockableContentProperty = AvaloniaProperty.Register<Dockable, Control>(
        nameof(DockableContent));

    public static readonly StyledProperty<bool> CanFloatProperty = AvaloniaProperty.Register<Dockable, bool>(
        nameof(CanFloat), true);

    public static readonly StyledProperty<bool> CanCloseProperty = AvaloniaProperty.Register<Dockable, bool>(
        nameof(CanClose), true);

    public static readonly StyledProperty<IImage?> IconProperty = AvaloniaProperty.Register<Dockable, IImage?>(
        nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    object? IDockable.Icon => Icon;

    public bool CanClose
    {
        get => GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public bool CanFloat
    {
        get => GetValue(CanFloatProperty);
        set => SetValue(CanFloatProperty, value);
    }

    public Control DockableContent
    {
        get => GetValue(DockableContentProperty);
        set => SetValue(DockableContentProperty, value);
    }

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

    public IEnumerator<IDockableLayoutElement> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}