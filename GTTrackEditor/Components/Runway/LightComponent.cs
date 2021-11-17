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

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Components.Runway;

public class LightComponent : TrackComponentBase
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
        for (int i = 0; i < RunwayData.LightSets.Count; i++)
        {
            Vector3 actualPos = RunwayData.LightSets[i].Position.ToSharpDXVector();
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(actualPos, 1);
            builder.ToMesh();

            LightModelEntity newLightModel = new()
            {
                Geometry = builder.ToMesh(),
                Material = LightMaterial,

                DepthBias = -15,
            };

            /*
            Point3D center = new(actualPos.X, actualPos.Y, actualPos.Z);

            // Apply angle
            float angle = MathUtils.PDRadToDeg(RunwayData.StartingGrid[i].AngleRad);
            ModelUtils.Rotate(newGridModel, center, angle);

            StartingGridPoints.Add(newGridModel);
            */

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

    }
}

