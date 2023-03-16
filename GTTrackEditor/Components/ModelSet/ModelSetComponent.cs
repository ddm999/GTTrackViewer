﻿using HelixToolkit.Wpf.SharpDX;
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
using System.ComponentModel;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Linq;
using PDTools.Files.Models.ModelSet3.Commands;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using SharpDX.Direct3D11;
using System.IO;

namespace GTTrackEditor.Components.ModelSet;

public class ModelSetComponent : TrackComponentBase
{
    public ModelSet3 ModelSet { get; set; }

    public ObservableCollection<ModelSetMeshComponent> ModelComponents { get; set; } = new();

    public DiffuseMaterial CourseMeshModelMaterial { get; set; } = new();

    public ModelSetComponent()
    {
        CourseMeshModelMaterial.DiffuseColor = new(1f, 1f, 1f, 0.5f);
    }

    // to test uv weirdness: check model [8], mesh [2]
    public void Init(ModelSet3 modelSet)
    {
        Name = "Model Set";
        ModelSet = modelSet;
        CourseMeshModelMaterial.DiffuseColor = new(1f, 1f, 1f, 0.5f);
    }


    public override void RenderComponent()
    {
        if (ModelSet.ShapeStream is null)
            return;

        for (int i = 0; i < ModelSet.Models.Count; i++)
        {
            var modelComponent = new ModelSetMeshComponent();

            ModelSet3Model model = ModelSet.Models[i];
            InterpretCommands(modelComponent, model.Commands);

            ModelComponents.Add(modelComponent);
        }
    }

    private void InterpretCommands(ModelSetMeshComponent modelEntity, List<ModelSetupCommand> commands)
    {
        foreach (var cmd in commands)
        {
            switch (cmd.Opcode)
            {
                case ModelSetupOpcode.Command_75_LoadMultipleMeshes2:
                    Command_75_LoadMultipleMeshes2(modelEntity, cmd as Command_75_LoadMultipleMeshes2);
                    break;

                default:
                    ;
                    break;
            }
        }
    }

    private void Command_75_LoadMultipleMeshes2(ModelSetMeshComponent modelEntity, Command_75_LoadMultipleMeshes2 cmd)
    {
        foreach (var meshIndex in cmd.MeshIndices)
        {
            LoadMesh(modelEntity, meshIndex);
        }
    }

