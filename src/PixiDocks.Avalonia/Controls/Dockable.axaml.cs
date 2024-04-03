using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":focused")]
public class Dockable : ContentControl, IDockable, IDockableSelectionEvents, IDockableCloseEvents
{

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

    public static readonly RoutedEvent<RoutedEventArgs> SelectedEvent = RoutedEvent.Register<Dockable, RoutedEventArgs>(
        nameof(Selected), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> DeselectedEvent = RoutedEvent.Register<Dockable, RoutedEventArgs>(
        nameof(Deselected), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CloseEvent = RoutedEvent.Register<Dockable, RoutedEventArgs>(
        nameof(Close), RoutingStrategies.Bubble);

    public event Action<bool>? FocusChanged;

    public event EventHandler<RoutedEventArgs>? Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Deselected
    {
        add => AddHandler(DeselectedEvent, value);
        remove => RemoveHandler(DeselectedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Close
    {
        add => AddHandler(CloseEvent, value);
        remove => RemoveHandler(CloseEvent, value);
    }

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    object? IDockable.Icon
    {
        get => Icon;
        set => Icon = (IImage?)value;
    }

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

    private IDockableHost? host;

    public IDockableHost? Host
    {
        get => host;
        set
        {
            if (host != value)
            {
                if (host != null)
                {
                    host.FocusedChanged -= OnFocusedChanged;
                }

                host = value;
                if (host != null)
                {
                    OnFocusedChanged(host.Context?.FocusedHost == host);
                    host.FocusedChanged += OnFocusedChanged;
                }
            }
        }
    }

    private void OnFocusedChanged(bool focused)
    {
        PseudoClasses.Set(":focused", focused);
    }

    void IDockableSelectionEvents.OnSelected()
    {
        if(Content is IDockableSelectionEvents selectionEvents)
        {
            selectionEvents.OnSelected();
        }

        RaiseEvent(new RoutedEventArgs(SelectedEvent));
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        if(Content is IDockableSelectionEvents selectionEvents)
        {
            selectionEvents.OnSelected();
        }

        RaiseEvent(new RoutedEventArgs(DeselectedEvent));
    }

    bool IDockableCloseEvents.OnClose()
    {
        bool close = true;
        if(Content is IDockableCloseEvents closeEvents)
        {
            close = closeEvents.OnClose();
        }

        RaiseEvent(new RoutedEventArgs(CloseEvent));
        return close;
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