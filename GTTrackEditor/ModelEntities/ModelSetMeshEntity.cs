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
using PDTools.Files.Courses.Minimap;

namespace GTTrackEditor.ModelEntities;

public class ModelSetMeshEntity : BaseModelEntity
{
    public ModelSetMeshEntity()
    {
        Name = "Mesh";
    }

    public override void OnManipulation()
    {

    }

    public override void UpdateValues()
    {

    }
}


