using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaRegion : TemplatedControl, IDockableHostRegion
{
    public static readonly StyledProperty<DockableTree> OutputProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableTree>(
        nameof(Output));

    public static readonly StyledProperty<DockableArea> DockableAreaProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableArea>(
        nameof(DockableArea), new DockableArea());

    [Content]
    public DockableArea DockableArea
    {
        get => GetValue(DockableAreaProperty);
        set => SetValue(DockableAreaProperty, value);
    }

    public DockableTree Output
    {
        get => GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
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
                sender._dockableAreaToTree.Add(dockableArea, sender.Output);
                sender.Output.First = dockableArea;
            }
        });

        OutputProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableTree tree)
            {
                tree.SetRegion(sender);
            }
        });
    }

    public DockableAreaRegion()
    {
        Output = new DockableTree();
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