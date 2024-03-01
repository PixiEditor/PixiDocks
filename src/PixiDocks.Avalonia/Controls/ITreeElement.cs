using Avalonia;
using Avalonia.Controls;

namespace PixiDocks.Avalonia.Controls;

public interface ITreeElement
{
    public Control FinalElement { get; }
}