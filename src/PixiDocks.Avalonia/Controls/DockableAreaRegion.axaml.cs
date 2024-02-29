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

        PrintGridTable();
    }

    private void IncrementColumn(int row)
    {
        if(row < 0)
        {
            return;
        }

        _occupiedColumns.TryAdd(row, 0);
        _occupiedColumns[row]++;

        PrintGridTable();
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

        PrintGridTable();
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

        PrintGridTable();
    }

    private void PrintGridTable()
    {
        Console.WriteLine("Grid Table:");
        for (int i = 0; i < OutputGrid.RowDefinitions.Count; i++)
        {
            for (int j = 0; j < OutputGrid.ColumnDefinitions.Count; j++)
            {
                if(_occupiedRows.ContainsKey(i) && _occupiedColumns.ContainsKey(j))
                {
                    int value = _occupiedRows[i] * _occupiedColumns[j];
                    Console.Write(value);
                }
                else
                {
                    Console.Write("O");
                }
            }
            Console.WriteLine();
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
                    Grid.SetRow(dockableAreaChild, row + by);
                    DecrementRow(row);
                    IncrementRow(row + by);
                }
                else if (row + rowSpan > newRow)
                {
                    Grid.SetRowSpan(dockableAreaChild, rowSpan + by);
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
                    Grid.SetColumn(dockableAreaChild, column + by);
                    DecrementColumn(column);
                    IncrementColumn(column + by);
                }
                else if (column + columnSpan > newColumn)
                {
                    Grid.SetColumnSpan(dockableAreaChild, columnSpan + by);
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

        if (_occupiedRows[row] == 0)
        {
            OutputGrid.RowDefinitions.RemoveAt(row);
            _occupiedRows.Remove(row);
            ShiftRows(row, -1);
        }

        if (_occupiedColumns[column] == 0)
        {
            OutputGrid.ColumnDefinitions.RemoveAt(column);
            _occupiedColumns.Remove(column);
            ShiftColumns(column, -1);
        }
    }
}
