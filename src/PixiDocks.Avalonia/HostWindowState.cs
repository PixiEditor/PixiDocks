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
            _lastHost?.OnDockableExited(hostDockable);
        };
    }

    public void ProcessDragEvent(PixelPoint position, EventType type)
    {
        if (type == EventType.DragMove)
        {
            if (Window.ActiveDockable != null)
            {
                if (IsOverDockHost(position, out var host))
                {
                    if (host != Window.ActiveDockable.Host)
                    {
                        if (_lastHost == null)
                        {
                            host!.OnDockableEntered(Window.ActiveDockable);
                        }
                        else
                        {
                            host!.OnDockableOver(Window.ActiveDockable);
                        }

                        _lastHost = host;
                    }
                }
                else if (_lastHost != null)
                {
                    _lastHost.OnDockableExited(Window.ActiveDockable);
                    _lastHost = null;
                }
            }
        }
        else if (type == EventType.DragEnd)
        {
            if(_lastHost != null)
            {
                Context.Dock(hostDockable, _lastHost);
                _lastHost = null;
            }
        }
    }

    private bool IsOverDockHost(PixelPoint position, out IDockableHost? host)
    {
        host = null;
        foreach (IDockableHost dockHost in Context.AllHosts)
        {
            Control c;
            if (dockHost.IsDockableWithin(position.X, position.Y))
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