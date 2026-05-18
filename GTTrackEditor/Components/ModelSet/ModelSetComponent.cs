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

using PDTools.Files.Courses.PS3;
using PDTools.Files.Models.PS3.ModelSet3;
using PDTools.Files.Models.PS3.ModelSet3.Materials;
using PDTools.Files.Models.PS3.ModelSet3.Models;
using PDTools.Files.Models.PS3.ModelSet3.Shapes;
using PDTools.Files.Textures;
using PDTools.Files.Textures;
using PDTools.Files.Textures.PS3;
using PDTools.Files.Models.PS3.PGLCommands;
using System.ComponentModel;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using SharpDX.Direct3D11;
using System.IO;

namespace GTTrackEditor.Components.ModelSet;

public class ModelSetComponent : TrackComponentBase
{
    public ModelSet3 ModelSet { get; set; }

    public ObservableCollection<ModelSetModelComponent> ModelComponents { get; set; } = new();
    public ModelSetModelsComponent ModelsBlock { get; set; }
    public ModelSetMaterialsComponent MaterialsBlock { get; set; }

    /// <summary>
    /// Combined tree children: model components followed by the materials block.
    /// </summary>
    public ObservableCollection<object> TreeChildren { get; set; } = new();

    /// <summary>
    /// Viewport group for each model
    /// </summary>
    public List<GroupModel3D> RenderingGroups { get; set; } = new();

    public ModelSetComponent()
    {

    }

    // to test uv weirdness: check model [8], mesh [2]
    public void Init(ModelSet3 modelSet)
    {
        Name = "Model Set";
        ModelSet = modelSet;
    }


    private record MeshComputeResult(
        MDL3Shape Shape,
        ushort MeshId,
        MeshGeometry3D Geometry,
        byte[] TextureData,                       // null = no texture
        SamplerStateDescription? Sampler,
        bool RenderWireframe,
        MDL3Material Material);

    private void BuildTreeBlocks()
    {
        ModelsBlock = new ModelSetModelsComponent { Models = ModelComponents };

        MaterialsBlock = new ModelSetMaterialsComponent();
        for (int i = 0; i < ModelSet.Materials.Definitions.Count; i++)
        {
            var def = ModelSet.Materials.Definitions[i];
            var entry = new ModelSetMaterialEntry { Index = i, Material = def };

            foreach (var key in def.ImageEntries)
            {
                PGLUCellTextureInfo texInfo = null;
                if ((key.TextureID & 0x8000) == 0 && (int)key.TextureID < ModelSet.Materials.TextureInfos.Count)
                    texInfo = ModelSet.Materials.TextureInfos[(int)key.TextureID];
                entry.ImageEntries.Add(new ResolvedTextureEntry { SamplerName = key.Name, TextureInfo = texInfo });
            }

            MaterialsBlock.Materials.Add(entry);
        }
    }

    private void PopulateTreeChildren()
    {
        TreeChildren.Add(ModelsBlock);
        TreeChildren.Add(MaterialsBlock);
    }

    public override void RenderComponent()
    {
        for (int i = 0; i < ModelSet.Models.Count; i++)
        {
            ModelSet3Model model = ModelSet.Models[i];
            var modelComponent = new ModelSetModelComponent(model, i);

            InterpretCommands(modelComponent, model.Commands, meshId => LoadMesh(modelComponent, meshId));
            ModelComponents.Add(modelComponent);
        }

        BuildTreeBlocks();
        PopulateTreeChildren();
    }

