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
using PDTools.Files.Models.ModelSet3.Meshes;

namespace GTTrackEditor.ModelEntities;

public class ModelSetMeshEntity : BaseModelEntity
{
    public MDL3Mesh Mesh { get; set; }
    public int MeshIndex { get; set; }

    [Browsable(true)]
    public string Flags => $"0x{Mesh.Flags:X4}";

    [Browsable(true)]
    public int MaterialIndex => Mesh.MaterialIndex;

    [Browsable(true)]
    public int FVFIndex => Mesh.FVFIndex;

    [Browsable(true)]
    public uint TriCount => Mesh.TriCount;

    [Browsable(true)]
    public uint VertCount => Mesh.VertexCount;

    public ModelSetMeshEntity(MDL3Mesh mesh, int meshIndex)
    {
        Mesh = mesh;
        EntityName = $"Mesh #{meshIndex}";
    }

    public override void OnManipulation()
    {

    }

    public override void UpdateValues()
    {

    }
}


