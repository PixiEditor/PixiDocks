using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PixiDocks.Avalonia.Behaviors;
using PixiDocks.Avalonia.Helpers;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":dockableOver", ":center", ":left", ":right", ":top", ":bottom", ":focused")]
public class DockableArea : TemplatedControl, IDockableHost, ITreeElement
{
    IDockContext IDockableHost.Context
    {
        get => Context;
        set => Context = value;
    }

    IReadOnlyCollection<IDockable> IDockableHost.Dockables => Dockables;

    IDockable IDockableHost.ActiveDockable
    {
        get => ActiveDockable;
        set
        {
            ActiveDockable = value;
        }
    }

    public static readonly StyledProperty<ObservableCollection<IDockable>> DockablesProperty =
        AvaloniaProperty.Register<DockableArea, ObservableCollection<IDockable>>(
            nameof(Dockables));

    public static readonly StyledProperty<ICommand> FloatCommandProperty = AvaloniaProperty.Register<DockableArea, ICommand>(
        nameof(FloatCommand));

    public static readonly StyledProperty<IDockContext> ContextProperty = AvaloniaProperty.Register<DockableArea, IDockContext>(
        nameof(Context));

    public static readonly StyledProperty<IDockable?> ActiveDockableProperty = AvaloniaProperty.Register<DockableArea, IDockable?>(
        nameof(ActiveDockable));

    public static readonly StyledProperty<DockableAreaRegion> RegionProperty = AvaloniaProperty.Register<DockableArea, DockableAreaRegion>(
        nameof(Region));

    public static readonly StyledProperty<Dock> TabPlacementProperty = AvaloniaProperty.Register<DockableArea, Dock>(
        nameof(TabPlacement), global::Avalonia.Controls.Dock.Top);

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<DockableArea, string>(
        nameof(Id));

    public static readonly StyledProperty<object?> FallbackContentProperty = AvaloniaProperty.Register<DockableArea, object?>(
        nameof(FallbackContent));

    public object? FallbackContent
    {
        get => GetValue(FallbackContentProperty);
        set => SetValue(FallbackContentProperty, value);
    }

    public Dock TabPlacement
    {
        get => GetValue(TabPlacementProperty);
        set => SetValue(TabPlacementProperty, value);
    }

    public DockableAreaRegion Region
    {
        get => GetValue(RegionProperty);
        set => SetValue(RegionProperty, value);
    }

    public ICommand FloatCommand
    {
        get => GetValue(FloatCommandProperty);
        set => SetValue(FloatCommandProperty, value);
    }

    public IDockContext Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    public IDockable? ActiveDockable
    {
        get => GetValue(ActiveDockableProperty);
        set => SetValue(ActiveDockableProperty, value);
    }

    public string Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public ObservableCollection<IDockable> Dockables
    {
        get => GetValue(DockablesProperty);
        set => SetValue(DockablesProperty, value);
    }

    private ItemsPresenter _strip;
    private TabControl tab;
    private DockingDirection? _lastDirection;

    static DockableArea()
    {
        ActiveDockableProperty.Changed.AddClassHandler<DockableArea>(ActiveDockableChanged);
        DockablesProperty.Changed.AddClassHandler<DockableArea>(DockablesChanged);
        ContextProperty.Changed.AddClassHandler<DockableArea>(ContextChanged);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        if(Context != null)
        {
            Context.FocusedHost = this;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        tab = e.NameScope.Find<TabControl>("PART_TabControl");
        tab.ApplyTemplate();
        _strip = tab.Presenter;
        tab.PointerPressed += OnTabPointerPressed;
    }

    private void OnTabPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        FocusHost();
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnTemplateChanged(e);
        if(tab != null)
        {
            tab.PointerPressed += OnTabPointerPressed;
        }
    }

    private void FocusHost()
    {
        if(Context != null)
        {
            Context.FocusedHost = this;
        }
    }

    public DockableArea()
    {
        Dockables = new ObservableCollection<IDockable>();
        ContextProperty.Changed.AddClassHandler<DockableArea>(ContextChanged);
    }

