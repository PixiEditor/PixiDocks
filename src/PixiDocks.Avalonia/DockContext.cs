using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.VisualTree;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia;

public class DockContext : IDockContext
{
    private IDockableHost? _focusedHost;
    private List<IDockableHost> allHosts = new();
    private List<IDockableHostRegion> allRegions = new();
    private Dictionary<string, HostWindow> floatingWindows = new();

    public IReadOnlyCollection<IDockableHostRegion> AllRegions => allRegions;
    public IReadOnlyCollection<IDockableHost> AllHosts => allHosts;

    public IDockableHost? FocusedHost
    {
        get => _focusedHost;
        set
        {
            if (_focusedHost == value)
            {
                return;
            }

            _focusedHost = value;
            FocusedHostChanged?.Invoke(_focusedHost);
        }
    }

    public Func<HostWindow> HostWindowFactory { get; set; } = () => new HostWindow();

    public event Action<IDockableHost?>? FocusedHostChanged;

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

    public void AddRegion(IDockableHostRegion sender)
    {
        if (allRegions.Contains(sender))
        {
            return;
        }

        allRegions.Add(sender);
    }

    public string Serialize()
    {
        SerializedLayout layout = new();
        foreach (IDockableHostRegion region in allRegions)
        {
            LayoutTree tree = new()
            {
                Root = region.Root
            };

            layout.LayoutRegions.Add(region.Id, tree);
        }

        return JsonSerializer.Serialize(layout);
    }


    public void RemoveRegion(IDockableHostRegion sender)
    {
        if (!allRegions.Contains(sender))
        {
            return;
        }

        allRegions.Remove(sender);
    }

    public IDockable CreateDockable(IDockableContent content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        Dockable dockable = new()
        {
            Id = content.Id,
            Title = content.Title,
            CanFloat = content.CanFloat,
            CanClose = content.CanClose,
            Icon = (IImage)content.Icon,
            Content = content
        };

        return dockable;
    }

    public IHostWindow Float(IDockable dockable, double x, double y)
    {
        if (floatingWindows.TryGetValue(dockable.Id, out HostWindow? value) && value.Region.AllHosts.Sum(x => x.Dockables.Count) == 1)
        {
            return value;
        }

        if (dockable.Host != null)
        {
            dockable.Host.ActiveDockable = dockable;
        }

        PixelPoint pos = dockable switch
        {
            Dockable dockableControl => dockableControl.IsAttachedToVisualTree() ? dockableControl.PointToScreen(new Point(0, -30) + new Point(x, y)) : new PixelPoint((int)x, (int)y),
            _ => new PixelPoint(0, 0)
        };

        dockable.Host?.RemoveDockable(dockable);
        var hostWindow = HostWindowFactory();
        hostWindow.Initialize(dockable, this, pos);
        hostWindow.Closing += HostWindowOnClosing;
        hostWindow.Activated += OnWindowActivated;
        if (!floatingWindows.TryAdd(dockable.Id, hostWindow))
        {
            floatingWindows[dockable.Id] = hostWindow;
        }

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            hostWindow.Show(desktop.MainWindow);
        }
        else
        {
            hostWindow.Show();
        }

        return hostWindow;
    }

    private void HostWindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (sender is not HostWindow hostWindow)
        {
            return;
        }

        allRegions.Remove(hostWindow.Region);
        hostWindow.Closing -= HostWindowOnClosing;
        hostWindow.Activated -= OnWindowActivated;

        var keys = floatingWindows.Where(x => x.Value == hostWindow).ToArray();
        foreach (var key in keys)
        {
            floatingWindows.Remove(key.Key);
        }
    }

    public void Close(IDockable dockable)
    {
        if(!dockable.CanClose) return;

        if (dockable is IDockableCloseEvents closeEvents)
        {
            if (!closeEvents.OnClose())
            {
                return;
            }
        }

        if (floatingWindows.TryGetValue(dockable.Id, out HostWindow? value))
        {
            value.Close();
            floatingWindows.Remove(dockable.Id);
        }

        dockable.Host?.RemoveDockable(dockable);
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

        allRegions.Add(hostWindow.Region);
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
        FocusedHost = toHost;
    }

    private HostWindow? TryGetHostWindow(IDockableHost toHost)
    {
        return floatingWindows.Values.FirstOrDefault(window => window.Region.AllHosts.Contains(toHost));
    }
}