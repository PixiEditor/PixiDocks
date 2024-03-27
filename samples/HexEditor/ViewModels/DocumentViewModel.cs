using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexEditor.Models;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace HexEditor.ViewModels;

public partial class DocumentViewModel : ObservableObject, IDockableContent
{
    public string Id { get; set; }
    public string Title => _document.FilePath;
    public bool CanFloat => true;
    public bool CanClose => true;
    public object? Icon { get; set; }

    private readonly Document _document;

    public string FilePath
    {
        get => _document.FilePath;
        set => _document.FilePath = value;
    }

    public byte[] Data
    {
        get => _document.Data;
        set => _document.Data = value;
    }

    public ObservableCollection<string> Lines { get; set; } = new();

    public DocumentViewModel()
    {
    }

    public DocumentViewModel(Document document)
    {
        _document = document;
        Id = document.FilePath;
    }

    [RelayCommand]
    public void Load()
    {
        _document.Load();
        Lines = new ObservableCollection<string>(_document.ToLines());
        OnPropertyChanged(nameof(Data));
    }
}