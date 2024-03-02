using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_GrabBorder", typeof(Border))]
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
    private Border border;
    private PointerPressedEventArgs? _lastPointerPressedEventArgs;
    private Point? _clickPoint;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        border = e.NameScope.Find<Border>("PART_GrabBorder");

        border.PointerPressed += OnBorderOnPointerPressed;
        border.PointerMoved += OnBorderOnPointerMoved;
        border.PointerReleased += OnBorderOnPointerReleased;
    }

    private void OnBorderOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            args.Pointer.Capture(border);
            _lastPointerPressedEventArgs = args;
            _clickPoint = args.GetPosition(border);
        }
    }

    private void OnBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && DistanceFromClick(e) > 10)
        {
            var window = Dockable.Host?.Context.Float(Dockable);
            if (window is HostWindow hostWindow)
            {
                hostWindow.MoveDrag(_lastPointerPressedEventArgs);
            }
            _isDragging = false;
            e.Pointer.Capture(null);
        }
    }

    private float DistanceFromClick(PointerEventArgs pointerEventArgs)
    {
        if (_clickPoint is null)
        {
            return 0;
        }

        var position = pointerEventArgs.GetPosition(border);
        return (float) Math.Sqrt(Math.Pow(position.X - _clickPoint.Value.X, 2) + Math.Pow(position.Y - _clickPoint.Value.Y, 2));
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