using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows;

using GTTrackEditor.Controls;

namespace GTTrackEditor.Views
{
    public class RunwayView
    {
        public RNW5 RunwayData { get; private set; }

        public static List<Color4> SurfaceTypeColors { get; set; } = new();

        public MeshGeometry3D RoadModel { get; set; } = new();
        public DiffuseMaterial RoadMaterial { get; set; } = new();

        public MeshGeometry3D CheckpointModel { get; set; } = new();
        public DiffuseMaterial CheckpointMaterial { get; set; } = new();

        public MeshGeometry3D BoundaryModel { get; set; } = new();
        public DiffuseMaterial BoundaryMaterial { get; set; } = new();

        public MeshGeometry3D PitStopAdjacentsModel { get; set; } = new();
        public DiffuseMaterial PitStopAdjacentsMaterial { get; set; } = new();

        public MeshGeometry3D PitStopsModel { get; set; } = new();
        public DiffuseMaterial PitStopsMaterial { get; set; } = new();

        public ObservableElement3DCollection StartingGridPoints { get; set; } = new();
        public DiffuseMaterial StartingGridMaterial { get; set; } = new();

        public bool RenderCheckpoints { get; set; }
        public bool RenderRoad { get; set; }
        public bool RenderBoundaries { get; set; }

        public bool RenderStartingGrid { get; set; }
        public bool RenderPitStops { get; set; }
        public bool RenderPitStopsAdjacents { get; set; }

        public RunwayView()
        {
            CheckpointMaterial.DiffuseColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            PitStopsMaterial.DiffuseColor = new(1.0f, 0.0f, 0.0f, 1.0f);
            StartingGridMaterial.DiffuseColor = new(0.0f, 1.0f, 0.0f, 1.0f);

            BoundaryMaterial.DiffuseColor = new(1.0f, 1.0f, 0.0f, 1.0f);
        }

        static RunwayView()
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
        }

        public void SetRunwayData(RNW5 runway)
        {
            RunwayData = runway;
        }

        public bool Loaded()
        {
            return RunwayData is not null;
        }

        public void Render()
        {
            if (RenderStartingGrid && RunwayData.StartingGrid != null)
            {
                StartingGridPoints.Clear();
                for (int i = 0; i < RunwayData.StartingGrid.Count; i++)
                {
                    StartingGridModel3D startGridModel = new();
                    
                    /*
                    Vector3Collection vc = new(1);
                    IntCollection id = new(1);
                    Color4Collection col = new(1);
                    */

                    Vector3 pos = RunwayData.StartingGrid[i].ToVector3() / Consts.ScaleDividor;

                    ObjReader reader = new ObjReader();
                    List<Object3D> list = reader.Read("Grid.obj");
                    MeshGeometry3D mod = list[0].Geometry as MeshGeometry3D;
                    for (int j = 0; j < mod.Positions.Count; ++j)
                        mod.Positions[j] = mod.Positions[j] + pos;
                    mod.UpdateBounds();

                    StartingGridPoints.Add(new StartingGridModel3D()
                    {
                        Geometry = mod,
                        Material = StartingGridMaterial,
                    });

                    /*
                    vc.Add(pos);
                    id.Add(i);
                    col.Add(new(1, 0, 0, 1));
                    */

                    /*
                    startGridModel.Geometry = new PointGeometry3D()
                    {
                        Positions = vc,
                        Indices = id,
                        Colors = col,
                    };*/

                    StartingGridPoints.Add(startGridModel);
                }
            }

            if (RenderPitStops && RunwayData.PitStops != null)
            {
                MeshGeometry3D arrows = BuildArrows(RunwayData.PitStops);
                arrows.AssignTo(PitStopsModel);
            }
            else
            {
                PitStopsModel.ClearAllGeometryData();
            }


            if (RenderPitStopsAdjacents && RunwayData.PitStopAdjacents != null)
            {
                MeshGeometry3D arrows = BuildArrows(RunwayData.PitStopAdjacents);
                arrows.AssignTo(PitStopAdjacentsModel);
            }
            else
            {
                PitStopAdjacentsModel.ClearAllGeometryData();
            }
            

            if (RenderCheckpoints && RunwayData.Checkpoints != null)
            {
                BuildCheckpoints(RunwayData.Checkpoints);
            }
            else
            {
                CheckpointModel.ClearAllGeometryData();
            }

            if (RenderRoad && RunwayData.RoadVerts != null)
            {
                //BuildRoadSurface(RunwayData.RoadTris, RunwayData.RoadVerts);
            }
            else
            {
                RoadModel.ClearAllGeometryData();
            }

            if (RenderBoundaries && RunwayData.BoundaryVerts != null)
            {
                int i = 0;
                List<List<Vector3>> boundaries = new();
                List<Vector3> boundary = new();
                while (i < RunwayData.BoundaryVerts.Count)
                {
                    RNW5BoundaryVert vert = RunwayData.BoundaryVerts[i];
                    boundary.Add(vert.ToVector3());

                    if (vert.counter < 0) // boundary end
                    {
                        boundaries.Add(boundary);
                        boundary = new List<Vector3>();
                    }
                    i++;
                }

                BuildBoundaries(boundaries);
                //Line3D.Visibility = Visibility.Visible;
            }
            else
            {
                //Line3D.Visibility = Visibility.Hidden;
            }
        }

