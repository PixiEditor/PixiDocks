using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace PixiDocks.Avalonia.Panels;

// A panel that lays tabs with size of their content, when there is enough space, but if not, it lays them proportionally smaller
// Measure should behave like StackPanel when there is enough space, but when there is not, flex weights should be calculated and
// tabs should be laid out proportionally to their weights within the available space
public class TabGrid : Panel
{
    double[] desiredWidths;
    float[] flexWeights;
    bool applyFlex = false;

    protected override Size MeasureOverride(Size availableSize)
    {
        var totalSize = new Size();
        double childrenDesiredWidth = 0;
        applyFlex = false;

        if (desiredWidths == null || Children.Count != desiredWidths.Length)
        {
            desiredWidths = new double[Children.Count];
        }

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            child.Measure(Size.Infinity);
            childrenDesiredWidth += child.DesiredSize.Width;
            desiredWidths[i] = child.DesiredSize.Width;
            totalSize = totalSize.WithWidth(totalSize.Width + child.DesiredSize.Width);
            totalSize = totalSize.WithHeight(Math.Max(totalSize.Height, child.DesiredSize.Height));
        }

        if (childrenDesiredWidth > availableSize.Width)
        {
            applyFlex = true;
            if (flexWeights == null || Children.Count != flexWeights.Length)
            {
                flexWeights = new float[Children.Count];
            }

            CalculateFlexWeights(flexWeights, desiredWidths);
            double uniformWidth = availableSize.Width / Children.Count;
            foreach (var child in Children)
            {
                float flexWeight = flexWeights[Children.IndexOf(child)];
                child.Measure(new Size(availableSize.Width * flexWeight, availableSize.Height));
                totalSize = totalSize.WithWidth(totalSize.Width + child.DesiredSize.Width);
                totalSize = totalSize.WithHeight(Math.Max(totalSize.Height, child.DesiredSize.Height));
            }
        }

        return totalSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalWidth = 0;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var childWidth = child.DesiredSize.Width;
            var childHeight = child.DesiredSize.Height;
            var childFinalWidth = childWidth;
            var childFinalHeight = childHeight;

            if (applyFlex)
            {
                childFinalWidth = finalSize.Width * flexWeights[i];
            }

            child.Arrange(new Rect(totalWidth, 0, childFinalWidth, childFinalHeight));
            totalWidth += childFinalWidth;
        }

        return finalSize;
    }

    private void CalculateFlexWeights(float[] flexWeights, double[] desiredWidths)
    {
        double totalDesiredWidth = desiredWidths.Sum();
        for (int i = 0; i < desiredWidths.Length; i++)
        {
            flexWeights[i] = (float)(desiredWidths[i] / totalDesiredWidth);
        }
    }
}