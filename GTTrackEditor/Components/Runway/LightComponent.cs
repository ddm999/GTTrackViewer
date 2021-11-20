using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;

using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;
using GTTrackEditor.Interfaces;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Components.Runway;

public class LightComponent : TrackComponentBase, IModelCollection
{
    public ObservableElement3DCollection Lights { get; set; } = new();
    public DiffuseMaterial LightMaterial { get; set; } = new();

    public RunwayFile RunwayData { get; set; }

    public const int DepthBias = -50;

    public LightComponent()
    {
        Name = "Light Definitions";
        LightMaterial.DiffuseColor = new(1.0f, 1.0f, 0.0f, 1.0f); // Yellow
    }

    public void Init(RunwayFile runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        Lights.Clear();
        for (int i = 0; i < RunwayData.LightDefs.Count; i++)
        {
            Vector3 actualPos = RunwayData.LightDefs[i].Position.ToSharpDXVector();
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(actualPos, 1);
            builder.ToMesh();

            LightModelEntity newLightModel = new()
            {
                Geometry = builder.ToMesh(),
                Material = LightMaterial,

                DepthBias = -15,
                LightData = RunwayData.LightDefs[i],
            };

            Lights.Add(newLightModel);
        }
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        foreach (Element3D i in Lights)
            (i as LightModelEntity).Hide();
        TreeViewItemColor = Brushes.Gray;
        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        foreach (Element3D i in Lights)
            (i as LightModelEntity).Show();
        TreeViewItemColor = Brushes.White;
        IsVisible = true;
    }


    public void AddNew()
    {
        var def = new RunwayLightDefinition();

        MeshBuilder builder = new MeshBuilder();
        builder.AddSphere(Vector3.Zero, 1);
        builder.ToMesh();

        RunwayData.LightDefs.Add(def);

        LightModelEntity newLightModel = new()
        {
            Geometry = builder.ToMesh(),
            Material = LightMaterial,

            DepthBias = -15,
            LightData = def,
        };

        Lights.Add(newLightModel);
    }

    public void Remove(Element3D entity)
    {
        LightModelEntity model = entity as LightModelEntity;
        RunwayData.LightDefs.Remove(model.LightData);

        Lights.Remove(entity);
    }
}

