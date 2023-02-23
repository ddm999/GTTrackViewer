using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;

using PDTools.Files.Courses;
using PDTools.Files.Courses.CourseData;
using PDTools.Files.Models;
using PDTools.Files.Models.ModelSet3;
using PDTools.Files.Textures;
using PDTools.Files.Models.ModelSet3.Meshes;

namespace GTTrackEditor.Components.CourseData;

public class CourseDataMeshComponent : TrackComponentBase
{
    public CourseDataFile CourseData { get; set; }

    public ObservableElement3DCollection Meshes { get; set; } = new();
    public DiffuseMaterial CourseMeshModelMaterial { get; set; } = new();

    public CourseDataMeshComponent()
    {
        CourseMeshModelMaterial.DiffuseColor = new(1f, 1f, 1f, 0.5f);
    }

    public void Init(CourseDataFile courseData)
    {
        Name = "Track Meshes";
        CourseData = courseData;
    }


    public override void RenderComponent()
    {
        for (int m = 0; m < 1; m++)
        {
            ModelSet3 mdl = CourseData.MainModel;

            for (short i = 0; i < mdl.Meshes.Count; i++)
            {
                var mesh = mdl.Meshes[i];

                // TODO: Optimize this
                var verts = mdl.GetVerticesOfMesh(i);
                var tris = mdl.GetTrisOfMesh((ushort)i);
                var uvs = mdl.GetUVsOfMesh((ushort)i);

                if (tris is null || tris.Count == 0)
                    continue; // Most likely tristrip - not supported for now

                Vector3Collection vertList = new Vector3Collection(verts.Length);
                Vector2Collection uvList = new Vector2Collection(uvs.Length);
                IntCollection col = new IntCollection(tris.Count);

                var dMat = new DiffuseMaterial();
                
                var mat = mdl.Materials.Definitions[mesh.MaterialIndex];

                var diffuseMapSampler = mat.ImageEntries.Find(e => e.Name == "diffuseMapSampler");
                if (diffuseMapSampler is not null)
                {
                    uint imageId = mdl.Materials.TextureInfos[(int)diffuseMapSampler.TextureID].ImageId;

                    Texture text = mdl.TextureSet.Textures[(int)imageId];

                    long vramStartPos;
                    if (mdl.ParentCourseData != null)
                        vramStartPos = mdl.ParentCourseData.Entries[1].DataStart;
                    else
                        vramStartPos = 0;

                    // XXX: Temporary fix to D3D device corruption exception
                    (text as CellTexture).LastMipmapLevel = 1;

                    byte[] data = mdl.TextureSet.GetExternalImageDataOfTexture(mdl.Stream, text, vramStartPos);
                    dMat.DiffuseMap = TextureModel.Create(new System.IO.MemoryStream(data)); // TODO: Also optimize, cache textures locally (would be useful for reading too)
                }

                for (int j = 0; j < verts.Length; j++)
                    vertList.Add(verts[j].ToSharpDXVector());

                for (int j = 0; j < tris.Count; j++)
                {
                    col.Add(tris[j].A);
                    col.Add(tris[j].B);
                    col.Add(tris[j].C);
                }

                for (int j = 0; j < uvs.Length; j++)
                    uvList.Add(new(uvs[j].X, uvs[j].Y));

                var geog = new MeshGeometry3D();
                geog.Positions = vertList;
                geog.Indices = col;
                geog.TextureCoordinates = uvList;
                //geog.CalculateNormals();

                TrackModelBase model = new TrackModelBase()
                {
                    Geometry = geog,
                    Material = dMat,

                    IsHitTestVisible = false, // Important for perf purposes - we won't be manipulating it anyway
                    CullMode = SharpDX.Direct3D11.CullMode.Back,
                    IsThrowingShadow = false,
                };

                Meshes.Add(model);
            }
        }
    }

    private int CountTotalRenderableTrisForModel(ModelSet3 mdl)
    {
        int total = 0;
        for (ushort i = 0; i < mdl.Meshes.Count; i++)
        {
            // Create mesh
            MDL3Mesh mesh = mdl.Meshes[i];
            if (mesh.Tristrip)
                continue;

            //total += mesh.Tris.Count * 3;
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

