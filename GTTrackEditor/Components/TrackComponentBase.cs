using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;

using GTTrackEditor.Interfaces;

namespace GTTrackEditor.Components;

    /// <summary>
    /// Represents a component of a global track part. This class is abstract.
    /// </summary>
public abstract class TrackComponentBase : INotifyPropertyChanged, IHideable
{
    private string _name;
    /// <summary>
    /// Name for the tree view.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private Brush _treeViewItemColor = Brushes.White;
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

    public abstract void RenderComponent();

    /* IHideable */
    public bool IsVisible { get; set; } = true;

    public abstract void Hide();

    public abstract void Show();
}

