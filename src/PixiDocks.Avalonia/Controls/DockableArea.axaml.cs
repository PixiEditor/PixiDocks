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
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiDocks.Avalonia.Helpers;
using PixiDocks.Avalonia.Utils;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":dockableOver", ":center", ":left", ":right", ":top", ":bottom", ":focused")]
public class DockableArea : TemplatedControl, IDockableHost, ITreeElement
{
    private const int BorderDockMargin = 10;
    int IDockableTarget.DockingOrder => 100;

    IDockContext IDockableTarget.Context
    {
        get => Context;
        set => Context = value;
    }

    IReadOnlyCollection<IDockable?> IDockableTarget.Dockables => Dockables;

    IDockable? IDockableTarget.ActiveDockable
    {
        get => ActiveDockable;
        set
        {
            ActiveDockable = value;
        }
    }

    public static readonly StyledProperty<ObservableCollection<IDockable?>> DockablesProperty =
        AvaloniaProperty.Register<DockableArea, ObservableCollection<IDockable>>(
            nameof(Dockables));

    public static readonly StyledProperty<ICommand> FloatCommandProperty =
        AvaloniaProperty.Register<DockableArea, ICommand>(
            nameof(FloatCommand));

    public static readonly StyledProperty<IDockContext> ContextProperty =
        AvaloniaProperty.Register<DockableArea, IDockContext>(
            nameof(Context));

    public static readonly StyledProperty<IDockable?> ActiveDockableProperty =
        AvaloniaProperty.Register<DockableArea, IDockable?>(
            nameof(ActiveDockable));

    public static readonly StyledProperty<DockableAreaRegion> RegionProperty =
        AvaloniaProperty.Register<DockableArea, DockableAreaRegion>(
            nameof(Region));

    public static readonly StyledProperty<Dock> TabPlacementProperty = AvaloniaProperty.Register<DockableArea, Dock>(
        nameof(TabPlacement), global::Avalonia.Controls.Dock.Top);

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<DockableArea, string>(
        nameof(Id));

    public static readonly StyledProperty<object?> FallbackContentProperty =
        AvaloniaProperty.Register<DockableArea, object?>(
            nameof(FallbackContent));

    public static readonly StyledProperty<bool> CloseRegionOnEmptyProperty =
        AvaloniaProperty.Register<DockableArea, bool>(
            nameof(CloseRegionOnEmpty), true);

    public bool CloseRegionOnEmpty
    {
        get => GetValue(CloseRegionOnEmptyProperty);
        set => SetValue(CloseRegionOnEmptyProperty, value);
    }

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

    public ObservableCollection<IDockable?> Dockables
    {
        get => GetValue(DockablesProperty);
        set => SetValue(DockablesProperty, value);
    }

    private ItemsPresenter _strip;
    private TabControl tabControl;
    private DockingDirection? _lastDirection;
    private DockableArea _lastDockingTarget;

    static DockableArea()
    {
        ActiveDockableProperty.Changed.AddClassHandler<DockableArea>(ActiveDockableChanged);
        DockablesProperty.Changed.AddClassHandler<DockableArea>(DockablesChanged);
        ContextProperty.Changed.AddClassHandler<DockableArea>(ContextChanged);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        if (Context != null)
        {
            Context.FocusedTarget = this;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        tabControl = e.NameScope.Find<TabControl>("PART_TabControl");
        tabControl.ApplyTemplate();
        _strip = tabControl.Presenter;
        tabControl.PointerPressed += OnTabControlPointerPressed;
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
    }

    private void OnTabControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        FocusHost();
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        FocusHost();
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnTemplateChanged(e);
        if (tabControl != null)
        {
            tabControl.PointerPressed += OnTabControlPointerPressed;
        }
    }

    private void FocusHost()
    {
        if (Context != null)
        {
            Context.FocusedTarget = this;
        }
    }

    public DockableArea()
    {
        Dockables = new ObservableCollection<IDockable?>();
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        TemplateApplied += OnTemplateApplied;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
       UpdateTabControl(); 
    }

