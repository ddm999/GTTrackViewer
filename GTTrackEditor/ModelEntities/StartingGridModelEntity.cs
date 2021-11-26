using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

using GTTrackEditor.Utils;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a starting grid model entity.
/// </summary>
public class StartingGridModelEntity : BaseModelEntity
{
    public RunwayStartingGridPosition StartingGridPoint { get; set; }

    /// <summary>
    /// Position on the grid (0 indexed).
    /// </summary>
    [Browsable(true)]
    public int StartingIndex { get; set; }

    public override bool PitchRotationAllowed => false;
    public override bool RollRotationAllowed => false;

    public override void OnManipulation()
    {
        UpdateValues();
    }

    public override void UpdateValues()
    {
        MatrixTransform3D m = Transform as MatrixTransform3D;
        float rad = m.GetMatrixYawAngleRad();

        StartingGridPoint.Position = new Vec3R(X, Y, Z, rad);
    }
}