    public override async Task RenderComponentAsync()
    {
        // Pass 1 (UI thread): walk commands to build the load plan
        var loadPlan = new List<(ModelSetModelComponent Comp, ushort MeshId)>();
        for (int i = 0; i < ModelSet.Models.Count; i++)
        {
            var model = ModelSet.Models[i];
            var comp = new ModelSetModelComponent(model, i);
            InterpretCommands(comp, model.Commands, meshId => loadPlan.Add((comp, meshId)));
            ModelComponents.Add(comp);
        }

        // Pass 2 (background thread): compute geometry + texture data
        var results = await Task.Run(() =>
            loadPlan.Select(p => TryComputeMeshData(p.MeshId)).ToList()
        );

        // Pass 3 (UI thread): create FrameworkElement entities
        for (int i = 0; i < loadPlan.Count; i++)
        {
            var data = results[i];
            if (data is null) continue;

            var dMat = new DiffuseMaterial();
            if (data.TextureData is not null)
            {
                dMat.DiffuseMap = TextureModel.Create(new MemoryStream(data.TextureData));
                dMat.DiffuseMapSampler = data.Sampler!.Value;
            }

            var entity = new ModelSetMeshEntity(data.Shape, data.MeshId)
            {
                Geometry = data.Geometry,
                Material = dMat,
                IsHitTestVisible = true,
                CullMode = CullMode.Back,
                IsThrowingShadow = false,
                RenderWireframe = data.RenderWireframe,
                WireframeColor = System.Windows.Media.Color.FromRgb(16, 16, 16),
                IsDepthClipEnabled = false,
                MaterialDef = data.Material,
            };
            loadPlan[i].Comp.MeshEntities.Add(entity);
        }

        BuildTreeBlocks();
        PopulateTreeChildren();
    }

    private void InterpretCommands(ModelSetModelComponent modelEntity, List<ModelSetupCommand> commands, Action<ushort> onMeshId)
    {
        if (commands.Count == 0)
            return;

        int instPtr = 0;
        bool advanceOne = true;

        while (true)
        {
            advanceOne = true;

            ModelSetupCommand cmd = commands[instPtr];
            if (cmd.Opcode == ModelSetupOpcode.Command_0_End)
                break;

            switch (cmd.Opcode)
            {
                case ModelSetupOpcode.Command_5_Switch:
                    var sw = cmd as Command_5_Switch;
                    instPtr = sw.BranchJumpIndices[0];
                    advanceOne = false;
                    break;

                case ModelSetupOpcode.Command_9_JumpToByte:
                    var jmpByte = cmd as Command_JumpByte;
                    instPtr = jmpByte.JumpToIndex;
                    advanceOne = false;
                    break;

                case ModelSetupOpcode.Command_10_JumpToShort:
                    var jmpShort = cmd as Command_JumpShort;
                    instPtr = jmpShort.JumpToIndex;
                    advanceOne = false;
                    break;

                case ModelSetupOpcode.Command_59_LoadMesh2_Byte:
                    onMeshId((cmd as Command_CallShape2Byte).MeshID);
                    break;

                case ModelSetupOpcode.Command_60_LoadMesh2_UShort:
                    onMeshId((ushort)(cmd as Command_CallShape2UShort).Unk);
                    break;

                case ModelSetupOpcode.Command_74_LoadMultipleMeshes:
                    foreach (var idx in (cmd as Command_74_LoadMultipleMeshes).MeshIndices)
                        onMeshId(idx);
                    break;

                case ModelSetupOpcode.Command_75_LoadMultipleMeshes2:
                    foreach (var idx in (cmd as Command_75_LoadMultipleMeshes2).MeshIndices)
                        onMeshId(idx);
                    break;
            }

            if (advanceOne)
                instPtr++;
        }
    }

    private void LoadMesh(ModelSetModelComponent modelEntity, ushort meshId)
    {
        var data = TryComputeMeshData(meshId);
        if (data is null) return;

        var dMat = new DiffuseMaterial();
        if (data.TextureData is not null)
        {
            dMat.DiffuseMap = TextureModel.Create(new MemoryStream(data.TextureData));
            dMat.DiffuseMapSampler = data.Sampler!.Value;
        }

        ModelSetMeshEntity mesh = new ModelSetMeshEntity(data.Shape, data.MeshId)
        {
            Geometry = data.Geometry,
            Material = dMat,
            IsHitTestVisible = true,
            CullMode = CullMode.Back,
            IsThrowingShadow = false,
            RenderWireframe = data.RenderWireframe,
            WireframeColor = System.Windows.Media.Color.FromRgb(16, 16, 16),
            IsDepthClipEnabled = false,
            MaterialDef = data.Material,
        };
        modelEntity.MeshEntities.Add(mesh);
    }

