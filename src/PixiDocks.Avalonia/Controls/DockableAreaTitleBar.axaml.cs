using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_GrabBorder", typeof(Border))]
public class DockableAreaTitleBar : TemplatedControl
{
    public static readonly StyledProperty<DockableArea> DockableAreaProperty = AvaloniaProperty.Register<DockableAreaTitleBar, DockableArea>(
        nameof(DockableArea));

    public DockableArea DockableArea
    {
        get => GetValue(DockableAreaProperty);
        set => SetValue(DockableAreaProperty, value);
    }

    private bool _isDragging;
    private Border border;
    private PointerPressedEventArgs? _lastPointerPressedEventArgs;

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
        }
    }

    private void OnBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var window = DockableArea.Context.Float(DockableArea.ActiveDockable);
            if (window is HostWindow hostWindow)
            {
                hostWindow.MoveDrag(_lastPointerPressedEventArgs);
            }
            _isDragging = false;
            e.Pointer.Capture(null);
        }
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