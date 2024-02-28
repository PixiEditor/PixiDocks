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
                sender._splitDockableAreas.Add(dockableArea, DockingDirection.Center);
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
        int rowSpan = Grid.GetRowSpan(dockableArea);
        int columnSpan = Grid.GetColumnSpan(dockableArea);

        switch (direction)
        {
            case DockingDirection.Top:
                OutputGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                ShiftRows(currentRow, 1);
                newRow = currentRow;
                columnSpan = Math.Min(columnSpan + 1, OutputGrid.ColumnDefinitions.Count);
                break;
            case DockingDirection.Bottom:
                OutputGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                ShiftRows(currentRow + 1, 1);
                newRow = currentRow + 1;
                columnSpan = Math.Min(columnSpan + 1, OutputGrid.ColumnDefinitions.Count);
                break;
            case DockingDirection.Left:
                OutputGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                ShiftColumns(currentColumn, 1);
                newColumn = currentColumn;
                rowSpan = Math.Min(rowSpan + 1, OutputGrid.RowDefinitions.Count);
                break;
            case DockingDirection.Right:
                OutputGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                ShiftColumns(currentColumn + 1, 1);
                newColumn = currentColumn + 1;
                rowSpan = Math.Min(rowSpan + 1, OutputGrid.RowDefinitions.Count);
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
        Grid.SetRowSpan(newDockableArea, rowSpan);
        Grid.SetColumnSpan(newDockableArea, columnSpan);
        OutputGrid.Children.Add(newDockableArea);

        return newDockableArea;
    }

    private void ShiftRows(int newRow, int by)
    {
        foreach (var child in OutputGrid.Children)
        {
            if (child is DockableArea dockableAreaChild && Grid.GetRow(dockableAreaChild) >= newRow)
            {
                Grid.SetRow(dockableAreaChild, Grid.GetRow(dockableAreaChild) + by);
            }
        }
    }

    private void ShiftColumns(int newColumn, int by)
    {
        foreach (var child in OutputGrid.Children)
        {
            if (child is DockableArea dockableAreaChild && Grid.GetColumn(dockableAreaChild) >= newColumn)
            {
                Grid.SetColumn(dockableAreaChild, Grid.GetColumn(dockableAreaChild) + by);
            }
        }
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
            ShiftRows(row, -1);
        }
        else if (direction is DockingDirection.Left or DockingDirection.Right)
        {
            OutputGrid.ColumnDefinitions.RemoveAt(column);
            ShiftColumns(column, -1);
        }

        _splitDockableAreas.Remove(dockableArea);
    }
}
