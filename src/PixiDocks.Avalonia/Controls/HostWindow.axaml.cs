using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PixiDocks.Core;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":dragging")]
[TemplatePart("PART_TitleBar", typeof(HostWindowTitleBar))]
[TemplatePart("PART_DockableArea", typeof(DockableArea))]
public class HostWindow : Window, IHostWindow
{
    public IDockableHostRegion Region => _dockableRegion;

    private HostWindowTitleBar? _hostWindowTitleBar;

    private bool _mouseDown, _draggingWindow;

    private Control? _chromeGrip;
    private HostWindowState _state;
    private Point _dragStartPoint;
    private DockableArea _dockableArea;
    private DockableAreaRegion _dockableRegion;

    protected override Type StyleKeyOverride => typeof(HostWindow);

    public void Initialize(IDockable dockable, IDockContext context, PixelPoint pos)
    {
        Control dockableObj = dockable as Control;
        Content = dockableObj;
        Width = dockableObj.Bounds.Width;
        Height = dockableObj.Bounds.Height;

        if (Width == 0) Width = 200;
        if (Height == 0) Height = 200;

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

        _dockableRegion = e.NameScope.Find<DockableAreaRegion>("PART_DockableRegion");
       _dockableArea = e.NameScope.Find<DockableArea>("PART_DockableArea");
        if (_dockableArea is not null)
        {
            _dockableArea.Context = _state.Context;
            _dockableArea.ActiveDockable = Content as IDockable;
        }

        _hostWindowTitleBar = e.NameScope.Find<HostWindowTitleBar>("PART_TitleBar");
        if (_hostWindowTitleBar is not null)
        {
            _hostWindowTitleBar.ApplyTemplate();

            if (_hostWindowTitleBar.BackgroundControl is not null)
            {
                _hostWindowTitleBar.BackgroundControl.PointerPressed += (_, args) =>
                {
                    MoveDrag(args, new Point(0, 0));
                };
            }
        }
    }

    public void MoveDrag(PointerPressedEventArgs e, Point pt)
    {
        _mouseDown = true;

        PseudoClasses.Set(":dragging", true);
        _draggingWindow = true;
        _dragStartPoint = e.GetPosition(this) + pt;
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
                MoveDrag(e, new Point(0, 0));
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