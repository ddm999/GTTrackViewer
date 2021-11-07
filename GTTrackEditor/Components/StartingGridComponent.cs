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

namespace GTTrackEditor.Components
{
    public class StartingGridComponent : TrackComponentBase
    {
        public override string Name => "Starting Grid";

        public ObservableElement3DCollection StartingGridPoints { get; set; } = new();
        public DiffuseMaterial StartingGridMaterial { get; set; } = new();

        public RNW5 RunwayData { get; set; }

        public StartingGridComponent()
        {
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

                // Apply angle
                float angle = MathUtils.PDRadToDeg(RunwayData.StartingGrid[i].AngleRad);
                ModelUtils.Rotate(newGridModel, center, angle);

                StartingGridPoints.Add(newGridModel);
            }
        }

        public override void Hide()
        {
            StartingGridPoints.Clear();
        }
    }
}