    public void AddDockable(IDockable dockable)
    {
        if (Dockables.Contains(dockable)) return;
        Dockables.Add(dockable);
    }

    public void RemoveDockable(IDockable dockable)
    {
        Dockables.Remove(dockable);
        if (ActiveDockable == dockable)
        {
            ActiveDockable = Dockables.Count > 0 ? Dockables[0] : null;
        }

        dockable.Host = null;

        if (Dockables.Count == 0)
        {
            if (FallbackContent == null)
            {
                Region.RemoveDockableArea(this);
            }
        }
    }

    public bool IsDockableWithin(int x, int y)
    {
        var pos = ToRelativePoint(x, y);
        return Bounds.Contains(pos);
    }

    private Point ToRelativePoint(int x, int y)
    {
        PixelPoint point = new PixelPoint(x, y);
        if(VisualRoot is null)
        {
            return new Point(-1, -1);
        }

        Point pos = this.PointToClient(point);

        pos += Bounds.Position;
        return pos;
    }

    public void OnDockableEntered(IDockableHostRegion region, int x, int y)
    {
        if(!region.CanDock()) return;
        PseudoClasses.Set(":dockableOver", true);
    }

    public void OnDockableOver(IDockableHostRegion region, int x, int y)
    {
        Point? pos = ToRelativePoint(x, y);

        _lastDirection = GetDockingDirection(pos.Value);
        if (_lastDirection.HasValue && region.CanDock())
        {
            bool isCenter = _lastDirection.Value == DockingDirection.Center;
            PseudoClasses.Set(":center", isCenter);

            bool isLeft = _lastDirection.Value == DockingDirection.Left;
            PseudoClasses.Set(":left", isLeft);

            bool isRight = _lastDirection.Value == DockingDirection.Right;
            PseudoClasses.Set(":right", isRight);

            bool isTop = _lastDirection.Value == DockingDirection.Top;
            PseudoClasses.Set(":top", isTop);

            bool isBottom = _lastDirection.Value == DockingDirection.Bottom;
            PseudoClasses.Set(":bottom", isBottom);
        }
        else if (IsOverStrip(pos) && region.CanDock())
        {
            _lastDirection = DockingDirection.Center;
            PseudoClasses.Set(":center", true);

            /*if (region.ValidDockable == null)
            {
                PseudoClasses.Set(":center", true);
            }
            else
            {
                var dockable = region.ValidDockable;
                Dock(dockable, DockingDirection.Center);
                tab.SelectedItem = dockable;
                var container = tab.ContainerFromItem(dockable);
                var behaviors = Interaction.GetBehaviors(container);
                foreach (var behavior in behaviors)
                {
                    if (behavior is DockableTabDragBehavior tabItemBehavior)
                    {
                        tabItemBehavior.BeginDrag(new Point(x, y) - Bounds.Position);
                    }
                }
            }*/
        }
        else
        {
            PseudoClasses.Set(":center", false);
            PseudoClasses.Set(":left", false);
            PseudoClasses.Set(":right", false);
            PseudoClasses.Set(":top", false);
            PseudoClasses.Set(":bottom", false);
        }
    }

    private bool IsOverStrip(Point? pos)
    {
        if(_strip is null)
        {
            return false;
        }

        if(pos is null)
        {
            return false;
        }

        return _strip.Bounds.Contains(pos.Value);
    }

    private DockingDirection? GetDockingDirection(Point pos)
    {
        Rect top = new Rect(0, 0, Bounds.Width, Bounds.Height / 3);
        Rect bottom = new Rect(0, Bounds.Height - Bounds.Height / 3, Bounds.Width, Bounds.Height / 3);
        Rect left = new Rect(0, 0, Bounds.Width / 3, Bounds.Height);
        Rect right = new Rect(Bounds.Width - Bounds.Width / 3, 0, Bounds.Width / 3, Bounds.Height);

        if(_strip != null && _strip.Bounds.Contains(pos))
        {
            return null;
        }

        if (top.Contains(pos))
        {
            return DockingDirection.Top;
        }

        if (bottom.Contains(pos))
        {
            return DockingDirection.Bottom;
        }

        if (left.Contains(pos))
        {
            return DockingDirection.Left;
        }

        if (right.Contains(pos))
        {
            return DockingDirection.Right;
        }

        if (Bounds.Contains(pos))
        {
            return DockingDirection.Center;
        }

        return null;
    }

