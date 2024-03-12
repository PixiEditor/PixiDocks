using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

[JsonConverter(typeof(LayoutTreeConverter))]
public struct LayoutTree
{
    public static Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type>();
    public static Dictionary<string, Type> TypeResolver = new Dictionary<string, Type>();
    public IDockableTree Root { get; set; }

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

    public void ApplyDockables(List<IDockable> dockables)
    {
        Traverse((element) =>
        {
            if (element is IDockableHost host)
            {
                foreach (var dockable in dockables)
                {
                    var found = host.Dockables.FirstOrDefault(d => d.Id == dockable.Id);
                    if (found != null)
                    {
                        host.AddDockable(dockable);
                        host.RemoveDockable(found);
                    }
                }
            }
        });
    }

    public void SetContext(IDockContext dockContext)
    {
        Traverse((element) =>
        {
            if (element is IDockableHost host)
            {
                host.Context = dockContext;
            }
        });
    }
}