        public static MeshGeometry3D BuildArrows(List<Vec3R> vec3rs)
        {
            MeshBuilder meshBuilder = new(false, false);
            for (int i = 0; i < vec3rs.Count; i++)
            {
                Vector3 pos = vec3rs[i].ToVector3();
                float r = vec3rs[i].R;

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

        public void BuildBoundaries(List<List<Vector3>> boundaries)
        {
            MeshBuilder meshBuilder = new(false, false);
            for (int n = 0; n < boundaries.Count; n++)
            {
                List<Vector3> vec3s = boundaries[n];
                meshBuilder.AddTube(vec3s, 0.005f, 18, true);
            }
            BoundaryModel = meshBuilder.ToMesh();
        }

        public void BuildCheckpoints(List<RNW5Checkpoint4> checkpoints)
        {
            MeshBuilder meshBuilder = new(false, false);
            for (int i = 0; i < checkpoints.Count; i++)
            {
                meshBuilder.AddQuad(
                    checkpoints[i].left,
                    checkpoints[i].middle,
                    (checkpoints[i].middle) + new Vector3(0.0f, 3f, 0.0f),
                    (checkpoints[i].left) + new Vector3(0.0f, 3f, 0.0f)
                    );
                meshBuilder.AddQuad(
                    checkpoints[i].middle,
                    checkpoints[i].right,
                    (checkpoints[i].right) + new Vector3(0.0f, 3f, 0.0f),
                    (checkpoints[i].middle) + new Vector3(0.0f, 3f, 0.0f)
                    );
            }

            MeshGeometry3D checks = meshBuilder.ToMesh();
            checks.AssignTo(CheckpointModel);
            CheckpointModel.Normals = CheckpointModel.CalculateNormals();
        }

        public void BuildRoadSurface(List<RNW5RoadTri> roadTris, List<Vec3> roadVerts)
        {
            MeshBuilder meshBuilder = new(false, false);
            Color4Collection colors = new();
            for (int n = 0; n < roadTris.Count; n++)
            {
                meshBuilder.AddTriangle(
                    roadVerts[roadTris[n].vertA].ToVector3(),
                    roadVerts[roadTris[n].vertB].ToVector3(),
                    roadVerts[roadTris[n].vertC].ToVector3());
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
            }

            MeshGeometry3D road = meshBuilder.ToMesh();
            road.Colors = colors;
            road.AssignTo(RoadModel);
        }
    }
}
