using Avalonia;
using Avalonia.Input;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core;

namespace PixiDocks.Avalonia;

public class DockContext : IDockContext
{
    private List<IDockableHost> allHosts = new();
    private Dictionary<string, HostWindow> floatingWindows = new();

    public IReadOnlyCollection<IDockableHost> AllHosts => allHosts;

    public void AddHost(IDockableHost host)
    {
        if (allHosts.Contains(host))
        {
            return;
        }

        allHosts.Add(host);
    }

    public void RemoveHost(IDockableHost host)
    {
        if (!allHosts.Contains(host))
        {
            return;
        }

        allHosts.Remove(host);
    }

    public IHostWindow Float(IDockable dockable)
    {
        if (floatingWindows.ContainsKey(dockable.Id))
        {
            return null;
        }

        PixelPoint pos = dockable switch
        {
            Dockable dockableControl => dockableControl.PointToScreen(new Point(0, -30)),
            _ => new PixelPoint(0, 0)
        };

        dockable.Host?.RemoveDockable(dockable);
        var hostWindow = new HostWindow(dockable, this, pos);
        hostWindow.Activated += OnWindowActivated;
        floatingWindows.Add(dockable.Id, hostWindow);
        hostWindow.Show();

        return hostWindow;
    }

    private void OnWindowActivated(object sender, EventArgs e)
    {
        if (sender is not HostWindow hostWindow)
        {
            return;
        }

        allHosts.Remove(hostWindow.ActiveDockable.Host);
        allHosts.Insert(0, hostWindow.ActiveDockable.Host);
    }

    public void Dock(IDockable dockable, IDockableHost toHost)
    {
        if (toHost == null)
        {
            return;
        }

        if (floatingWindows.ContainsKey(dockable.Id))
        {
            HostWindow hostWindow = floatingWindows[dockable.Id];
            hostWindow.Close();
            floatingWindows.Remove(dockable.Id);
        }

        dockable.Host?.RemoveDockable(dockable);
        toHost.AddDockable(dockable);
        toHost.ActiveDockable = dockable;
    }
}