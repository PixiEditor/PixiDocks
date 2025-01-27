using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using PixiDocks.Avalonia.Utils;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Avalonia.Controls;

[PseudoClasses(":split", ":left", ":right", ":top", ":bottom", ":horizontal", ":vertical")]
[TemplatePart(Name = "PART_FirstPresenter", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "PART_SecondPresenter", Type = typeof(ContentPresenter))]
public class DockableTree : TemplatedControl, ITreeElement, IDockableTree
{
    public const int DockBorderThickness = 25;

    public static readonly StyledProperty<ITreeElement> FirstProperty =
        AvaloniaProperty.Register<DockableTree, ITreeElement>(
            nameof(First));

    public static readonly StyledProperty<ITreeElement?> SecondProperty =
        AvaloniaProperty.Register<DockableTree, ITreeElement?>(
            nameof(Second));

    public static readonly StyledProperty<DockingDirection?> SplitDirectionProperty =
        AvaloniaProperty.Register<DockableTree, DockingDirection?>(
            nameof(SplitDirection));

    public static readonly StyledProperty<bool> AutoExpandProperty = AvaloniaProperty.Register<DockableTree, bool>(
        nameof(AutoExpand));

    public bool AutoExpand
    {
        get => GetValue(AutoExpandProperty);
        set => SetValue(AutoExpandProperty, value);
    }

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

    int IDockableTarget.DockingOrder => 50;
    IReadOnlyCollection<IDockable?> IDockableTarget.Dockables { get; } = new List<IDockable?>();
    IDockable? IDockableTarget.ActiveDockable { get; set; }

    public static readonly StyledProperty<IDockContext> ContextProperty =
        AvaloniaProperty.Register<DockableTree, IDockContext>(
            nameof(DockContext));

    public IDockContext Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    public DockableAreaRegion Region { get; set; }
    public DockableTree? DockableParent { get; set; }

    private double TargetFirstSize
    {
        get
        {
            if(IsFirstExpanded) return 1;
            if(IsSecondExpanded) return 0;

            return _queuedFirstSize;
        }
    }

    private double TargetSecondSize
    {
        get
        {
            if(IsSecondExpanded) return 1;
            if(IsFirstExpanded) return 0;

            return _queuedSecondSize;
        }
    }
    
    private GridUnitType TargetFirstType => IsFirstExpanded ? GridUnitType.Star : _queuedFirstType;
    private GridUnitType TargetSecondType => IsSecondExpanded ? GridUnitType.Star : _queuedSecondType;
    
    private bool IsFirstExpanded => Second is null or IDockableHost { Dockables.Count: 0, FallbackContent: null } && AutoExpand;
    private bool IsSecondExpanded => First is null or IDockableHost { Dockables.Count: 0, FallbackContent: null } && AutoExpand;