    private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        UpdateTabControl();
    }

    private void UpdateTabControl()
    {
        bool isOverTab = false;

        if (VisualRoot is HostWindow hostWindow)
        {
            var relative = FindRelativePos(tabControl, hostWindow);

            isOverTab = relative is { X: <= 65, Y: <= 25 };
        }

        tabControl.Classes.Set("overTab", isOverTab);
    }

    public void AddDockable(IDockable? dockable)
    {
        if (Dockables.Contains(dockable)) return;
        Dockables.Add(dockable);
    }

    public void RemoveDockable(IDockable? dockable)
    {
        Dockables.Remove(dockable);
        if (ActiveDockable == dockable)
        {
            ActiveDockable = Dockables.Count > 0 ? Dockables[0] : null;
        }

        dockable.Host = null;

        if (Dockables.Count == 0)
        {
            if (FallbackContent == null && CloseRegionOnEmpty)
            {
                Region.RemoveDockableArea(this);
            }
        }
    }

    public bool IsPointWithin(int x, int y)
    {
        var pos = CoordinatesUtil.ToRelativePoint(this, x, y);
        return Bounds.Deflate(new Thickness(BorderDockMargin, 0, BorderDockMargin, BorderDockMargin)).Contains(pos);
    }

    public void OnDockableEntered(IDockableHostRegion region, int x, int y)
    {
        if (!CanDock()) return;
        PseudoClasses.Set(":dockableOver", true);
    }

    public void OnDockableOver(IDockableHostRegion region, int x, int y)
    {
        Point? pos = CoordinatesUtil.ToRelativePoint(this, x, y);

        _lastDirection = GetDockingDirection(pos.Value);
        if (_lastDirection.HasValue && CanDock())
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
        else if (IsOverStrip(pos) && CanDock())
        {
            _lastDirection = DockingDirection.Center;
            PseudoClasses.Set(":center", true);
            PseudoClasses.Set(":left", false);
            PseudoClasses.Set(":right", false);
            PseudoClasses.Set(":top", false);
            PseudoClasses.Set(":bottom", false);

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

    public void OnDockableExited(IDockableHostRegion region, int x, int y)
    {
        PseudoClasses.Set(":dockableOver", false);
        PseudoClasses.Set(":center", false);
        PseudoClasses.Set(":left", false);
        PseudoClasses.Set(":right", false);
        PseudoClasses.Set(":top", false);
        PseudoClasses.Set(":bottom", false);
    }

    public bool CanDock()
    {
        return true;
    }

    private bool IsOverStrip(Point? pos)
    {
        if (_strip is null)
        {
            return false;
        }

        if (pos is null)
        {
            return false;
        }

        return _strip.Bounds.Contains(pos.Value);
    }

    private DockingDirection? GetDockingDirection(Point pos)
    {
        Rect top = new Rect(0, 0, Bounds.Width, Bounds.Height / 4);
        Rect bottom = new Rect(0, Bounds.Height - Bounds.Height / 4, Bounds.Width, Bounds.Height / 4);
        Rect left = new Rect(0, 0, Bounds.Width / 4, Bounds.Height);
        Rect right = new Rect(Bounds.Width - Bounds.Width / 4, 0, Bounds.Width / 4, Bounds.Height);

        if (_strip != null && _strip.Bounds.Contains(pos))
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

        if (Dockables.Count == 0 && Bounds.Contains(pos))
        {
            return DockingDirection.Center;
        }

        return null;
    }

    public void Dock(IDockable? dockable)
    {
        Dock(dockable, _lastDirection);
    }

    public void Dock(IDockable? dockable, DockingDirection? direction)
    {
        if (direction.HasValue && direction.Value != DockingDirection.Center)
        {
            IDockableTarget newArea = Region.SplitDockableArea(this, direction.Value);
            Context.Dock(dockable, newArea);
        }
        else if (direction is DockingDirection.Center)
        {
            Context.Dock(dockable, this);
        }
    }

    public void Float(IDockable? dockable)
    {
        Context.Float(dockable, 0, 0);
    }

    public void CloseHost()
    {
        Region.RemoveDockableArea(this);
    }

    public async void Close(IDockable? dockable)
    {
        await Context.Close(dockable);
    }

    public async void CloseAll()
    {
        for (var i = 0; i < Dockables.Count; i++)
        {
            var dockable = Dockables[i];
            await Context.Close(dockable);
            i--;

            Dockables.Remove(dockable);
        }
    }

    public async void CloseAllExcept(IDockable dockable)
    {
        for (var i = 0; i < Dockables.Count; i++)
        {
            var d = Dockables[i];
            if (d != dockable)
            {
                await Context.Close(d);
                i--;

                Dockables.Remove(d);
            }
        }
    }

    public void SplitDown(IDockable? dockable)
    {
        Dock(dockable, DockingDirection.Bottom);
    }

    public void SplitLeft(IDockable? dockable)
    {
        Dock(dockable, DockingDirection.Left);
    }

    public void SplitRight(IDockable? dockable)
    {
        Dock(dockable, DockingDirection.Right);
    }

    public void SplitUp(IDockable? dockable)
    {
        Dock(dockable, DockingDirection.Top);
    }

    public bool IsPointerOverTab(PixelPoint position)
    {
        if (tabControl is null || !this.IsAttachedToVisualTree())
        {
            return false;
        }

        Point pos = this.PointToClient(position);
        return tabControl.GetRealizedContainers().Any(x => x.Bounds.Contains(pos - tabControl.ItemsPanelRoot.Bounds.Position));
    }

    public event Action<bool>? FocusedChanged;

    private static Point? FindRelativePos(Control control, Visual relativeTo)
    {
        if (control is null)
        {
            return null;
        }

        Point position = control.Bounds.Position;

        var parent = control.GetVisualParent();

        while (parent != null && parent != relativeTo)
        {
            position += parent.Bounds.Position;
            parent = parent.GetVisualParent();
        }

        return position;
    }

    private static void ActiveDockableChanged(DockableArea sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is IDockable oldDockable)
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
            context.AddDockableTarget(area);
            context.FocusedHostChanged += area.OnFocusedHostChanged;

            if (context.FocusedTarget == area)
            {
                area.OnFocusedHostChanged(area, true);
            }
        }
    }

    private void OnFocusedHostChanged(IDockableTarget? host, bool selecting)
    {
        bool isFocused = ReferenceEquals(host, this);
        PseudoClasses.Set(":focused", isFocused);
        if (isFocused)
        {
            if (!selecting) return;

            if (ActiveDockable is IDockableSelectionEvents selectionEvents)
            {
                selectionEvents.OnSelected();
            }
        }
        else
        {
            if (selecting) return;

            if (ActiveDockable is IDockableSelectionEvents selectionEvents)
            {
                selectionEvents.OnDeselected();
            }
        }

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
