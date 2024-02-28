using System.Collections.ObjectModel;
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

[TemplatePart("PART_Drop", typeof(AvaloniaObject))]
[TemplatePart("PART_DockingPicker", typeof(DockingPicker))]
public class DockableArea : TemplatedControl, IDockableHost
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
            nameof(Dockables), new ObservableCollection<IDockable>());

    public static readonly StyledProperty<ICommand> FloatCommandProperty = AvaloniaProperty.Register<DockableArea, ICommand>(
        "FloatCommand");

    public ICommand FloatCommand
    {
        get => GetValue(FloatCommandProperty);
        set => SetValue(FloatCommandProperty, value);
    }

    public static readonly StyledProperty<IDockContext> ContextProperty = AvaloniaProperty.Register<DockableArea, IDockContext>(
        nameof(Context));

    public IDockContext Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    public static readonly StyledProperty<IDockable> ActiveDockableProperty = AvaloniaProperty.Register<DockableArea, IDockable>(
        nameof(ActiveDockable));

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

    static DockableArea()
    {
        ActiveDockableProperty.Changed.AddClassHandler<DockableArea>(ActiveDockableChanged);
        ContextProperty.Changed.AddClassHandler<DockableArea>(ContextChanged);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        AvaloniaObject drop = e.NameScope.Find<AvaloniaObject>("PART_Drop");
        AddHandler(DragDrop.DropEvent, Drop);

        _picker = e.NameScope.Find<DockingPicker>("PART_DockingPicker");
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
    }

    public bool IsDockableWithin(int x, int y)
    {
        var pos = ToRelativePoint(x, y);
        return Bounds.Contains(pos);
    }

    private Point ToRelativePoint(int x, int y)
    {
        PixelPoint point = new PixelPoint(x, y);
        Point pos = this.PointToClient(point);

        pos += Bounds.Position;
        return pos;
    }

    public void OnDockableEntered(IDockable dockable, int x, int y)
    {
        PseudoClasses.Set(":dockableOver", true);
    }

    public void OnDockableOver(IDockable dockable, int x, int y)
    {
        Point? pos = ToRelativePoint(x, y);
        pos -= _picker.Bounds.Position;
        DockingDirection? direction = _picker.GetDockingDirection(pos.Value);
        if (direction.HasValue)
        {
            bool isCenter = direction.Value == DockingDirection.Center;
            PseudoClasses.Set(":center", isCenter);

            bool isLeft = direction.Value == DockingDirection.Left;
            PseudoClasses.Set(":left", isLeft);

            bool isRight = direction.Value == DockingDirection.Right;
            PseudoClasses.Set(":right", isRight);

            bool isTop = direction.Value == DockingDirection.Top;
            PseudoClasses.Set(":top", isTop);

            bool isBottom = direction.Value == DockingDirection.Bottom;
            PseudoClasses.Set(":bottom", isBottom);
        }
    }

    public void OnDockableExited(IDockable dockable, int x, int y)
    {
        PseudoClasses.Set(":dockableOver", false);
        PseudoClasses.Set(":center", false);
        PseudoClasses.Set(":left", false);
        PseudoClasses.Set(":right", false);
        PseudoClasses.Set(":top", false);
        PseudoClasses.Set(":bottom", false);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(HostWindow.DragFormat))
        {
            IDockable dockable = (IDockable)e.Data.Get(HostWindow.DragFormat);
            if(dockable == null) return;

            if (!Equals(dockable.Host, this))
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
            sender.FloatCommand = new RelayCommand<IDockable>(sender.Context.Float);
        }
    }

    private static void ContextChanged(DockableArea area, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is IDockContext context)
        {
            context.AddHost(area);
        }
    }
}