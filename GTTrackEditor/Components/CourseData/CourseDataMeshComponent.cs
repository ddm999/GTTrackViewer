using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

namespace GTTrackEditor.Components.CourseData;

public class CourseDataMeshComponent : TrackComponentBase
{
    public PACB CourseData { get; set; }

    public ObservableElement3DCollection Meshes { get; set; } = new();
    public DiffuseMaterial CourseMeshModelMaterial { get; set; } = new();

    public CourseDataMeshComponent()
    {
        CourseMeshModelMaterial.DiffuseColor = new(1f, 1f, 1f, 1f);
    }

    public void Init(PACB courseData)
    {
        Name = "Track Meshes";
        CourseData = courseData;
    }


    public override void RenderComponent()
    {
        List<Vector3> trilist = new List<Vector3>(CountTotalRenderableTrisForModel(CourseData.Models[0])); // Wisely pre-allocate
        for (int m = 0; m < CourseData.Models.Count; m++)
        {
            MDL3 mdl = CourseData.Models[(byte)m];

            int triCountForModel = CountTotalRenderableTrisForModel(mdl);
            trilist.Clear();
            trilist.EnsureCapacity(triCountForModel);

            for (ushort i = 0; i < mdl.MeshCount; i++)
            {
                // Create mesh
                MDL3Mesh mesh = mdl.Meshes[i];
                if (mesh.Tristrip)
                    continue;

                for (int j = 0; j < mesh.Tris.Count; j++)
                {
                    trilist.Add(mesh.Verts[mesh.Tris[j].A]);
                    trilist.Add(mesh.Verts[mesh.Tris[j].B]);
                    trilist.Add(mesh.Verts[mesh.Tris[j].C]);
                }
            }

            MeshBuilder meshBuilder = new(false, false);
            meshBuilder.AddTriangles(trilist);
            var mesh3d = meshBuilder.ToMesh();
            mesh3d.CalculateNormals();

            TrackModelBase model = new TrackModelBase()
            {
                Geometry = mesh3d,
                Material = CourseMeshModelMaterial,

                IsHitTestVisible = false, // Important for perf purposes - we won't be manipulating it anyway
            };

            Meshes.Add(model);
        }
    }

    private int CountTotalRenderableTrisForModel(MDL3 mdl)
    {
        int total = 0;
        for (ushort i = 0; i < mdl.MeshCount; i++)
        {
            // Create mesh
            MDL3Mesh mesh = mdl.Meshes[i];
            if (mesh.Tristrip)
                continue;

            total += mesh.Tris.Count * 3;
        }

        return total;
    }

    public override void Hide()
    {
        throw new NotImplementedException();
    }

    public override void Show()
    {
        throw new NotImplementedException();
    }
}