    public void OnDockableExited(IDockableHostRegion region, int x, int y)
    {
        PseudoClasses.Set(":dockableOver", false);
        PseudoClasses.Set(":center", false);
        PseudoClasses.Set(":left", false);
        PseudoClasses.Set(":right", false);
        PseudoClasses.Set(":top", false);
        PseudoClasses.Set(":bottom", false);
    }

    public void Dock(IDockable dockable)
    {
        Dock(dockable, _lastDirection);
    }

    public void Dock(IDockable dockable, DockingDirection? direction)
    {
        if (direction.HasValue && direction.Value != DockingDirection.Center)
        {
            DockableArea newArea = Region.SplitDockableArea(this, direction.Value);
            Context.Dock(dockable, newArea);
        }
        else if (direction is DockingDirection.Center)
        {
            Context.Dock(dockable, this);
        }
    }

    public void Float(IDockable dockable)
    {
        Context.Float(dockable, 0, 0);
    }

    public void CloseHost()
    {
        Region.RemoveDockableArea(this);
    }

    public void Close(IDockable dockable)
    {
        Context.Close(dockable);
    }

    public void CloseAll()
    {
        for (var i = 0; i < Dockables.Count; i++)
        {
            var dockable = Dockables[i];
            Context.Close(dockable);
            i--;
        }
    }

    public void CloseAllExcept(IDockable dockable)
    {
        for (var i = 0; i < Dockables.Count; i++)
        {
            var d = Dockables[i];
            if (d != dockable)
            {
                Context.Close(d);
                i--;
            }
        }
    }

    public void SplitDown(IDockable dockable)
    {
        Dock(dockable, DockingDirection.Bottom);
    }

    public void SplitLeft(IDockable dockable)
    {
        Dock(dockable, DockingDirection.Left);
    }

    public void SplitRight(IDockable dockable)
    {
        Dock(dockable, DockingDirection.Right);
    }

    public void SplitUp(IDockable dockable)
    {
        Dock(dockable, DockingDirection.Top);
    }

    public event Action<bool>? FocusedChanged;

    private static void ActiveDockableChanged(DockableArea sender, AvaloniaPropertyChangedEventArgs args)
    {
        if(args.OldValue is IDockable oldDockable)
        {
            if (oldDockable is IDockableSelectionEvents selectionEvents)
            {
                selectionEvents.OnDeselected();
            }
        }

        if (args.NewValue is IDockable dockable)
        {
            dockable.Host = sender;
            if (sender.FloatCommand == null)
            {
                sender.FloatCommand = new RelayCommand<IDockable>(sender.Float);
            }

            if (!sender.Dockables.Contains(dockable))
            {
                sender.Dockables.Add(dockable);
            }

            if (dockable is IDockableSelectionEvents selectionEvents)
            {
                selectionEvents.OnSelected();
            }
        }
    }

    private static void DockablesChanged(DockableArea sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is ObservableCollection<IDockable> dockables)
        {
            dockables.CollectionChanged += (s, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (IDockable dockable in args.NewItems)
                    {
                        dockable.Host = sender;
                    }
                }
            };
        }
    }

    private static void ContextChanged(DockableArea area, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is IDockContext oldContext)
        {
            oldContext.FocusedHostChanged -= area.OnFocusedHostChanged;
        }
        if (args.NewValue is IDockContext context)
        {
            context.AddHost(area);
            context.FocusedHostChanged += area.OnFocusedHostChanged;

            if(context.FocusedHost == area)
            {
                area.OnFocusedHostChanged(area);
            }
        }
    }

    private void OnFocusedHostChanged(IDockableHost? host)
    {
        bool isFocused = ReferenceEquals(host, this);
        PseudoClasses.Set(":focused", isFocused);
        FocusedChanged?.Invoke(isFocused);
    }

    public IEnumerator<IDockableLayoutElement> GetEnumerator()
    {
        return Dockables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}