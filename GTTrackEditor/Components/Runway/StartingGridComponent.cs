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

public class StartingGridComponent : TrackComponentBase, IModelCollection
{
    public ObservableElement3DCollection StartingGridPoints { get; set; } = new();
    public DiffuseMaterial StartingGridMaterial { get; set; } = new();

    public RunwayFile RunwayData { get; set; }

    public const int DepthBias = -50;

    public StartingGridComponent()
    {
        Name = "Starting Grid";
        StartingGridMaterial.DiffuseColor = new(0.0f, 1.0f, 0.0f, 1.0f);
    }

    public void Init(RunwayFile runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        StartingGridPoints.Clear();
        for (int i = 0; i < RunwayData.StartingGrid.Count; i++)
        {
            ObjReader reader = new();
            List<Object3D> list = reader.Read("Resources/Models/Grid.obj");
            MeshGeometry3D gridGeometry = list[0].Geometry as MeshGeometry3D;

            Vector3 actualPos = RunwayData.StartingGrid[i].Position.ToSharpDXVector();
            for (int j = 0; j < gridGeometry.Positions.Count; ++j)
                gridGeometry.Positions[j] += actualPos;

            gridGeometry.UpdateBounds();

            float angle = MathUtils.Atan2RadToDeg(RunwayData.StartingGrid[i].Position.AngleRad);
            StartingGridModelEntity newGridModel = new()
            {
                Geometry = gridGeometry,
                Material = StartingGridMaterial,
                DepthBias = DepthBias,

                StartingIndex = i,
                StartingGridPoint = RunwayData.StartingGrid[i],

                YawAngle = angle,
            };

            StartingGridPoints.Add(newGridModel);
        }
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        foreach (Element3D i in StartingGridPoints)
            (i as StartingGridModelEntity).Hide();
        TreeViewItemColor = Brushes.Gray;
        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        foreach (Element3D i in StartingGridPoints)
            (i as StartingGridModelEntity).Show();
        TreeViewItemColor = Brushes.White;
        IsVisible = true;
    }

    public void AddNew()
    {
        ObjReader reader = new();
        List<Object3D> list = reader.Read("Resources/Models/Grid.obj");
        MeshGeometry3D gridGeometry = list[0].Geometry as MeshGeometry3D;

        var newStartGridPos = new RunwayStartingGridPosition();
        RunwayData.StartingGrid.Add(newStartGridPos);

        StartingGridPoints.Add(new StartingGridModelEntity()
        {
            Geometry = gridGeometry,
            Material = StartingGridMaterial,
            DepthBias = DepthBias,

            StartingGridPoint = newStartGridPos,
        });
    }

    public void Remove(Element3D entity)
    {
        StartingGridModelEntity model = entity as StartingGridModelEntity;
        RunwayData.StartingGrid.Remove(model.StartingGridPoint);

        StartingGridPoints.Remove(entity);
    }
}

