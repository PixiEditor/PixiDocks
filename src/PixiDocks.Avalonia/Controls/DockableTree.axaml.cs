using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":split", ":left", ":right", ":top", ":bottom", ":horizontal", ":vertical")]
public class DockableTree : TemplatedControl, ITreeElement
{
    public static readonly StyledProperty<ITreeElement?> FirstProperty = AvaloniaProperty.Register<DockableTree, ITreeElement>(
        nameof(First));

    public static readonly StyledProperty<ITreeElement?> SecondProperty = AvaloniaProperty.Register<DockableTree, ITreeElement?>(
        nameof(Second));

    public static readonly StyledProperty<DockingDirection?> SplitDirectionProperty = AvaloniaProperty.Register<DockableTree, DockingDirection?>(
        nameof(SplitDirection));

    public DockingDirection? SplitDirection
    {
        get => GetValue(SplitDirectionProperty);
        set => SetValue(SplitDirectionProperty, value);
    }

    public ITreeElement? First
    {
        get => GetValue(FirstProperty);
        set => SetValue(FirstProperty, value);
    }

    public ITreeElement? Second
    {
        get => GetValue(SecondProperty);
        set => SetValue(SecondProperty, value);
    }
    public DockableTree? DockableParent { get; set; }

    private Grid _grid;

    public DockableTree()
    {

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _grid = e.NameScope.Find<Grid>("PART_Grid");
    }

    public DockableArea Split(DockingDirection direction, Dictionary<DockableArea, DockableTree> dockableAreaToTree)
    {
        SplitDirection = direction;
        DockableArea second = new();

        DockableArea area = First as DockableArea;
        DetachOldParents(area);
        First = new DockableTree() { First = area, DockableParent = this };
        dockableAreaToTree[area] = First as DockableTree;

        Second = new DockableTree() { First = second, DockableParent = this };
        dockableAreaToTree.Add(second, Second as DockableTree);

        UpdateGrid();
        return second;
    }

    private void DetachOldParents(DockableArea? area)
    {
        if (area is null)
        {
            return;
        }

        foreach (var dockable in area.Dockables)
        {
            if (dockable is Dockable dockableControl)
            {
                if (dockableControl.Parent is TabItem tabItem)
                {
                    tabItem.Content = null; // This removes Dockable as a content from old tabitem which doesn't have any parent
                }
            }
        }
    }

    public void RemoveDockableArea(ITreeElement element, Dictionary<DockableArea, DockableTree> cache)
    {
        if (First == element)
        {
            First = null;
        }
        else
        {
            Second = null;
        }

        if (element is DockableArea area)
        {
            cache.Remove(area);
        }

        if (First == null && Second == null)
        {
            DockableParent?.RemoveDockableArea(this, cache);
        }

        SplitDirection = null;
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        if (First == null || Second == null)
        {
            _grid.ColumnDefinitions.Clear();
            _grid.RowDefinitions.Clear();

            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            SetPseudoClasses();
            return;
        }

        if (SplitDirection is DockingDirection.Right or DockingDirection.Left)
        {
            _grid.ColumnDefinitions.Clear();
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(5, GridUnitType.Pixel));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        }
        else if (SplitDirection == DockingDirection.Top || SplitDirection == DockingDirection.Bottom)
        {
            _grid.RowDefinitions.Clear();
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            _grid.RowDefinitions.Add(new RowDefinition(5, GridUnitType.Pixel));
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
        }

        SetPseudoClasses();
    }

    private void SetPseudoClasses()
    {
        PseudoClasses.Set(":split", SplitDirection.HasValue);
        PseudoClasses.Set(":left", SplitDirection is DockingDirection.Left);
        PseudoClasses.Set(":right", SplitDirection is DockingDirection.Right);
        PseudoClasses.Set(":top", SplitDirection is DockingDirection.Top);
        PseudoClasses.Set(":bottom", SplitDirection is DockingDirection.Bottom);
        PseudoClasses.Set(":horizontal", SplitDirection is DockingDirection.Left or DockingDirection.Right);
        PseudoClasses.Set(":vertical", SplitDirection is DockingDirection.Top or DockingDirection.Bottom);
    }

    public IEnumerator<IDockableLayoutElement> GetEnumerator()
    {
        if (First is IDockableLayoutElement first)
        {
            yield return first;
        }

        if (Second is IDockableLayoutElement second)
        {
            yield return second;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}