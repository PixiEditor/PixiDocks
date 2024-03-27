using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaRegion : TemplatedControl, IDockableHostRegion
{
    public static readonly StyledProperty<DockableTree> RootProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableTree>(
        nameof(Root));

    public static readonly StyledProperty<DockableArea> DockableAreaProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableArea>(
        nameof(DockableArea), new DockableArea());

    public static readonly StyledProperty<IDockContext> ContextProperty = AvaloniaProperty.Register<DockableAreaRegion, IDockContext>(
        nameof(Context));

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<DockableAreaRegion, string>(
        nameof(Id));

    public string Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public IDockContext Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    [Content]
    public DockableArea DockableArea
    {
        get => GetValue(DockableAreaProperty);
        set => SetValue(DockableAreaProperty, value);
    }

    public IDockableTree Root
    {
        get => GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public IReadOnlyCollection<IDockableHost> AllHosts => _dockableAreaToTree.Keys;

    public IDockable ValidDockable => AllHosts.First().ActiveDockable;

    private Dictionary<DockableArea, DockableTree> _dockableAreaToTree = new();

    static DockableAreaRegion()
    {
        DockableAreaProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableArea dockableArea)
            {
                dockableArea.Region = sender;
                sender._dockableAreaToTree.Add(dockableArea, sender.Root as DockableTree);
                sender.Root.First = dockableArea;
            }
        });

        RootProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableTree tree)
            {
                tree.SetRegion(sender);
                tree.Traverse((element, parent) =>
                {
                    if (element is DockableArea area)
                    {
                        sender._dockableAreaToTree.Add(area, parent as DockableTree);
                    }
                });
            }
        });

        ContextProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.OldValue is IDockContext oldDockContext)
            {
                oldDockContext.RemoveRegion(sender);
            }

            if (args.NewValue is IDockContext dockContext)
            {
               dockContext.AddRegion(sender);
            }
        });
    }

    public DockableAreaRegion()
    {
        Root = new DockableTree();
    }

    public bool CanDock()
    {
        return AllHosts.Count == 1 && AllHosts.First().Dockables.Count == 1;
    }

    public DockableArea SplitDockableArea(DockableArea dockableArea, DockingDirection direction)
    {
        if (direction == DockingDirection.Center)
        {
            return dockableArea;
        }

        var tree = _dockableAreaToTree[dockableArea];
        var area = tree.Split(direction, _dockableAreaToTree);
        area.Region = this;
        area.Context = dockableArea.Context;
        return area;
    }

    public void RemoveDockableArea(DockableArea dockableArea)
    {
        var tree = _dockableAreaToTree[dockableArea];
        if (_dockableAreaToTree.Count == 1)
        {
            return;
        }

        tree.RemoveDockableArea(dockableArea, _dockableAreaToTree);
    }

    public Point? TranslatePointRelative(Point pos, DockableArea dockableArea)
    {
        if (_dockableAreaToTree.TryGetValue(dockableArea, out DockableTree? tree))
        {
            return tree.TranslatePoint(pos, dockableArea);
        }

        return this.TranslatePoint(pos, dockableArea);
    }
}