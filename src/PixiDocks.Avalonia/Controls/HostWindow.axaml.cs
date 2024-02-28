using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":dragging")]
public class HostWindow : Window, IHostWindow
{
    public const string DragFormat = "PixiDocks.Avalonia.Controls.Dockable";
    public IDockable? ActiveDockable { get; }

    private HostWindowTitleBar? _hostWindowTitleBar;

    private bool _mouseDown, _draggingWindow;

    private Control? _chromeGrip;
    private HostWindowState _state;
    private Point _dragStartPoint;

    protected override Type StyleKeyOverride => typeof(HostWindow);

    public HostWindow(IDockable activeDockable, IDockContext context, PixelPoint pos)
    {
        ActiveDockable = activeDockable;
        Control dockableObj = ActiveDockable as Control;
        Content = dockableObj;
        Width = dockableObj.Bounds.Width;
        Height = dockableObj.Bounds.Height;
        Position = pos;

        _state = new HostWindowState(context, this);

        PositionChanged += OnPositionChanged;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_draggingWindow)
        {
            _state.ProcessDragEvent(this.PointToScreen(_dragStartPoint), EventType.DragMove);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Content = null;
        base.OnClosing(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _hostWindowTitleBar = e.NameScope.Find<HostWindowTitleBar>("PART_TitleBar");
        if (_hostWindowTitleBar is not null)
        {
            _hostWindowTitleBar.ApplyTemplate();

            if (_hostWindowTitleBar.BackgroundControl is not null)
            {
                _hostWindowTitleBar.BackgroundControl.PointerPressed += (_, args) =>
                {
                    MoveDrag(args);
                };
            }
        }
    }

    private void MoveDrag(PointerPressedEventArgs e)
    {
        _mouseDown = true;

        PseudoClasses.Set(":dragging", true);
        _draggingWindow = true;
        _dragStartPoint = e.GetPosition(this);
        BeginMoveDrag(e);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            EndDrag(e);
        }
    }

    private void EndDrag(PointerEventArgs e)
    {
        PseudoClasses.Set(":dragging", false);

        _mouseDown = false;
        _draggingWindow = false;

        _state.ProcessDragEvent(this.PointToScreen(e.GetPosition(null)), EventType.DragEnd);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_chromeGrip is not null && _chromeGrip.IsPointerOver)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                MoveDrag(e);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_draggingWindow)
        {
            EndDrag(e);
        }
    }
}