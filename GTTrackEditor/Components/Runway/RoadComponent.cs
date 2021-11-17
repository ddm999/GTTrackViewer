using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Numerics;

using System.Collections.Generic;
using System.Windows.Media;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;

using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Components.Runway;

public class RoadComponent : TrackComponentBase
{
    public MeshGeometry3D RoadModel { get; set; } = new();
    public static VertColorMaterial RoadMaterial { get; set; } = new();

    public RunwayFile RunwayData { get; set; }

    public static List<Color4> SurfaceTypeColors { get; set; } = new();
    static RoadComponent()
    {
        SurfaceTypeColors.Add(new(0.5f, 0.5f, 0.5f, 1.0f)); // TARMAC
        SurfaceTypeColors.Add(new(0.8f, 0.2f, 0.2f, 1.0f)); // GUIDE
        SurfaceTypeColors.Add(new(0.2f, 0.8f, 0.2f, 1.0f)); // GREEN
        SurfaceTypeColors.Add(new(0.7f, 0.6f, 0.5f, 1.0f)); // GRAVEL
        SurfaceTypeColors.Add(new(0.4f, 0.3f, 0.2f, 1.0f)); // DIRT
        SurfaceTypeColors.Add(new(0.4f, 0.5f, 0.8f, 1.0f)); // WATER
        SurfaceTypeColors.Add(new(0.7f, 0.7f, 0.7f, 1.0f)); // STONE
        SurfaceTypeColors.Add(new(0.4f, 0.25f, 0.2f, 1.0f)); // WOOD
        SurfaceTypeColors.Add(new(0.8f, 0.75f, 0.65f, 1.0f)); // PAVE
        SurfaceTypeColors.Add(new(0.2f, 0.5f, 0.2f, 1.0f)); // GUIDE1
        SurfaceTypeColors.Add(new(0.2f, 0.7f, 0.3f, 1.0f)); // GUIDE2
        SurfaceTypeColors.Add(new(0.3f, 0.7f, 0.2f, 1.0f)); // GUIDE3
        SurfaceTypeColors.Add(new(0.8f, 0.65f, 0.75f, 1.0f)); // PEBBLE
        SurfaceTypeColors.Add(new(0.8f, 0.8f, 0.2f, 1.0f)); // BEACH

        // Fill me
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
        SurfaceTypeColors.Add(new(0f, 0f, 0f, 1.0f)); // ??
    }

    public RoadComponent()
    {
        Name = "Road";
    }

    public void Init(RunwayFile runwayData)
    {
        RunwayData = runwayData;
    }

    public override void RenderComponent()
    {
        MeshBuilder meshBuilder = new(false, false);
        Color4Collection colors = new();

        RunwayFile rwy = RunwayData;
        for (int n = 0; n < rwy.RoadTris.Count; n++)
        {
            meshBuilder.AddTriangle(
                rwy.RoadVerts.Vertices[(int)rwy.RoadTris[n].VertA].Vertex.ToSharpDXVector(),
                rwy.RoadVerts.Vertices[(int)rwy.RoadTris[n].VertB].Vertex.ToSharpDXVector(),
                rwy.RoadVerts.Vertices[(int)rwy.RoadTris[n].VertC].Vertex.ToSharpDXVector());
            colors.Add(SurfaceTypeColors[rwy.RoadTris[n].SurfaceType]);
            colors.Add(SurfaceTypeColors[rwy.RoadTris[n].SurfaceType]);
            colors.Add(SurfaceTypeColors[rwy.RoadTris[n].SurfaceType]);
        }

        var m = meshBuilder.ToMesh();
        m.Colors = colors;
        m.AssignTo(RoadModel);
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        RoadModel.ClearAllGeometryData();
        RoadModel.UpdateVertices();
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
