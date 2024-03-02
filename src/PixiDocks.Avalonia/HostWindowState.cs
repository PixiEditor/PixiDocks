using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core;

namespace PixiDocks.Avalonia;

public class HostWindowState
{
    public IDockContext Context { get; }
    public HostWindow Window { get; }

    private IDockableHost? _lastHost;
    private IDockable hostDockable;

    public HostWindowState(IDockContext context, HostWindow window)
    {
        Context = context;
        Window = window;
        hostDockable = window.ActiveDockable;

        window.Closing += (sender, args) =>
        {
            _lastHost?.OnDockableExited(hostDockable, 0, 0);
        };
    }

    public void ProcessDragEvent(PixelPoint position, EventType type)
    {
        if (type == EventType.DragMove)
        {
            if (Window.ActiveDockable != null)
            {
                if (IsOverDockHost(Window.ActiveDockable.Host, position, out var host))
                {
                    if (host != Window.ActiveDockable.Host)
                    {
                        if(_lastHost != null && _lastHost != host)
                        {
                            _lastHost.OnDockableExited(Window.ActiveDockable, position.X, position.Y);
                            _lastHost = null;
                        }

                        if (_lastHost == null)
                        {
                            host!.OnDockableEntered(Window.ActiveDockable, position.X, position.Y);
                        }
                        else
                        {
                            host!.OnDockableOver(Window.ActiveDockable, position.X, position.Y);
                        }

                        _lastHost = host;
                    }
                }
                else if (_lastHost != null)
                {
                    _lastHost.OnDockableExited(Window.ActiveDockable, position.X, position.Y);
                    _lastHost = null;
                }
            }
        }
        else if (type == EventType.DragEnd)
        {
            if(_lastHost != null)
            {
                _lastHost.OnDockableExited(hostDockable, position.X, position.Y);
                _lastHost.Dock(hostDockable);
                _lastHost = null;
            }
        }
    }

    private bool IsOverDockHost(IDockableHost? except, PixelPoint position, out IDockableHost? host)
    {
        host = null;
        foreach (IDockableHost dockHost in Context.AllHosts)
        {
            if (dockHost.IsDockableWithin(position.X, position.Y) && dockHost != except)
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