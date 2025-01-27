using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia;

public class DockContext : IDockContext
{
    private IDockableTarget? _focusedHost;
    private List<IDockableTarget> allTargets = new();
    private List<IDockableHostRegion> allRegions = new();
    private Dictionary<string, HostWindow> floatingWindows = new();

    public IReadOnlyCollection<IDockableHostRegion> AllRegions => allRegions;
    public IReadOnlyCollection<IDockableTarget> AllTargets => allTargets;

    public IDockableTarget? FocusedTarget
    {
        get => _focusedHost;
        set
        {
            if (_focusedHost == value)
            {
                return;
            }

            FocusedHostChanged?.Invoke(_focusedHost, false);
            _focusedHost = value;
            FocusedHostChanged?.Invoke(_focusedHost, true);
        }
    }

    public Func<HostWindow> HostWindowFactory { get; set; } = () => new HostWindow();
    public event Action<HostWindow>? WindowFloated;
    public event Action<IDockable>? DockableClosed;

    public event Action<IDockableTarget?, bool>? FocusedHostChanged;
    public bool IsFloating(IDockableHost dockableHost)
    {
        return floatingWindows.Values.Any(window => window.Region.AllTargets.Contains(dockableHost));
    }

    public void AddDockableTarget(IDockableTarget target)
    {
        if (allTargets.Contains(target))
        {
            return;
        }

        allTargets.Add(target);
    }

    public void RemoveDockableTarget(IDockableTarget target)
    {
        if (!allTargets.Contains(target))
        {
            return;
        }

        allTargets.Remove(target);
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

    public IDockable? CreateDockable(IDockableContent content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        Dockable? dockable = new()
        {
            Id = content.Id,
            Title = content.Title,
            CanFloat = content.CanFloat,
            CanClose = content.CanClose,
            TabCustomizationSettings = content.TabCustomizationSettings,
            DockableContent = content,
            Content = content,
        };

        CreateBindings(dockable);

        return dockable;
    }

    private void CreateBindings(Dockable? dockable)
    {
        Binding titleBinding = new(nameof(dockable.DockableContent.Title), BindingMode.TwoWay)
        {
            Source = dockable.DockableContent,
        };

        Binding tabCustomizationSettingsBinding = new(nameof(dockable.DockableContent.TabCustomizationSettings), BindingMode.TwoWay)
        {
            Source = dockable.DockableContent,
        };

        dockable.Bind(Dockable.TitleProperty, titleBinding);
        dockable.Bind(Dockable.TabCustomizationSettingsProperty, tabCustomizationSettingsBinding);
    }

    public IHostWindow Float(IDockable? dockable, double x, double y)
    {
        if (floatingWindows.TryGetValue(dockable.Id, out HostWindow? value) && value.Region.AllTargets.Sum(x => x.Dockables.Count) == 1)
        {
            return value;
        }

        if (dockable.Host != null)
        {
            dockable.Host.ActiveDockable = dockable;
        }

        PixelPoint pos = dockable switch
        {
             Dockable dockableControl => dockableControl.IsAttachedToVisualTree() ? dockableControl.PointToScreen(new Point(x, y)) : new PixelPoint((int)x, (int)y),
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

        WindowFloated?.Invoke(hostWindow);
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

    public async Task<bool> Close(IDockable? dockable)
    {
        if (!dockable.CanClose) return false;

        if (dockable is IDockableCloseEvents closeEvents)
        {
            if (!await closeEvents.OnClose())
            {
                return false;
            }
        }
        
        dockable.Host?.RemoveDockable(dockable);

        if (floatingWindows.TryGetValue(dockable.Id, out HostWindow? value))
        {
            if (value.Region.ValidDockable == null)
            {
                value.Close();
                floatingWindows.Remove(dockable.Id);
            }
        }

        DockableClosed?.Invoke(dockable);
        
        return true;
    }

    private void OnWindowActivated(object sender, EventArgs e)
    {
        if (sender is not HostWindow hostWindow)
        {
            return;
        }

        for (var i = 0; i < hostWindow.Region.AllTargets.Count; i++)
        {
            var host = hostWindow.Region.AllTargets.ElementAt(i);
            allTargets.Remove(host);
            allTargets.Insert(0, host);
        }

        allRegions.Add(hostWindow.Region);
    }

    public void Dock(IDockable? dockable, IDockableTarget toHost)
    {
        if (toHost == null)
        {
            return;
        }

        dockable.Host?.RemoveDockable(dockable);

        if (floatingWindows.ContainsKey(dockable.Id))
        {
            HostWindow hostWindow = floatingWindows[dockable.Id];
            if (hostWindow.Region.AllTargets.Sum(x => x.Dockables.Count) == 0)
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
        FocusedTarget = toHost;
    }

    private HostWindow? TryGetHostWindow(IDockableTarget toHost)
    {
        return floatingWindows.Values.FirstOrDefault(window => window.Region.AllTargets.Contains(toHost));
    }
}
