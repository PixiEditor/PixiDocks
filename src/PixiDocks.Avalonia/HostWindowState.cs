using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia;

public class HostWindowState
{
    public IDockContext Context { get; }
    public HostWindow Window { get; }

    private IDockableHost? _lastHost;
    private bool isDraggingTab;

    public HostWindowState(IDockContext context, HostWindow window)
    {
        Context = context;
        Window = window;

        window.Closing += (sender, args) =>
        {
            _lastHost?.OnDockableExited(window.Region, 0, 0);
        };
    }

    public void ProcessDragEvent(PixelPoint position, EventType type)
    {
        if (type == EventType.DragStart && Window.DockableArea.IsPointerOverTab(position))
        {
            isDraggingTab = true;
        }
        if (type == EventType.DragMove && isDraggingTab)
        {
            if (IsOverDockHost(Window.Region.AllHosts, position, out var host))
            {
                if(_lastHost != null && _lastHost != host)
                {
                    _lastHost.OnDockableExited(Window.Region, position.X, position.Y);
                    _lastHost = null;
                }

                if (_lastHost == null)
                {
                    host!.OnDockableEntered(Window.Region, position.X, position.Y);
                }
                else
                {
                    host!.OnDockableOver(Window.Region, position.X, position.Y);
                }

                _lastHost = host;
            }
            else if (_lastHost != null)
            {
                _lastHost.OnDockableExited(Window.Region, position.X, position.Y);
                _lastHost = null;
            }
        }
        else if (type == EventType.DragEnd)
        {
            if(_lastHost != null)
            {
                _lastHost.OnDockableExited(Window.Region, position.X, position.Y);
                if (Window.Region.CanDock())
                {
                    _lastHost.Dock(Window.Region.ValidDockable);
                    _lastHost = null;
                }
            }

            isDraggingTab = false;
        }
    }

    private bool IsOverDockHost(IReadOnlyCollection<IDockableHost> except, PixelPoint position, out IDockableHost? host)
    {
        host = null;
        foreach (IDockableHost dockHost in Context.AllHosts)
        {
            if (dockHost.IsDockableWithin(position.X, position.Y) && !except.Contains(dockHost))
            {
                host = dockHost;
                return true;
            }
        }

        return false;
    }
}

public enum EventType
{
    DragStart,
    DragEnd,
    DragMove,
}