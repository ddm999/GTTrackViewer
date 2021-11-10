using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;
using System.Collections.ObjectModel;

namespace GTTrackEditor.Components.Runway;

public class StartingGridComponent : TrackComponentBase
{
    public ObservableElement3DCollection StartingGridPoints { get; set; } = new();
    public DiffuseMaterial StartingGridMaterial { get; set; } = new();

    public RNW5 RunwayData { get; set; }

    public StartingGridComponent()
    {
        Name = "Starting Grid";
        StartingGridMaterial.DiffuseColor = new(0.0f, 1.0f, 0.0f, 1.0f);
    }

    public void Init(RNW5 runwayData)
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

            Vector3 actualPos = RunwayData.StartingGrid[i].ToVector3();
            for (int j = 0; j < gridGeometry.Positions.Count; ++j)
            {
                gridGeometry.Positions[j] += actualPos;
            }
            gridGeometry.UpdateBounds();

            StartingGridModelEntity newGridModel = new()
            {
                Geometry = gridGeometry,
                Material = StartingGridMaterial,
                StartingIndex = i,
            };

            Point3D center = new(RunwayData.StartingGrid[i].X, RunwayData.StartingGrid[i].Y, RunwayData.StartingGrid[i].Z);

            // Apply angle
            float angle = MathUtils.PDRadToDeg(RunwayData.StartingGrid[i].AngleRad);
            ModelUtils.Rotate(newGridModel, center, angle);

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
        TreeViewItemColor = Brushes.Black;
        IsVisible = true;
    }
}

