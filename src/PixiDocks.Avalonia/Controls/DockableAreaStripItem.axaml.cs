using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using PixiDocks.Core;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaStripItem : TemplatedControl
{
    public static readonly StyledProperty<IDockable> DockableProperty = AvaloniaProperty.Register<DockableAreaStripItem, IDockable>(
        nameof(Dockable));

    public IDockable Dockable
    {
        get => GetValue(DockableProperty);
        set => SetValue(DockableProperty, value);
    }

    private bool _isDragging;
    private PointerPressedEventArgs? _lastPointerPressedEventArgs;
    private Point? _clickPoint;
    private Panel _parent;
    private TabItem _tabItem;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _tabItem = this.FindAncestorOfType<TabItem>();
        _tabItem.PointerPressed += OnBorderOnPointerPressed;
        _tabItem.PointerMoved += OnBorderOnPointerMoved;
        _tabItem.PointerReleased += OnBorderOnPointerReleased;
        _parent = _tabItem.FindAncestorOfType<Panel>();
    }

    private void OnBorderOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            args.Pointer.Capture(_tabItem);
            _lastPointerPressedEventArgs = args;
            _clickPoint = args.GetPosition(_parent);
        }
    }

    private void OnBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            if (IsOutsideStripBar(e) || Dockable.Host?.Dockables.Count == 1)
            {
                Point pt = e.GetPosition(_parent);
                Point diff = pt - _clickPoint!.Value;
                var window = Dockable.Host?.Context.Float(Dockable, pt.X, pt.Y);
                if (window is HostWindow hostWindow)
                {
                    hostWindow.MoveDrag(_lastPointerPressedEventArgs, diff);
                }

                e.Pointer.Capture(null);
                _isDragging = false;
            }
        }
    }

    private bool IsOutsideStripBar(PointerEventArgs pointerEventArgs)
    {
        var position = pointerEventArgs.GetPosition(_tabItem);
        return position.X < 0 || position.X > _parent.Bounds.Width || position.Y < 0 || position.Y > _parent.Bounds.Height;
    }

    private void OnBorderOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }
    }
}