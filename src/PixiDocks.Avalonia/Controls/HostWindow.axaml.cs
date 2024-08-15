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

    public HostWindowTitleBar? TitleBar { get; private set; }
    public DockableArea DockableArea { get; private set; }

    private bool _mouseDown, _draggingWindow;

    private HostWindowState _state;
    private Point _dragStartPoint;
    private DockableAreaRegion _dockableRegion;
    private IDockContext _dockContext;

    protected override Type StyleKeyOverride => typeof(HostWindow);

    public void Initialize(IDockable? dockable, IDockContext context, PixelPoint pos)
    {
        Control dockableObj = dockable as Control;
        Content = dockableObj;
        Width = dockableObj.Bounds.Width;
        Height = dockableObj.Bounds.Height;

        if (Width == 0) Width = 200;
        if (Height == 0) Height = 200;

        Position = pos;

        _state = new HostWindowState(context, this);
        _dockContext = context;
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
        _dockableRegion.Context = _state.Context;
       DockableArea = e.NameScope.Find<DockableArea>("PART_DockableArea");
        if (DockableArea is not null)
        {
            DockableArea.Context = _state.Context;
            DockableArea.ActiveDockable = Content as IDockable;
        }

        TitleBar = e.NameScope.Find<HostWindowTitleBar>("PART_TitleBar");
        if (TitleBar is not null)
        {
            TitleBar.ApplyTemplate();

            if (TitleBar.BackgroundControl is not null)
            {
                TitleBar.BackgroundControl.PointerPressed += (_, args) =>
                {
                    MoveDrag(args, new Point(0, 0));
                };
            }
        }
    }

    public void MoveDrag(PointerPressedEventArgs e, Point pt)
    {
        BeginMoveDrag(e);

        _mouseDown = true;
        PseudoClasses.Set(":dragging", true);
        _dragStartPoint = e.GetPosition(this) + pt;
        _state.ProcessDragEvent(this.PointToScreen(_dragStartPoint), EventType.DragStart);
        e.Pointer.Capture(this);
        _draggingWindow = true;
    }

    private void EndDrag(PointerEventArgs e)
    {
        PseudoClasses.Set(":dragging", false);

        _mouseDown = false;
        _draggingWindow = false;

        _state.ProcessDragEvent(Position, EventType.DragEnd);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (TitleBar is not null && TitleBar.IsPointerOver)
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

        if (_draggingWindow && !Equals(e.Pointer.Captured, this))
        {
            EndDrag(e);
        }
    }
}