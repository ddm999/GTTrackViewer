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

public class OldGadgetComponent : TrackComponentBase, IModelCollection
{
    public ObservableCollection<Element3D> Gadgets { get; set; } = new();
    public DiffuseMaterial GadgetMaterial { get; set; } = new();

    public RunwayFile RunwayData { get; set; }

    public const int DepthBias = -50;

    public OldGadgetComponent()
    {
        Name = "Old Gadgets";
        GadgetMaterial.DiffuseColor = new(0.0f, 0.0f, 1.0f, 1.0f);
    }

    public void Init(RunwayFile runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        Gadgets.Clear();
        for (int i = 0; i < RunwayData.Gadgets.Count; i++)
        {
            Vector3 actualPos = RunwayData.Gadgets[i].Position.ToSharpDXVector();
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(actualPos, 1);
            builder.ToMesh();

            OldGadgetModelEntity newGadgetModel = new()
            {
                Geometry = builder.ToMesh(),
                Material = GadgetMaterial,

                DepthBias = -15,
                GadgetData = RunwayData.Gadgets[i],
            };

            Gadgets.Add(newGadgetModel);
        }
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        foreach (Element3D i in Gadgets)
            (i as OldGadgetModelEntity).Hide();
        TreeViewItemColor = Brushes.Gray;
        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        foreach (Element3D i in Gadgets)
            (i as OldGadgetModelEntity).Show();
        TreeViewItemColor = Brushes.White;
        IsVisible = true;
    }

    public void AddNew()
    {
        var gData = new RunwayGadgetOld();

        MeshBuilder builder = new MeshBuilder();
        builder.AddSphere(Vector3.Zero, 1);
        builder.ToMesh();

        RunwayData.Gadgets.Add(gData);

        OldGadgetModelEntity newGadgetModel = new()
        {
            Geometry = builder.ToMesh(),
            Material = GadgetMaterial,

            DepthBias = -15,
            GadgetData = gData,
        };

        Gadgets.Add(newGadgetModel);
    }

    public void Remove(Element3D entity)
    {
        OldGadgetModelEntity model = entity as OldGadgetModelEntity;
        RunwayData.Gadgets.Remove(model.GadgetData);

        Gadgets.Remove(entity);
    }
}

