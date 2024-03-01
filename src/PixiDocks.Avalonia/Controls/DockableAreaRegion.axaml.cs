using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaRegion : TemplatedControl
{
    public static readonly StyledProperty<DockableTree> OutputProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableTree>(
        nameof(Output), new DockableTree());

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
    }

    public DockableArea SplitDockableArea(DockableArea dockableArea, DockingDirection direction)
    {
        if (direction == DockingDirection.Center)
        {
            return dockableArea;
        }

        var tree = _dockableAreaToTree[dockableArea];
        var area = tree.Split(direction, _dockableAreaToTree, dockableArea);
        area.Region = this;
        area.Context = dockableArea.Context;
        return area;
    }

    public void RemoveDockableArea(DockableArea dockableArea)
    {
        var tree = _dockableAreaToTree[dockableArea];
        tree.RemoveDockableArea(dockableArea, _dockableAreaToTree);
    }

    public Point? TranslatePointRelative(Point pos, DockableArea dockableArea)
    {
        if (_dockableAreaToTree.TryGetValue(dockableArea, out DockableTree? tree))
        {
            return tree.FinalElement.TranslatePoint(pos, dockableArea);
        }

        return this.TranslatePoint(pos, dockableArea);
    }
}