    private MeshComputeResult TryComputeMeshData(ushort meshId)
    {
        var mdl3Mesh = ModelSet.Shapes[meshId];

        var verts = ModelSet.GetVerticesOfShape(meshId);
        var tris = ModelSet.GetTrisOfMesh(meshId);
        var uvs = ModelSet.GetUVsOfMesh(meshId);
        var norms = ModelSet.GetNormalsOfShape(meshId);

        if (tris is null || tris.Count == 0)
            return null; // Most likely tristrip - not supported for now

        Vector3Collection vertList = new Vector3Collection(verts.Length);
        Vector2Collection uvList = new Vector2Collection(uvs.Length);
        IntCollection col = new IntCollection(tris.Count * 3);
        Vector3Collection normList = new Vector3Collection(norms.Length);

        byte[] textureData = null;
        SamplerStateDescription? sampler = null;

        var mat = ModelSet.Materials.Definitions[mdl3Mesh.MaterialIndex];

        var diffuseMapSampler = mat.ImageEntries.Find(e => e.Name == "diffuseMapSampler");
        if (diffuseMapSampler is not null && (diffuseMapSampler.TextureID & 0x8000) == 0)
        {
            PGLUCellTextureInfo textureInfo = ModelSet.Materials.TextureInfos[(int)diffuseMapSampler.TextureID];

            // BufferId is not set during Read() so BufferInfo defaults to Buffers[0] for all textures;
            // use ImageId (the value stored in the file) to fetch the correct buffer
            var bufferInfo = (CellTextureBuffer)ModelSet.TextureSet.Buffers[(int)textureInfo.ImageId];
            textureInfo.BufferInfo = bufferInfo;

            if (bufferInfo.ImageOffset != 0 && bufferInfo.ImageSize != 0)
            {
                long vramStartPos = ModelSet.ParentCourseData != null
                    ? ModelSet.ParentCourseData.Entries[1].DataStart
                    : 0;

                // XXX: Clamp to 1 mip level - raw image data in the stream only covers the base mip,
                // so the DDS header must not advertise more levels than are present
                textureInfo.MipmapLevelLast = 1;

                textureData = ModelSet.TextureSet.GetExternalImageDataOfTexture(ModelSet.Stream, textureInfo, vramStartPos);
                sampler = new SharpDX.Direct3D11.SamplerStateDescription()
                {
                    AddressU = ConvertTextureWrapMode(textureInfo.WrapS),
                    AddressV = ConvertTextureWrapMode(textureInfo.WrapT),
                    AddressW = ConvertTextureWrapMode(textureInfo.WrapR),
                };
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
            uvList.Add(new(uvs[j].X, uvs[j].Y));

        for (int j = 0; j < norms.Length; j++)
            normList.Add(new(norms[j].Item1, norms[j].Item2, norms[j].Item3));

        var geog = new MeshGeometry3D
        {
            Positions = vertList,
            Indices = col,
            TextureCoordinates = uvList,
            Normals = normList,
        };

        return new MeshComputeResult(mdl3Mesh, meshId, geog, textureData, sampler, false, mat);
    }

    private int CountTotalRenderableTrisForModel(ModelSet3 mdl)
    {
        int total = 0;
        for (ushort i = 0; i < mdl.Shapes.Count; i++)
        {
            // Create mesh
            MDL3Shape mesh = mdl.Shapes[i];
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

    static TextureAddressMode ConvertTextureWrapMode(CELL_GCM_TEXTURE_WRAP cellWrap)
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

