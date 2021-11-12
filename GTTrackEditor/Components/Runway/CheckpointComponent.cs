using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

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

namespace GTTrackEditor.Components.Runway;

public class CheckpointComponent : TrackComponentBase
{
    public MeshGeometry3D CheckpointModel { get; set; } = new();
    public DiffuseMaterial CheckpointMaterial { get; set; } = new();

    public RNW5 RunwayData { get; set; }

    public CheckpointComponent()
    {
        Name = "Checkpoints";
        CheckpointMaterial.DiffuseColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
    }

    public void Init(RNW5 runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        List<RNW5Checkpoint4> chks = RunwayData.Checkpoints;

        MeshBuilder meshBuilder = new(false, false);
        for (int i = 0; i < chks.Count; i++)
        {
            meshBuilder.AddQuad(
                chks[i].Left,
                chks[i].Middle,
                chks[i].Middle + new Vector3(0.0f, 3f, 0.0f),
                chks[i].Left + new Vector3(0.0f, 3f, 0.0f)
                );
            meshBuilder.AddQuad(
                chks[i].Middle,
                chks[i].Right,
                chks[i].Right + new Vector3(0.0f, 3f, 0.0f),
                chks[i].Middle + new Vector3(0.0f, 3f, 0.0f)
                );
        }

        MeshGeometry3D checks = meshBuilder.ToMesh();
        checks.Normals = checks.CalculateNormals();
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

