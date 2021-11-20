using System.Numerics;

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a gadget model entity.
/// </summary>
public class LightModelEntity : BaseModelEntity
{
    public RunwayLightDefinition LightData { get; set; }

    public override void OnManipulation()
    {
        UpdateValues();
    }

    public override void UpdateValues()
    {
        LightData.Position = new Vector3(X, Y, Z);
    }
}

