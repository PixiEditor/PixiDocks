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
[TemplatePart(Name = "PART_FirstPresenter", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "PART_SecondPresenter", Type = typeof(ContentPresenter))]
public class DockableTree : TemplatedControl, ITreeElement, IDockableTree
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

    public static readonly StyledProperty<string> IdProperty = AvaloniaProperty.Register<DockableTree, string>(
        nameof(Id));

    public string Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public DockableTree? DockableParent { get; set; }

    public double FirstSize
    {
        get
        {
            if (SplitDirection.HasValue)
            {
                return SplitDirection is DockingDirection.Left or DockingDirection.Right
                    ? _grid.ColumnDefinitions[0].Width.Value
                    : _grid.RowDefinitions[0].Height.Value;
            }

            return 1;
        }
        set
        {
            _queuedFirstSize = value;
            if (_queuedFirstSize <= 1)
            {
                _queuedFirstType = GridUnitType.Star;
                _queuedSecondSize = 1 - _queuedFirstSize;
                _queuedSecondType = GridUnitType.Star;
            }
            else
            {
                _queuedFirstType = GridUnitType.Pixel;
            }
        }
    }

    public double SecondSize
    {
        get
        {
            if (SplitDirection.HasValue)
            {
                return SplitDirection is DockingDirection.Left or DockingDirection.Right
                    ? _grid.ColumnDefinitions[2].Width.Value
                    : _grid.RowDefinitions[2].Height.Value;
            }

            return 1;
        }
        set
        {
           _queuedSecondSize = value;
            if (_queuedSecondSize <= 1)
            {
                _queuedSecondType = GridUnitType.Star;
                _queuedFirstSize = 1 - _queuedSecondSize;
                _queuedFirstType = GridUnitType.Star;
            }
            else
            {
                _queuedSecondType = GridUnitType.Pixel;
            }
        }
    }

    private Grid _grid;
    private double _queuedFirstSize = 1;
    private GridUnitType _queuedFirstType = GridUnitType.Star;
    private double _queuedSecondSize = 1;
    private GridUnitType _queuedSecondType = GridUnitType.Star;

    static DockableTree()
    {
        FirstProperty.Changed.AddClassHandler<DockableTree>(ElementAttached);
        SecondProperty.Changed.AddClassHandler<DockableTree>(ElementAttached);
    }

    private static void ElementAttached(DockableTree sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DockableTree tree)
        {
            tree.DockableParent = sender;
        }
    }

    public DockableTree()
    {

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _grid = e.NameScope.Find<Grid>("PART_Grid");
        UpdateGrid();
    }

    public DockableArea Split(DockingDirection direction, Dictionary<DockableArea, DockableTree> dockableAreaToTree,
        DockableArea areaToSplit)
    {
        DockableArea second = new();

        DockableArea area;
        DockableTree resultTree;
        if (areaToSplit == Second)
        {
            area = Second as DockableArea;
            DetachOldParents(area);
            Second = new DockableTree() { First = area, DockableParent = this };
            resultTree = Second as DockableTree;
        }
        else
        {
            area = First as DockableArea;
            DetachOldParents(area);
            First = new DockableTree() { First = area, DockableParent = this };
            resultTree = First as DockableTree;
        }
        dockableAreaToTree[area] = resultTree;

        resultTree.Second = new DockableTree() { First = second, DockableParent = resultTree };
        dockableAreaToTree.Add(second, resultTree.Second as DockableTree);

        resultTree.SplitDirection = direction;

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

            _grid.ColumnDefinitions.Add(new ColumnDefinition(_queuedFirstSize, _queuedFirstType));
            _grid.RowDefinitions.Add(new RowDefinition(_queuedSecondSize, _queuedSecondType));
            SetPseudoClasses();
            return;
        }

        if (SplitDirection is DockingDirection.Right or DockingDirection.Left)
        {
            _grid.ColumnDefinitions.Clear();
            _grid.ColumnDefinitions.Add(new ColumnDefinition(_queuedFirstSize, _queuedFirstType));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(5, GridUnitType.Pixel));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(_queuedSecondSize, _queuedSecondType));
        }
        else if (SplitDirection is DockingDirection.Top or DockingDirection.Bottom)
        {
            _grid.RowDefinitions.Clear();
            _grid.RowDefinitions.Add(new RowDefinition(_queuedFirstSize, _queuedFirstType));
            _grid.RowDefinitions.Add(new RowDefinition(5, GridUnitType.Pixel));
            _grid.RowDefinitions.Add(new RowDefinition(_queuedSecondSize, _queuedSecondType));
        }

        SetPseudoClasses();
    }


    public void SetRegion(DockableAreaRegion sender)
    {
        if (First is DockableArea area)
        {
            area.Region = sender;
        }
        else if (First is DockableTree first)
        {
            first.SetRegion(sender);
        }

        if (Second is DockableArea second)
        {
            second.Region = sender;
        }
        else if (Second is DockableTree secondTree)
        {
            secondTree.SetRegion(sender);
        }
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

            foreach (var element in first)
            {
                yield return element;
            }
        }

        if (Second is IDockableLayoutElement second)
        {
            yield return second;

            foreach (var element in second)
            {
                yield return element;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Traverse(Action<ITreeElement, IDockableTree> action)
    {
        action(this, DockableParent);
        if (First is IDockableTree first)
        {
            first.Traverse(action);
        }
        else if(First is not null)
        {
            action(First, this);
        }

        if (Second is IDockableTree second)
        {
            second.Traverse(action);
        }
        else if(Second is not null)
        {
            action(Second, this);
        }
    }
}