    public double FirstSize
    {
        get
        {
            if (IsFirstExpanded)
            {
                return 1;
            }

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
            if (IsSecondExpanded)
            {
                return 1;
            }

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

    private DockingDirection? _lastDirection = null;

    static DockableTree()
    {
        FirstProperty.Changed.AddClassHandler<DockableTree>(ElementAttached);
        SecondProperty.Changed.AddClassHandler<DockableTree>(ElementAttached);
        ContextProperty.Changed.AddClassHandler<DockableTree>(ContextChanged);
    }

    public DockableTree()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _grid = e.NameScope.Find<Grid>("PART_Grid");
        if (TargetFirstSize == 1 && TargetSecondSize == 1)
        {
            if (SplitDirection is DockingDirection.Right or DockingDirection.Bottom)
            {
                FirstSize = 0.66;
            }
            else
            {
                SecondSize = 0.66;
            }
        }
        
        UpdateGrid();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (First is DockableArea area)
        {
            area.Dockables.CollectionChanged -= DockablesOnCollectionChanged;
        }

        if (Second is DockableArea area2)
        {
            area2.Dockables.CollectionChanged -= DockablesOnCollectionChanged;
        }
    }

    public DockableArea Split(DockingDirection direction,
        IDockableTarget areaToSplit)
    {
        DockableArea second = new();

        DockableArea area;
        DockableTree resultTree;
        if (areaToSplit == Second)
        {
            area = Second as DockableArea;
            Second = new DockableTree() { First = area, DockableParent = this, Context = Context, Region = Region };
            resultTree = Second as DockableTree;
        }
        else if (areaToSplit == First)
        {
            area = First as DockableArea;
            First = new DockableTree() { First = area, DockableParent = this, Context = Context, Region = Region };
            resultTree = First as DockableTree;
        }
        else if (Equals(areaToSplit, this))
        {
            resultTree = SplitThis(direction);
        }
        else
        {
            throw new InvalidOperationException("Area to split is not a child of this tree");
        }

        resultTree.Second = new DockableTree()
        {
            First = second, DockableParent = resultTree, Context = Context, Region = Region
        };

        resultTree.SplitDirection = direction;

        UpdateGrid();
        return second;
    }

    private DockableTree SplitThis(DockingDirection direction)
    {
        DockableTree resultTree;
        var firstElement = First;
        var secondElement = Second;

        DockableTree newTree = new DockableTree
        {
            Context = Context,
            Region = Region,
            DockableParent = this,
            First = firstElement,
            Second = secondElement,
            SplitDirection = SplitDirection
        };

        First = newTree;
        resultTree = this;
        return resultTree;
    }

    public void RemoveDockableArea(ITreeElement element)
    {
        if (First == element)
        {
            First = null;
        }
        else
        {
            Second = null;
        }

        if (First == null && Second == null)
        {
            DockableParent?.RemoveDockableArea(this);
        }

        SplitDirection = null;
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        if(_grid is null) return;
        if (First == null || Second == null)
        {
            _grid.ColumnDefinitions.Clear();
            _grid.RowDefinitions.Clear();

            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            SetPseudoClasses();
            return;
        }

        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        double splitterSize = IsFirstExpanded || IsSecondExpanded ? 0 : 5;
        if (SplitDirection is DockingDirection.Right or DockingDirection.Left)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(TargetFirstSize, TargetFirstType));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(splitterSize, GridUnitType.Pixel));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(TargetSecondSize, TargetSecondType));
        }
        else if (SplitDirection is DockingDirection.Top or DockingDirection.Bottom)
        {
            _grid.RowDefinitions.Add(new RowDefinition(TargetFirstSize, TargetFirstType));
            _grid.RowDefinitions.Add(new RowDefinition(splitterSize, GridUnitType.Pixel));
            _grid.RowDefinitions.Add(new RowDefinition(TargetSecondSize, TargetSecondType));
        }

        SetPseudoClasses();
    }


    public void SetRegion(DockableAreaRegion sender)
    {
        Region = sender;
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

    public void Dock(IDockable? dockable)
    {
        if (_lastDirection.HasValue)
        {
            var target = Region.SplitDockableArea(this, _lastDirection.Value);
            Context.Dock(dockable, target);
        }
    }

    void IDockableTarget.AddDockable(IDockable? dockable)
    {
    }

    void IDockableTarget.RemoveDockable(IDockable? dockable)
    {
    }

    public bool IsPointWithin(int x, int y)
    {
        Point point = CoordinatesUtil.ToRelativePoint(this, x, y);
        if (First is IDockableTarget target)
        {
            if (target.IsPointWithin(x, y)) return false;
        }

        if (Second is IDockableTarget target2)
        {
            if (target2.IsPointWithin(x, y)) return false;
        }

        return Bounds.Contains(point) && !Bounds.Deflate(DockBorderThickness).Contains(point);
    }

    public void OnDockableEntered(IDockableHostRegion region, int x, int y)
    {
        if (!CanDock()) return;
        PseudoClasses.Set(":dockableOver", true);
    }

    public void OnDockableOver(IDockableHostRegion region, int x, int y)
    {
        Point? pos = CoordinatesUtil.ToRelativePoint(this, x, y);

        _lastDirection = GetDockingDirection(pos.Value);
        if (_lastDirection.HasValue && CanDock())
        {
            bool isCenter = _lastDirection.Value == DockingDirection.Center;
            PseudoClasses.Set(":centerPreview", isCenter);

            bool isLeft = _lastDirection.Value == DockingDirection.Left;
            PseudoClasses.Set(":leftPreview", isLeft);

            bool isRight = _lastDirection.Value == DockingDirection.Right;
            PseudoClasses.Set(":rightPreview", isRight);

            bool isTop = _lastDirection.Value == DockingDirection.Top;
            PseudoClasses.Set(":topPreview", isTop);

            bool isBottom = _lastDirection.Value == DockingDirection.Bottom;
            PseudoClasses.Set(":bottomPreview", isBottom);
        }
        else
        {
            PseudoClasses.Set(":centerPreview", false);
            PseudoClasses.Set(":leftPreview", false);
            PseudoClasses.Set(":rightPreview", false);
            PseudoClasses.Set(":topPreview", false);
            PseudoClasses.Set(":bottomPreview", false);
        }
    }

    private DockingDirection? GetDockingDirection(Point pos)
    {
        Rect top = new Rect(0, 0, Bounds.Width, Bounds.Height / 4);
        Rect bottom = new Rect(0, Bounds.Height - Bounds.Height / 4, Bounds.Width, Bounds.Height / 4);
        Rect left = new Rect(0, 0, Bounds.Width / 4, Bounds.Height);
        Rect right = new Rect(Bounds.Width - Bounds.Width / 4, 0, Bounds.Width / 4, Bounds.Height);

        if (top.Contains(pos))
        {
            return DockingDirection.Top;
        }

        if (bottom.Contains(pos))
        {
            return DockingDirection.Bottom;
        }

        if (left.Contains(pos))
        {
            return DockingDirection.Left;
        }

        if (right.Contains(pos))
        {
            return DockingDirection.Right;
        }

        return null;
    }

    public void OnDockableExited(IDockableHostRegion region, int x, int y)
    {
        PseudoClasses.Set(":dockableOver", false);
        PseudoClasses.Set(":centerPreview", false);
        PseudoClasses.Set(":leftPreview", false);
        PseudoClasses.Set(":rightPreview", false);
        PseudoClasses.Set(":topPreview", false);
        PseudoClasses.Set(":bottomPreview", false);
    }

    public bool CanDock()
    {
        return true;
    }

    public IEnumerator<IDockableLayoutElement> GetEnumerator()
    {
        yield return this;
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
        else if (First is not null)
        {
            action(First, this);
        }

        if (Second is IDockableTree second)
        {
            second.Traverse(action);
        }
        else if (Second is not null)
        {
            action(Second, this);
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

    private static void ElementAttached(DockableTree sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DockableTree tree)
        {
            tree.DockableParent = sender;
        }
        else if (e.NewValue is DockableArea area)
        {
            area.Dockables.CollectionChanged += sender.DockablesOnCollectionChanged;
        }

        if (e.OldValue is DockableArea oldArea)
        {
            oldArea.Dockables.CollectionChanged -= sender.DockablesOnCollectionChanged;
        }
    }

    private void DockablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateGrid();
    }

    private static void ContextChanged(DockableTree sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is IDockContext oldDockContext)
        {
            oldDockContext.RemoveDockableTarget(sender);
        }

        if (e.NewValue is IDockContext dockContext)
        {
            dockContext.AddDockableTarget(sender);
        }
    }
}
