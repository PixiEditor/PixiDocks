using System.Collections;
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

    public IReadOnlyCollection<IDockableTarget> AllTargets => CollectDockableTargets();

    public IDockable? ValidDockable => AllTargets.FirstOrDefault(x => x.Dockables.Count > 0)?.Dockables.FirstOrDefault();

    static DockableAreaRegion()
    {
        DockableAreaProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableArea dockableArea)
            {
                dockableArea.Region = sender;
                sender.Root.First = dockableArea;
            }
        });

        RootProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableTree tree)
            {
                tree.SetRegion(sender);
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
        Root = new DockableTree() { Context = Context, Region = this };
    }

    public bool CanDock()
    {
        return true;
    }

    public IDockableTarget SplitDockableArea(IDockableTarget dockableTarget, DockingDirection direction)
    {
        if (direction == DockingDirection.Center)
        {
            return dockableTarget;
        }

        DockableTree? tree = null;
        if (dockableTarget is DockableTree dockableTree)
        {
            tree = dockableTree;
        }
        else
        {
            tree = FindTree(dockableTarget);
        }

        if (tree == null)
        {
            throw new InvalidOperationException("Dockable target not found in region");
        }

        var area = tree.Split(direction, dockableTarget);
        area.Region = this;
        area.Context = dockableTarget.Context;
        return area;
    }

    public void RemoveDockableArea(DockableArea dockableArea)
    {
        var tree = FindTree(dockableArea);
        if (AllTargets.Count == 1)
        {
            return;
        }

        tree?.RemoveDockableArea(dockableArea);
    }

    private DockableTree? FindTree(IDockableTarget dockableTarget, IEnumerable root = null)
    {
        if (root == null)
        {
            root = AllTargets;
        }

        foreach (var target in root)
        {
            if (target is DockableTree tree)
            {
                if (tree.First == dockableTarget || tree.Second == dockableTarget)
                {
                    return tree;
                }
                else if (tree.First is DockableTree subTree)
                {
                    var found = FindTree(dockableTarget, subTree);
                    if (found != null)
                    {
                        return found;
                    }
                }
                else if (tree.Second is DockableTree subTree2)
                {
                    var found = FindTree(dockableTarget, subTree2);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
        }

        return null;
    }

    private IReadOnlyCollection<IDockableTarget> CollectDockableTargets()
    {
        var targets = new List<IDockableTarget>();
        foreach (var element in Root)
        {
            if (element is IDockableTarget target)
            {
                targets.Add(target);
            }
        }

        return targets;
    }
}