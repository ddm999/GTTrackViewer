using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.Controls;
using GTTrackEditor.Utils;
using GTTrackEditor.Components;

namespace GTTrackEditor.Views
{
    public class RunwayView : TrackEditorViewBase
    {
        public override string TreeViewName => "Runway";
        public string FileName => "c004";

        public RNW5 RunwayData { get; private set; }

        public StartingGridComponent StartingGrid { get; set; } = new();
        public BoundaryComponent Boundary { get; set; } = new();
        public RoadComponent Road { get; set; } = new();
        public CheckpointComponent Checkpoints { get; set; } = new();

        public MeshGeometry3D PitStopAdjacentsModel { get; set; } = new();
        public DiffuseMaterial PitStopAdjacentsMaterial { get; set; } = new();

        public MeshGeometry3D PitStopsModel { get; set; } = new();
        public DiffuseMaterial PitStopsMaterial { get; set; } = new();


        public RunwayView()
        {
            PitStopsMaterial.DiffuseColor = new(1.0f, 0.0f, 0.0f, 1.0f);
        }

        public void SetRunwayData(RNW5 runway)
        {
            RunwayData = runway;
        }

        public bool Loaded()
        {
            return RunwayData is not null;
        }

        public void Init()    
        {
            StartingGrid.Init(RunwayData);
            Boundary.Init(RunwayData);
            Road.Init(RunwayData);
            Checkpoints.Init(RunwayData);

            Components.Add(Road);
            Components.Add(Boundary);
            Components.Add(StartingGrid);
            Components.Add(Checkpoints);

            foreach (var component in Components)
                component.RenderComponent();
        }

        public static MeshGeometry3D BuildArrows(List<Vec3R> vec3rs)
        {
            MeshBuilder meshBuilder = new(false, false);
            for (int i = 0; i < vec3rs.Count; i++)
            {
                Vector3 pos = vec3rs[i].ToVector3();
                float r = vec3rs[i].AngleRad;

                meshBuilder.AddSphere(pos, 0.025f);
                Vector3 pos2 = pos;
                pos2.X += (0 * MathF.Cos(-r)) - (0.1f * MathF.Sin(-r));
                pos2.Z += (0 * MathF.Sin(-r)) + (0.1f * MathF.Cos(-r));
                meshBuilder.AddArrow(pos, pos2, 0.01f, 3.0f, 18);
            }
            MeshGeometry3D model = meshBuilder.ToMesh();
            model.Normals = model.CalculateNormals();
            return model;
        }
    }
}
