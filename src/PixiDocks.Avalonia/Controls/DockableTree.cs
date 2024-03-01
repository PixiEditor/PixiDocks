using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

namespace PixiDocks.Avalonia.Controls;

public class DockableTree : AvaloniaObject, ITreeElement
{
    public DockableTree? Parent { get; set; }
    public ITreeElement? First
    {
        get => _first;
        set
        {
            _first = value;
            UpdateFirst();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalElement)));
        }
    }

    public ITreeElement? Second
    {
        get => _second;
        set
        {
            _second = value;
            UpdateSecond();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalElement)));
        }
    }

    public DockingDirection? SplitDirection { get; set; }

    public Control FinalElement => _grid;

    private Grid _grid = new Grid();
    private ITreeElement _first;
    private ITreeElement? _second;

    public event PropertyChangedEventHandler PropertyChanged;

    public DockableTree()
    {
    }

    public DockableArea Split(DockingDirection direction, Dictionary<DockableArea, DockableTree> dockableAreaToTree,
        DockableArea dockableArea)
    {
        SplitDirection = direction;
        DockableArea second = new();
        var first = First;
        First = null;

        if(direction is DockingDirection.Right or DockingDirection.Bottom)
        {
            First = new DockableTree() { First = first, Parent = this };
            Second = new DockableTree() { First = second, Parent = this };
            dockableAreaToTree.Add(second, Second as DockableTree);
            dockableAreaToTree[dockableArea] = First as DockableTree;
        }
        else
        {
            First = new DockableTree() { First = second, Parent = this };
            Second = new DockableTree() { First = first, Parent = this };
            dockableAreaToTree.Add(second, First as DockableTree);
            dockableAreaToTree[dockableArea] = Second as DockableTree;
        }

        UpdateGrid();
        return second;
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
            Parent?.RemoveDockableArea(this, cache);
        }

        SplitDirection = null;
        UpdateGrid();
    }

    private void UpdateFirst()
    {
        if(_grid.Children.Count == 0 && _first != null)
        {
            _grid.Children.Add(_first.FinalElement);
        }
        else if(_first != null)
        {
            _grid.Children[0] = _first.FinalElement;
        }
        else if(_grid.Children.Count > 0)
        {
            _grid.Children.RemoveAt(0);
        }
    }

    private void UpdateSecond()
    {
        if(_grid.Children.Count >= 1 && _second != null)
        {
            _grid.Children.Add(_second.FinalElement);
        }
        else if(_second != null)
        {
            _grid.Children[1] = _second.FinalElement;
        }
        else if(_grid.Children.Count > 1)
        {
            _grid.Children.RemoveAt(1);
        }
    }

    private void UpdateGrid()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        if (!SplitDirection.HasValue)
        {
            if (_first != null)
            {
                _grid.Children.Add(_first.FinalElement);
            }
            if (_second != null)
            {
                _grid.Children.Add(_second.FinalElement);
            }
        }
        else if (SplitDirection == DockingDirection.Right || SplitDirection == DockingDirection.Left)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.Children.Add(_first.FinalElement);
            Grid.SetColumn(_first.FinalElement, 0);
            if (_second != null)
            {
                _grid.Children.Add(_second.FinalElement);
                Grid.SetColumn(_second.FinalElement, 1);
            }
        }
        else if (SplitDirection == DockingDirection.Top || SplitDirection == DockingDirection.Bottom)
        {
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            _grid.Children.Add(_first.FinalElement);
            Grid.SetRow(_first.FinalElement, 0);
            if (_second != null)
            {
                _grid.Children.Add(_second.FinalElement);
                Grid.SetRow(_second.FinalElement, 1);
            }
        }
    }
}