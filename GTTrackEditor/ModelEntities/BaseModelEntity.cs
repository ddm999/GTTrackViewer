using HelixToolkit.Wpf.SharpDX;

using System.ComponentModel;
using System.Windows.Media;

using GTTrackEditor.Interfaces;
using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using SharpDX;

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a custom mesh model entity for the track editor.
/// </summary>
public abstract class BaseModelEntity : MeshGeometryModel3D, INotifyPropertyChanged, IHideable
{
    [Browsable(true)]
    [PropertyOrder(0)]
    public virtual float X
    {
        get => BoundsWithTransform.Center.X;
        set
        {
            SetNewPosition(x: value);
        }
    }

    [Browsable(true)]
    [PropertyOrder(1)]
    public virtual float Y
    {
        get => BoundsWithTransform.Center.Y;
        set
        {
            SetNewPosition(y: value);
        }
    }

    [Browsable(true)]
    [PropertyOrder(2)]
    public virtual float Z
    {
        get => BoundsWithTransform.Center.Z;
        set
        {
            SetNewPosition(z: value);
        }
    }

    private void SetNewPosition(float x = float.NaN, float y = float.NaN, float z = float.NaN, float r = float.NaN)
    {
        x = -(Bounds.Center.X - (float.IsNaN(x) ? BoundsWithTransform.Center.X : x));
        y = -(Bounds.Center.Y - (float.IsNaN(y) ? BoundsWithTransform.Center.Y : y));
        z = -(Bounds.Center.Z - (float.IsNaN(z) ? BoundsWithTransform.Center.Z : z));

        // Todo: Implement angle

        var m = SharpDX.Matrix.Translation(new Vector3(x, y, z));
        Transform = new System.Windows.Media.Media3D.MatrixTransform3D(m.ToMatrix3D());
    }

    /// <summary>
    /// Whether the model is allowed to rotate on the X axis.
    /// </summary>
    public virtual bool CanRotateX { get; } = true;

    /// <summary>
    /// Whether the model is allowed to rotate on the Y axis.
    /// </summary>
    public virtual bool CanRotateY { get; } = true;

    /// <summary>
    /// Whether the model is allowed to rotate on the Z axis.
    /// </summary>
    public virtual bool CanRotateZ { get; } = true;

    /// <summary>
    /// Whether the model is allowed to translate on the X axis.
    /// </summary>
    public virtual bool CanTranslateX { get; } = true;

    /// <summary>
    /// Whether the model is allowed to translate on the Y axis.
    /// </summary>
    public virtual bool CanTranslateY { get; } = true;

    /// <summary>
    /// Whether the model is allowed to translate on the Z axis.
    /// </summary>
    public virtual bool CanTranslateZ { get; } = true;

    /// <summary>
    /// Whether the model is allowed to be scaled.
    /// </summary>
    public virtual bool CanScale { get; }

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

    public bool IsVisible { get; set; } = true;

    public abstract void OnMove();

    public void Hide()
    {
        Visibility = System.Windows.Visibility.Hidden;
        TreeViewItemColor = Brushes.Gray;

        IsVisible = false;
    }

    public void Show()
    {
        Visibility = System.Windows.Visibility.Visible;
        TreeViewItemColor = Brushes.White;

        IsVisible = true;
    }
}
