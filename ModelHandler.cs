using DemoCore;
using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using GTTrackEditor.Readers.Entities.AutoDrive;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows;

using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

using GTTrackEditor.Views;

namespace GTTrackEditor
{
    public class ModelHandler : BaseViewModel
    {
        public MainWindow Parent;

        /// <summary>
        /// The center of the gizmo
        /// </summary>
        public Vector3 CenterOffset { get; set; }

        /// <summary>
        /// To which element the gizmo is currently being targetted
        /// </summary>
        public Element3D Target { get; set; }

        public RunwayView RunwayView { get; } = new();
        public CourseDataView CourseDataView { get; } = new();
        public AutodriveView AutodriveView { get; } = new();

        static MeshBuilder meshBuilder;

        public const float ScaleDividor = 50f;

        /// <summary>
        /// Grid, for utility
        /// </summary>
        public static LineGeometry3D Grid { get; set; }

        public static MeshGeometry3D PlainModel { get; set; }
        public static MeshGeometry3D NoneditModel { get; set; }

        public static MeshGeometry3D EditModel { get; set; } = new();
        public static DiffuseMaterial EditMaterial { get; set; } = new();

        public static MeshGeometry3D ManipulatedModel { get; set; }

        public static DiffuseMaterial PlainMaterial { get; set; } = new();
        public static DiffuseMaterial NoneditMaterial { get; set; } = new();
        
        
        public Vector3D DirectionalLightDirection { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Color AmbientLightColor { get; private set; }

        public ModelHandler()
        {
            EffectsManager = new DefaultEffectsManager();

            Camera = new PerspectiveCamera
            {
                Position = new Point3D(3, 3, 3),
                LookDirection = new Vector3D(-3, -3, -3),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 500,
                NearPlaneDistance = 0.001
            };

            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;
            DirectionalLightDirection = new Vector3D(-2, -5, -2);

            PlainMaterial.DiffuseColor = new(1.0f, 1.0f, 1.0f, 1.0f);
            NoneditMaterial.DiffuseColor = new(0.8f, 0.8f, 0.8f, 0.4f);
            EditMaterial.DiffuseColor = new(1.0f, 0.8f, 0.0f, 1.0f);


            Grid = LineBuilder.GenerateGrid(new Vector3(0, 1, 0), -25, 25, -25, 25);
        }

        public void OnMouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            if (e.HitTestResult != null && e.HitTestResult.ModelHit is PointGeometryModel3D m)
            {
                Parent.manipulator.Visibility = Visibility.Visible;
                Parent.manipulator.Target = null;
                Parent.manipulator.CenterOffset = m.Geometry.Bound.Center; // Must update this before updating target
                Parent.manipulator.Target = e.HitTestResult.ModelHit as Element3D;
            }

    /*
    // ignore everything except left mouse
    if (MouseButton.Left != ((MouseButtonEventArgs)e.OriginalInputEventArgs).ChangedButton)
        return;
    */
    ;
            /*
            ModelHandler.TargetElement = null;
            ModelHandler.CenterOffset = ((Element3D)e.HitTestResult.ModelHit).BoundsWithTransform.Center;
            ModelHandler.TargetElement = (Element3D)e.HitTestResult.ModelHit;
            */
        }

        public void BuildAxisesAtPoint(Vector3 point)
        {
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddArrow(point, point + new Vector3(+0.5f, 0, 0), 0.04f);
            meshBuilder.AddArrow(point, point + new Vector3(0, +0.5f, 0), 0.04f);
            meshBuilder.AddArrow(point, point + new Vector3(0, 0f, 0.5f), 0.04f);
            meshBuilder.ToMesh().AssignTo(EditModel);
        }

        public static void Trilists(List<Vector3> tris, List<Vector3> neTris, List<Vector3> eTris)
        {
            meshBuilder = new(false, false);
            meshBuilder.AddTriangles(tris);
            PlainModel = meshBuilder.ToMesh();
            PlainModel.Normals = PlainModel.CalculateNormals();

            meshBuilder = new(false, false);
            meshBuilder.AddTriangles(neTris);
            NoneditModel = meshBuilder.ToMesh();
            NoneditModel.Normals = NoneditModel.CalculateNormals();

            meshBuilder = new(false, false);
            meshBuilder.AddTriangles(eTris);
            EditModel = meshBuilder.ToMesh();
            EditModel.Normals = EditModel.CalculateNormals();
        }

        public static void ManipulatedPos(Vector3 pos)
        {
            meshBuilder = new(false, false);
            meshBuilder.AddSphere(pos, 0.005f);
            ManipulatedModel = meshBuilder.ToMesh();
            ManipulatedModel.Normals = ManipulatedModel.CalculateNormals();
        }
    }
}
