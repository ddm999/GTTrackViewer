using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
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
public class CourseMapFaceModelEntity : BaseModelEntity
{
    public override void OnManipulation()
    {

    }

    public override void UpdateValues()
    {

    }
}


