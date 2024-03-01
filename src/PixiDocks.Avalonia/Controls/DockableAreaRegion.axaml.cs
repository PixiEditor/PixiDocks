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

    private Dictionary<int, int> _occupiedRows = new();
    private Dictionary<int, int> _occupiedColumns = new();

    static DockableAreaRegion()
    {
        DockableAreaProperty.Changed.AddClassHandler<DockableAreaRegion>((sender, args) =>
        {
            if (args.NewValue is DockableArea dockableArea)
            {
                dockableArea.Region = sender;
                sender._occupiedColumns[0] = 1;
                sender._occupiedRows[0] = 1;
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

        IncrementRow(newRow);
        IncrementColumn(newColumn);

        Grid.SetRow(newDockableArea, newRow);
        Grid.SetColumn(newDockableArea, newColumn);
        int newRowSpan = Math.Min(rowSpan, OutputGrid.RowDefinitions.Count);
        int newColumnSpan = Math.Min(columnSpan, OutputGrid.ColumnDefinitions.Count);
        Grid.SetRowSpan(newDockableArea, newRowSpan);
        Grid.SetColumnSpan(newDockableArea, newColumnSpan);
        OutputGrid.Children.Add(newDockableArea);

        return newDockableArea;
    }

    private void IncrementRow(int row)
    {
        if(row < 0)
        {
            return;
        }

        _occupiedRows.TryAdd(row, 0);
        _occupiedRows[row]++;
    }

    private void IncrementColumn(int row)
    {
        if(row < 0)
        {
            return;
        }

        _occupiedColumns.TryAdd(row, 0);
        _occupiedColumns[row]++;
    }

    private void DecrementRow(int row)
    {
        if(row < 0)
        {
            return;
        }

        _occupiedRows[row]--;
        if (_occupiedRows[row] == 0)
        {
            _occupiedRows.Remove(row);
        }
    }

    private void DecrementColumn(int column)
    {
        if(column < 0)
        {
            return;
        }

        _occupiedColumns[column]--;
        if (_occupiedColumns[column] == 0)
        {
            _occupiedColumns.Remove(column);
        }
    }

    private void ShiftRows(int newRow, int by)
    {
        foreach (var child in OutputGrid.Children)
        {
            if (child is DockableArea dockableAreaChild)
            {
                int row = Grid.GetRow(dockableAreaChild);
                int rowSpan = Grid.GetRowSpan(dockableAreaChild);
                if (row >= newRow)
                {
                    int clampedRow = Math.Max(row + by, 0);
                    Grid.SetRow(dockableAreaChild, clampedRow);
                    DecrementRow(row);
                    IncrementRow(clampedRow);
                    Grid.SetRowSpan(dockableAreaChild, Math.Clamp(rowSpan, 1, OutputGrid.RowDefinitions.Count));
                }
                else if (row + rowSpan > newRow)
                {
                    Grid.SetRowSpan(dockableAreaChild, Math.Max(rowSpan + by, 1));
                }
            }
        }
    }

    private void ShiftColumns(int newColumn, int by)
    {
        foreach (var child in OutputGrid.Children)
        {
            if (child is DockableArea dockableAreaChild)
            {
                int column = Grid.GetColumn(dockableAreaChild);
                int columnSpan = Grid.GetColumnSpan(dockableAreaChild);
                if (column >= newColumn)
                {
                    int clampedColumn = Math.Max(column + by, 0);
                    Grid.SetColumn(dockableAreaChild, clampedColumn);
                    DecrementColumn(column);
                    IncrementColumn(clampedColumn);
                    Grid.SetColumnSpan(dockableAreaChild, Math.Clamp(columnSpan, 1, OutputGrid.ColumnDefinitions.Count));
                }
                else if (column + columnSpan > newColumn)
                {
                    Grid.SetColumnSpan(dockableAreaChild, Math.Max(columnSpan + by, 1));
                }
            }
        }
    }

    public void RemoveDockableArea(DockableArea dockableArea)
    {
        int row = Grid.GetRow(dockableArea);
        int column = Grid.GetColumn(dockableArea);
        OutputGrid.Children.Remove(dockableArea);

        _occupiedRows[row]--;
        _occupiedColumns[column]--;

        bool shifted = false;

        if (_occupiedRows[row] == 0)
        {
            OutputGrid.RowDefinitions.RemoveAt(row);
            _occupiedRows.Remove(row);
            ShiftRows(row, -1);
            shifted = true;
        }

        if (_occupiedColumns[column] == 0)
        {
            OutputGrid.ColumnDefinitions.RemoveAt(column);
            _occupiedColumns.Remove(column);
            ShiftColumns(column, -1);
            shifted = true;
        }

        if (!shifted && _occupiedColumns.ContainsKey(column + 1) && _occupiedColumns[column] == _occupiedColumns[column + 1])
        {
            OutputGrid.ColumnDefinitions.RemoveAt(column);
            ShiftColumns(column, -1);
        }
        else if (!shifted && _occupiedRows.ContainsKey(row + 1) && _occupiedRows[row] == _occupiedRows[row + 1])
        {
            OutputGrid.RowDefinitions.RemoveAt(row);
            ShiftRows(row, -1);
        }
    }
}
