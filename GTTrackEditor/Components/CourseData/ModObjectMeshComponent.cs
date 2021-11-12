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

namespace GTTrackEditor.Components.CourseData;

public class ModObjectMeshComponent : TrackComponentBase
{
    static public int Count = 0;

    public MeshGeometry3D Geometry;
    public ModObjectMeshModel Model;
    public int Index;
    public DiffuseMaterial ModObjectModelMaterial { get; set; } = new();

    public ModObjectMeshComponent()
    {
        ModObjectModelMaterial.DiffuseColor = new(1f, 1f, 1f, 1.0f);
    }
    public void Init(PACB courseData, string objPath)
    {
        Index = courseData.Models.Count+ ++Count;
        Name = $"Mod Object {Index}";

        ObjReader reader = new();
        List<Object3D> list = reader.Read(objPath);
        Geometry = list[0].Geometry as MeshGeometry3D;
    }

    public override void RenderComponent()
    {
        Model = new()
        {
            Geometry = Geometry,
            Material = ModObjectModelMaterial,
            MeshIndex = Index,
        };
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        Model.Hide();
        TreeViewItemColor = Brushes.Gray;
        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        Model.Show();
        TreeViewItemColor = Brushes.Black;
        IsVisible = true;
    }
}