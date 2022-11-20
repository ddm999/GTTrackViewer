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

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Components.Runway;

public class CheckpointComponent : TrackComponentBase
{
    public MeshGeometry3D CheckpointModel { get; set; } = new();
    public DiffuseMaterial CheckpointMaterial { get; set; } = new();

    public RunwayFile RunwayData { get; set; }

    public CheckpointComponent()
    {
        Name = "Checkpoints";
        CheckpointMaterial.DiffuseColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
    }

    public void Init(RunwayFile runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        MeshBuilder meshBuilder = new(false, false);
        for (int n = 0; n < RunwayData.Checkpoints.Count; n++)
        {
            List<Vector3> vec3s = new();


            RunwayData.Checkpoints[n].Left = new System.Numerics.Vector3(RunwayData.Checkpoints[n].Left.X, RunwayData.Checkpoints[n].Left.Y + 12f, RunwayData.Checkpoints[n].Left.Z);
            RunwayData.Checkpoints[n].Middle = new System.Numerics.Vector3(RunwayData.Checkpoints[n].Middle.X, RunwayData.Checkpoints[n].Middle.Y + 12f, RunwayData.Checkpoints[n].Middle.Z);
            RunwayData.Checkpoints[n].Right = new System.Numerics.Vector3(RunwayData.Checkpoints[n].Right.X, RunwayData.Checkpoints[n].Right.Y + 12f, RunwayData.Checkpoints[n].Right.Z);

            vec3s.Add(RunwayData.Checkpoints[n].Left.ToSharpDXVector());
            vec3s.Add(RunwayData.Checkpoints[n].Middle.ToSharpDXVector());
            vec3s.Add(RunwayData.Checkpoints[n].Right.ToSharpDXVector());
            meshBuilder.AddTube(vec3s, 1f, 18, false, true, true);
        }

        MeshGeometry3D checks = meshBuilder.ToMesh();
        //checks.Normals = checks.CalculateNormals();
        checks.AssignTo(CheckpointModel);
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        CheckpointModel.ClearAllGeometryData();
        CheckpointModel.UpdateVertices();
        TreeViewItemColor = Brushes.Gray;

        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        RenderComponent();

        TreeViewItemColor = Brushes.White;
        IsVisible = true;
    }
}

