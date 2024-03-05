using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using PixiDocks.Avalonia.Helpers;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_DockingPicker", typeof(DockingPicker))]
[PseudoClasses(":dockableOver", ":center", ":left", ":right", ":top", ":bottom")]
public class DockableArea : TemplatedControl, IDockableHost, ITreeElement
{
    IDockContext IDockableHost.Context => Context;

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

    public static readonly StyledProperty<IDockable> ActiveDockableProperty = AvaloniaProperty.Register<DockableArea, IDockable>(
        nameof(ActiveDockable));

    public static readonly StyledProperty<DockableAreaRegion> RegionProperty = AvaloniaProperty.Register<DockableArea, DockableAreaRegion>(
        nameof(Region));

    public static readonly StyledProperty<Dock> TabPlacementProperty = AvaloniaProperty.Register<DockableArea, Dock>(
        nameof(TabPlacement), global::Avalonia.Controls.Dock.Top);

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

    public IDockable ActiveDockable
    {
        get => GetValue(ActiveDockableProperty);
        set => SetValue(ActiveDockableProperty, value);
    }

    public ObservableCollection<IDockable> Dockables
    {
        get => GetValue(DockablesProperty);
        set => SetValue(DockablesProperty, value);
    }

    private DockingPicker _picker;
    private DockingDirection? _lastDirection;

    static DockableArea()
    {
        ActiveDockableProperty.Changed.AddClassHandler<DockableArea>(ActiveDockableChanged);
        ContextProperty.Changed.AddClassHandler<DockableArea>(ContextChanged);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _picker = e.NameScope.Find<DockingPicker>("PART_DockingPicker");
    }

    public DockableArea()
    {
        Dockables = new ObservableCollection<IDockable>();
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
            Region.RemoveDockableArea(this);
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
        pos = Region.TranslatePointRelative(pos.Value, this);

        pos -= _picker.Bounds.Position;
        _lastDirection = _picker.GetDockingDirection(pos.Value);
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

    public void Dock(IDockable dockable)
    {
        if (!Equals(dockable.Host, this))
        {
            if (_lastDirection.HasValue && _lastDirection.Value != DockingDirection.Center)
            {
                DockableArea newArea = Region.SplitDockableArea(this, _lastDirection.Value);
                Context.Dock(dockable, newArea);
            }
            else if (_lastDirection.HasValue && _lastDirection.Value == DockingDirection.Center)
            {
                Context.Dock(dockable, this);
            }
        }
    }

    private static void ActiveDockableChanged(DockableArea sender, AvaloniaPropertyChangedEventArgs args)
    {
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
        }
    }

    private void Float(IDockable dockable)
    {
        Context.Float(dockable, 0, 0);
    }

    private static void ContextChanged(DockableArea area, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is IDockContext context)
        {
            context.AddHost(area);
        }
    }

    Control ITreeElement.FinalElement => this;
}