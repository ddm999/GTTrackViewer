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
                BuildRenderStartingGrid();
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
                BuildRenderBoundaries();
                //Line3D.Visibility = Visibility.Visible;
            }
            else
            {
                //Line3D.Visibility = Visibility.Hidden;
            }
        }

        private void BuildRenderStartingGrid()
        {
            StartingGridPoints.Clear();
            for (int i = 0; i < RunwayData.StartingGrid.Count; i++)
            {
                ObjReader reader = new();
                List<Object3D> list = reader.Read("Models/Grid.obj");
                MeshGeometry3D gridGeometry = list[0].Geometry as MeshGeometry3D;

                Vector3 actualPos = RunwayData.StartingGrid[i].ToVector3();
                for (int j = 0; j < gridGeometry.Positions.Count; ++j)
                {
                    gridGeometry.Positions[j] += actualPos;
                }
                gridGeometry.UpdateBounds();

                StartingGridModel3D newGridModel = new()
                {
                    Geometry = gridGeometry,
                    Material = StartingGridMaterial,

                    StartingIndex = i,
                };

                Point3D center = new(RunwayData.StartingGrid[i].X, RunwayData.StartingGrid[i].Y, RunwayData.StartingGrid[i].Z);
                System.Diagnostics.Debug.WriteLine(RunwayData.StartingGrid[i].AngleRad);


                // Apply angle
                float angle = MathUtils.PDRadToDeg(RunwayData.StartingGrid[i].AngleRad);
                ModelUtils.Rotate(newGridModel, center, angle);

                StartingGridPoints.Add(newGridModel);
            }
        }

        public void BuildRenderBoundaries()
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

            MeshBuilder meshBuilder = new(false, false);
            for (int n = 0; n < boundaries.Count; n++)
            {
                List<Vector3> vec3s = boundaries[n];
                meshBuilder.AddTube(vec3s, 1f, 18, true);
            }

            MeshGeometry3D m = meshBuilder.ToMesh();
            m.AssignTo(BoundaryModel);
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
