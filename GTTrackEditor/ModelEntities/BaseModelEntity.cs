using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using System.ComponentModel;
using System.Windows.Media;

using GTTrackEditor.Interfaces;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a custom mesh model entity for the track editor.
/// </summary>
public abstract class BaseModelEntity : MeshGeometryModel3D, INotifyPropertyChanged, IHideable
{
    /// <summary>
    /// Whether the model is allowed to rotate.
    /// </summary>
    public abstract bool CanRotate { get; }

    /// <summary>
    /// Whether the model is allowed to be translated.
    /// </summary>
    public abstract bool CanTranslate { get; }

    /// <summary>
    /// Whether the model is allowed to be scaled.
    /// </summary>
    public abstract bool CanScale { get; }

    private Brush _treeViewItemColor = Brushes.Black;
    /// <summary>
    /// Color for the tree view.
    /// </summary>
    public Brush TreeViewItemColor
    {
        get { return _treeViewItemColor; }
        set
        {
            _treeViewItemColor = value;
            OnPropertyChanged(nameof(TreeViewItemColor));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IsVisible { get; set; } = true;

    public void Hide()
    {
        Visibility = System.Windows.Visibility.Hidden;
        TreeViewItemColor = Brushes.Gray;

        IsVisible = false;
    }

    public void Show()
    {
        Visibility = System.Windows.Visibility.Visible;
        TreeViewItemColor = Brushes.Black;

        IsVisible = true;
    }
}
