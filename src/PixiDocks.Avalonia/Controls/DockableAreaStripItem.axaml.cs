using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiDocks.Core;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaStripItem : TemplatedControl
{
    public static readonly StyledProperty<IDockable?> DockableProperty =
        AvaloniaProperty.Register<DockableAreaStripItem, IDockable>(
            nameof(Dockable));

    public IDockable? Dockable
    {
        get => GetValue(DockableProperty);
        set => SetValue(DockableProperty, value);
    }

    private bool _isDragging;
    private PointerPressedEventArgs? _lastPointerPressedEventArgs;
    private Point? _clickPoint;
    private Panel _parent;
    private TabItem _tabItem;
    private TabControl _tabControl;

    static DockableAreaStripItem()
    {
        DockableProperty.Changed.AddClassHandler<DockableAreaStripItem>(DockableChanged);
    }

    private static void DockableChanged(DockableAreaStripItem strip, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is IDockable oldDockable)
        {
            if (oldDockable.Host != null)
            {
                oldDockable.Host.Context.FocusedHostChanged -= strip.FocusedHostChanged;
            }
        }

        if (e.NewValue is IDockable dockable)
        {
            if (dockable.Host != null)
            {
                dockable.Host.Context.FocusedHostChanged += strip.FocusedHostChanged;
            }
        }

        strip.UpdateStripPseudoClasses();
    }

    private void FocusedHostChanged(IDockableTarget? obj, bool selecting)
    {
        UpdateStripPseudoClasses();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _tabItem = this.FindAncestorOfType<TabItem>();
        _tabItem.PointerPressed += OnBorderOnPointerPressed;
        _tabItem.PointerMoved += OnBorderOnPointerMoved;
        _tabItem.PointerReleased += OnBorderOnPointerReleased;
        _parent = _tabItem.FindAncestorOfType<Panel>();
        _tabControl = _tabItem.FindAncestorOfType<TabControl>();
        _tabControl.SelectionChanged += (sender, args) => UpdateStripPseudoClasses();
        UpdateStripPseudoClasses();
    }

    private void OnBorderOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var properties = args.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            args.Pointer.Capture(_tabItem);
            _lastPointerPressedEventArgs = args;
            _clickPoint = args.GetPosition(_parent);
        }
        else if (properties.IsMiddleButtonPressed)
        {
            Dockable?.Host?.Context.Close(Dockable);
        }
    }

    private void OnBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            if (Dockable != null && (IsOutsideStripBar(e) || Dockable.Host?.Dockables.Count == 1))
            {
                Point pt = e.GetPosition(_parent);
                Point diff = pt - _clickPoint!.Value;

                if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                {
                    FloatUnix(e, pt, diff);
                }
                else
                {
                    FloatWindowsOs(pt, diff, e.Pointer);
                }

                _isDragging = false;
                e.Handled = true;
            }
        }
    }

    private void FloatUnix(PointerEventArgs e, Point pt, Point diff)
    {
        bool wasFloating = Dockable.Host.Context.IsFloating(Dockable.Host);
        if (!wasFloating)
        {
            Point leftMargin = new Point(75, 0);
            pt += leftMargin;
        }

        var window = Dockable.Host?.Context.Float(Dockable, _clickPoint.Value.X, -pt.Y - diff.Y);
        if (window is HostWindow hostWindow)
        {
            e.Pointer.Capture(hostWindow);

            Point startPos = new Point(_tabItem.Bounds.Width / 2, _tabItem.Bounds.Height / 2);
            if (this.IsAttachedToVisualTree())
            {
                pt += new Point(_parent.Margin.Left, 0);
                startPos = pt;
            }
            else
            {
                Point toAdd =
                    new Point(hostWindow.FindDescendantOfType<TabControl>().ItemsPanelRoot.Margin.Left, 0);
                startPos += toAdd;
            }

            hostWindow.MoveUntilReleased(startPos);
        }
    }

    private void FloatWindowsOs(Point pt, Point diff, IPointer pointer)
    {
        var window = Dockable.Host?.Context.Float(Dockable, pt.X - 50, pt.Y - 40);
        if (window is HostWindow hostWindow)
        {
            hostWindow.MoveDrag(_lastPointerPressedEventArgs, diff);
        }

        pointer.Capture(null);

        _isDragging = false;
    }

    private bool IsOutsideStripBar(PointerEventArgs pointerEventArgs)
    {
        var position = pointerEventArgs.GetPosition(_tabItem);
        return position.X < 0 || position.X > _parent.Bounds.Width || position.Y < 0 ||
               position.Y > _parent.Bounds.Height;
    }

    private void OnBorderOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }
    }

    private void UpdateStripPseudoClasses()
    {
        PseudoClasses.Set(":selected", _tabItem != null && _tabItem.IsSelected);
        PseudoClasses.Set(":focused", Dockable != null && Dockable.Host?.Context.FocusedTarget == Dockable.Host);
    }
}
