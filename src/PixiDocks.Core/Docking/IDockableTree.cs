using System.Text.Json.Serialization;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableTreeConverter))]
public interface IDockableTree : IDockableLayoutElement, IDockableTarget
{
    public ITreeElement First { get; set; }
    public double FirstSize { get; set; }
    public ITreeElement Second { get; set; }
    public double SecondSize { get; set; }
    public DockingDirection? SplitDirection { get; set; }
    public void Traverse(Action<ITreeElement, IDockableTree> action);
}