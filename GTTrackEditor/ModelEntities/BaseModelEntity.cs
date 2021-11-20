using HelixToolkit.Wpf.SharpDX;

using System;
using System.ComponentModel;
using System.Windows.Media;

using GTTrackEditor.Interfaces;
using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using GTTrackEditor.Utils;

using SharpDX;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a custom mesh model entity for the track editor.
/// </summary>
public abstract class BaseModelEntity : MeshGeometryModel3D, INotifyPropertyChanged, IHideable
{
    /// <summary>
    /// Whether the model is allowed to rotate on pitch.
    /// </summary>
    public virtual bool PitchRotationAllowed { get; } = true;

    /// <summary>
    /// Whether the model is allowed to rotate on yaw.
    /// </summary>
    public virtual bool YawRotationAllowed { get; } = true;

    /// <summary>
    /// Whether the model is allowed to rotate on roll.
    /// </summary>
    public virtual bool RollRotationAllowed { get; } = true;

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

    /// <summary>
    /// Current X position of this model.
    /// </summary>
    /// <remarks>Note: Updating this property will update the transform.</remarks>
    [Browsable(true)]
    [PropertyOrder(0)]
    public float X
    {
        get => BoundsWithTransform.Center.X;
        set
        {
            UpdateTransform(x: value);
        }
    }

    /// <summary>
    /// Current Y position of this model.
    /// </summary>
    /// <remarks>Note: Updating this property will update the transform.</remarks>
    [Browsable(true)]
    [PropertyOrder(1)]
    public float Y
    {
        get => BoundsWithTransform.Center.Y;
        set
        {
            UpdateTransform(y: value);
        }
    }

    /// <summary>
    /// Current Z position of this model.
    /// </summary>
    /// <remarks>Note: Updating this property will update the transform.</remarks>
    [Browsable(true)]
    [PropertyOrder(2)]
    public float Z
    {
        get => BoundsWithTransform.Center.Z;
        set
        {
            UpdateTransform(z: value);
        }
    }

    protected float _yawAngle;

    /// <summary>
    /// Yaw angle of the entity from 0° to 360°, if allowed to rotate. 
    /// </summary>
    /// <remarks>Note: Updating this property will update the transform.</remarks>
    [Browsable(true)]
    public float YawAngle
    {
        get => _yawAngle;
        set
        {
            if (!YawRotationAllowed)
                throw new NotSupportedException("Entity can not be rotated axis.");

            if (value > 360 || value < 0)
                return;

            _yawAngle = value;
            UpdateTransform();
            UpdateValues();
        }
    }

    /// <summary>
    /// Updates the model's transform.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    protected void UpdateTransform(float x = float.NaN, float y = float.NaN, float z = float.NaN)
    {
        x = -(Bounds.Center.X - (float.IsNaN(x) ? BoundsWithTransform.Center.X : x));
        y = -(Bounds.Center.Y - (float.IsNaN(y) ? BoundsWithTransform.Center.Y : y));
        z = -(Bounds.Center.Z - (float.IsNaN(z) ? BoundsWithTransform.Center.Z : z));
        var sMatrix = SharpDX.Matrix.Translation(new Vector3(x, y, z));

        Vector3 newPos = new(Bounds.Center.X + x, Bounds.Center.Y + x, Bounds.Center.Z + z);

        var m = sMatrix.ToMatrix3D();

        // Apply yaw
        if (YawRotationAllowed)
        {
            Vector3D axis = new(0, 1, 0);
            m.RotateAt(new Quaternion(axis, YawAngle), new Point3D(newPos.X, newPos.Y, newPos.Z));

            float rad = (float)Math.Atan2(m.M31, m.M11);
            _yawAngle = MathUtils.Atan2RadToDeg(rad);
        }

        if (PitchRotationAllowed)
        {
            // TODO
        }

        if (RollRotationAllowed)
        {
            // TODO
        }

        Transform = new System.Windows.Media.Media3D.MatrixTransform3D(m);
    }

    public abstract void UpdateValues();

    /// <summary>
    /// Fired when the entity is being manipulated.
    /// </summary>
    public abstract void OnManipulation();

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
