using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

public class DockableAreaRegion : TemplatedControl
{
    public static readonly StyledProperty<Grid> OutputGridProperty = AvaloniaProperty.Register<DockableAreaRegion, Grid>(
        nameof(OutputGrid), new Grid() { RowDefinitions = new RowDefinitions("*"), ColumnDefinitions = new ColumnDefinitions("*") });

    public static readonly StyledProperty<DockableArea> DockableAreaProperty = AvaloniaProperty.Register<DockableAreaRegion, DockableArea>(
        nameof(DockableArea), new DockableArea());

    [Content]
    public DockableArea DockableArea
    {
        get => GetValue(DockableAreaProperty);
        set => SetValue(DockableAreaProperty, value);
    }

    public Grid OutputGrid
    {
        get => GetValue(OutputGridProperty);
        set => SetValue(OutputGridProperty, value);
    }

    private Dictionary<DockableArea, DockingDirection> _splitDockableAreas = new();

    static DockableAreaRegion()
    {
        DockableAreaProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableArea dockableArea)
            {
                dockableArea.Region = sender;
                sender.OutputGrid.Children.Add(dockableArea);
            }
        });
    }

    public DockableArea SplitDockableArea(DockableArea dockableArea, DockingDirection direction)
    {
        int currentRow = Grid.GetRow(dockableArea);
        int currentColumn = Grid.GetColumn(dockableArea);
        int newRow = 0;
        int newColumn = 0;

        switch (direction)
        {
            case DockingDirection.Top:
                OutputGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                Grid.SetRow(dockableArea, currentRow + 1);
                newRow = currentRow;
                break;
            case DockingDirection.Bottom:
                OutputGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                newRow = currentRow + 1;
                break;
            case DockingDirection.Left:
                OutputGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                Grid.SetColumn(dockableArea, currentColumn + 1);
                newColumn = currentColumn;
                break;
            case DockingDirection.Right:
                OutputGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                newColumn = currentColumn + 1;
                break;
            case DockingDirection.Center:
                return dockableArea;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        var newDockableArea = new DockableArea();
        newDockableArea.Region = this;
        newDockableArea.Context = dockableArea.Context;
        _splitDockableAreas.Add(newDockableArea, direction);
        Grid.SetRow(newDockableArea, newRow);
        Grid.SetColumn(newDockableArea, newColumn);
        OutputGrid.Children.Add(newDockableArea);

        return newDockableArea;
    }

    public void RemoveDockableArea(DockableArea dockableArea)
    {
        int row = Grid.GetRow(dockableArea);
        int column = Grid.GetColumn(dockableArea);
        OutputGrid.Children.Remove(dockableArea);

        DockingDirection direction = _splitDockableAreas[dockableArea];

        if (direction is DockingDirection.Top or DockingDirection.Bottom)
        {
            OutputGrid.RowDefinitions.RemoveAt(row);
            foreach (var child in OutputGrid.Children)
            {
                if (child is DockableArea dockableAreaChild && Grid.GetRow(dockableAreaChild) > row)
                {
                    Grid.SetRow(dockableAreaChild, Grid.GetRow(dockableAreaChild) - 1);
                }
            }
        }
        else if (direction is DockingDirection.Left or DockingDirection.Right)
        {
            OutputGrid.ColumnDefinitions.RemoveAt(column);
            foreach (var child in OutputGrid.Children)
            {
                if (child is DockableArea dockableAreaChild && Grid.GetColumn(dockableAreaChild) > column)
                {
                    Grid.SetColumn(dockableAreaChild, Grid.GetColumn(dockableAreaChild) - 1);
                }
            }
        }

        _splitDockableAreas.Remove(dockableArea);
    }
}
