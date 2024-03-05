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

    public IHostWindow Float(IDockable dockable, double x, double y)
    {
        if (floatingWindows.TryGetValue(dockable.Id, out HostWindow? value) && value.Region.AllHosts.Sum(x => x.Dockables.Count) == 1)
        {
            return value;
        }

        PixelPoint pos = dockable switch
        {
            Dockable dockableControl => dockableControl.PointToScreen(new Point(0, -30) + new Point(x, y)),
            _ => new PixelPoint(0, 0)
        };

        dockable.Host?.RemoveDockable(dockable);
        var hostWindow = new HostWindow(dockable, this, pos);
        hostWindow.Activated += OnWindowActivated;
        if (!floatingWindows.TryAdd(dockable.Id, hostWindow))
        {
            floatingWindows[dockable.Id] = hostWindow;
        }

        hostWindow.Show();

        return hostWindow;
    }

    private void OnWindowActivated(object sender, EventArgs e)
    {
        if (sender is not HostWindow hostWindow)
        {
            return;
        }

        for (var i = 0; i < hostWindow.Region.AllHosts.Count; i++)
        {
            var host = hostWindow.Region.AllHosts.ElementAt(i);
            allHosts.Remove(host);
            allHosts.Insert(0, host);
        }
    }

    public void Dock(IDockable dockable, IDockableHost toHost)
    {
        if (toHost == null)
        {
            return;
        }

        dockable.Host?.RemoveDockable(dockable);

        if (floatingWindows.ContainsKey(dockable.Id))
        {
            HostWindow hostWindow = floatingWindows[dockable.Id];
            if (hostWindow.Region.AllHosts.Sum(x => x.Dockables.Count) == 0)
            {
                hostWindow.Close();
                floatingWindows.Remove(dockable.Id);
            }
        }

        HostWindow? toHostWindow = TryGetHostWindow(toHost);
        if (toHostWindow is not null)
        {
            if (!floatingWindows.TryAdd(dockable.Id, toHostWindow))
            {
                floatingWindows[dockable.Id] = toHostWindow;
            }
        }

        toHost.AddDockable(dockable);
        toHost.ActiveDockable = dockable;
    }

    private HostWindow? TryGetHostWindow(IDockableHost toHost)
    {
        return floatingWindows.Values.FirstOrDefault(window => window.Region.AllHosts.Contains(toHost));
    }
}