    private void LoadMesh(ModelSetMeshComponent modelEntity, ushort meshId)
    {
        var mdl3Mesh = ModelSet.Meshes[meshId];

        // TODO: Optimize this
        var verts = ModelSet.GetVerticesOfMesh(meshId);
        var tris = ModelSet.GetTrisOfMesh(meshId);
        var uvs = ModelSet.GetUVsOfMesh(meshId);
        var norms = ModelSet.GetNormalsOfMesh(meshId);

        if (tris is null || tris.Count == 0)
            return; // Most likely tristrip - not supported for now

        Vector3Collection vertList = new Vector3Collection(verts.Length);
        Vector2Collection uvList = new Vector2Collection(uvs.Length);
        IntCollection col = new IntCollection(tris.Count);
        Vector3Collection normList = new Vector3Collection(norms.Length);

        var dMat = new DiffuseMaterial();
        var badDMat = false;

        var mat = ModelSet.Materials.Definitions[mdl3Mesh.MaterialIndex];

        var diffuseMapSampler = mat.ImageEntries.Find(e => e.Name == "diffuseMapSampler");
        if (diffuseMapSampler is not null && (diffuseMapSampler.TextureID & 0x8000) == 0)
        {
            try
            {

                PGLUCellTextureInfo textureInfo = ModelSet.Materials.TextureInfos[(int)diffuseMapSampler.TextureID];

                Texture text = ModelSet.TextureSet.Textures[(int)textureInfo.ImageId];

                long vramStartPos;
                if (ModelSet.ParentCourseData != null)
                    vramStartPos = ModelSet.ParentCourseData.Entries[1].DataStart;
                else
                    vramStartPos = 0;

                // XXX: Temporary fix to D3D device corruption exception
                (text as CellTexture).LastMipmapLevel = 1;

                byte[] data = ModelSet.TextureSet.GetExternalImageDataOfTexture(ModelSet.Stream, text, vramStartPos);
                dMat.DiffuseMap = TextureModel.Create(new System.IO.MemoryStream(data)); // TODO: Also optimize, cache textures locally (would be useful for reading too)

                /*
                using (var sw = new StreamWriter("test.obj"))
                {
                    sw.WriteLine("mtllib testmtl.mtl");
                    
                    foreach (var i in verts)
                    {
                        sw.WriteLine($"v {i.X} {i.Y} {i.Z}");
                    }

                    foreach (var i in uvs)
                    {
                        sw.WriteLine($"vt {i.X} {i.Y}");
                    }

                    sw.WriteLine("usemtl testmtl");

                    foreach (var i in tris)
                    {
                        sw.WriteLine($"f {i.A+1}/{i.A + 1} {i.B+1}/{i.B + 1} {i.C+1}/{i.C + 1}");
                    }


                }

                using (var sw = new StreamWriter("testmtl.mtl"))
                {
                    sw.WriteLine("newmtl testmtl");
                    sw.WriteLine("Ka 1.000000 1.000000 1.000000");
                    sw.WriteLine("Kd 1.000000 1.000000 1.000000");
                    sw.WriteLine("Ks 0.000000 0.000000 0.000000");
                    sw.WriteLine("Tr 1.000000");
                    sw.WriteLine("illum 1");
                    sw.WriteLine("Ns 0.000000");
                    sw.WriteLine("map_Kd test.dds");
                }

                File.WriteAllBytes("test.dds", data);
                */

                /*
                dMat.DiffuseMapSampler = new SharpDX.Direct3D11.SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    //Filter = Filter.Anisotropic,
                };*/

                TextureAddressMode Get(CELL_GCM_TEXTURE_WRAP cellWrap)
                {
                    switch (cellWrap)
                    {
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_WRAP_NONE:
                            return TextureAddressMode.Clamp;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_WRAP:
                            return TextureAddressMode.Wrap;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_MIRROR:
                            return TextureAddressMode.Mirror;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_CLAMP_TO_EDGE:
                            return TextureAddressMode.Clamp;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_BORDER:
                            return TextureAddressMode.Border;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_CLAMP:
                            return TextureAddressMode.Clamp;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_MIRROR_ONCE_CLAMP_TO_EDGE:
                            return TextureAddressMode.MirrorOnce;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_MIRROR_ONCE_BORDER:
                            return TextureAddressMode.MirrorOnce;
                        case CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_MIRROR_ONCE_CLAMP:
                            return TextureAddressMode.MirrorOnce;
                        default:
                            return TextureAddressMode.Clamp;
                    }
                }
            }
            catch
            {
                // reset material and make it grey
                dMat = new DiffuseMaterial();
                dMat.DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
                badDMat = true;
            }
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
            uvList.Add(new(0, 1));

        for (int j = 0; j < norms.Length; j++)
            normList.Add(new(norms[j].Item1, norms[j].Item2, norms[j].Item3));

        var geog = new MeshGeometry3D();
        geog.Positions = vertList;
        geog.Indices = col;
        geog.TextureCoordinates = uvList;
        geog.Normals = normList;

        ModelSetMeshEntity mesh = new ModelSetMeshEntity()
        {
            Geometry = geog,
            Material = dMat,

            IsHitTestVisible = true, // Important for perf purposes - we won't be manipulating it anyway
            //DepthBias = 300,
            IsThrowingShadow = false,
            RenderWireframe = badDMat,
            WireframeColor = System.Windows.Media.Color.FromRgb(16, 16, 16),
            IsDepthClipEnabled = false,
        };

        modelEntity.MeshEntities.Add(mesh);
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
        
    }

    public override void Show()
    {
        
    }
}

