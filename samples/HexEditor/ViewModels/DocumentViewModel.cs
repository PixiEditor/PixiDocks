using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexEditor.Models;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiDocks.Core.Serialization;

namespace HexEditor.ViewModels;

public partial class DocumentViewModel : ObservableObject, IDockableContent, IDockableSelectionEvents
{
    public string Id { get; set; }
    public string Title => _document.FileName;
    public bool CanFloat => true;
    public bool CanClose => true;
    public TabCustomizationSettings TabCustomizationSettings { get; } = new(showCloseButton: true);

    private readonly Document _document;
    private int _selectionStart;
    private int _selectionEnd;

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

    public event Action Selected;
    public event Action Deselected;

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
        if (_document.FilePath.EndsWith(".png") || _document.FilePath.EndsWith(".jpg") || _document.FilePath.EndsWith(".jpeg"))
        {
            TabCustomizationSettings.Icon = LoadThumbnail();
        }

        Lines = new ObservableCollection<string>(_document.ToLines());
        OnPropertyChanged(nameof(Data));
    }

    [RelayCommand]
    public void GotFocus()
    {
        Selected?.Invoke();
    }

    private IImage LoadThumbnail()
    {
        return new Bitmap(_document.FilePath);
    }

    void IDockableSelectionEvents.OnSelected()
    {
        Selected?.Invoke();
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        Deselected?.Invoke();
    }
}