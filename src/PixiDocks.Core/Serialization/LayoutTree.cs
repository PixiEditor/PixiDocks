namespace PixiDocks.Core.Serialization;

public struct LayoutTree
{
    public IDockableLayoutElement Root { get; set; }

    public void Traverse(Action<IDockableLayoutElement> action)
    {
        if (Root != null)
        {
            Traverse(Root, action);
        }
    }

    private void Traverse(IDockableLayoutElement element, Action<IDockableLayoutElement> action)
    {
        action(element);
        foreach (var child in element)
        {
            Traverse(child, action);
        }
    }
}