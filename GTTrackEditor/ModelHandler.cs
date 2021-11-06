using DemoCore;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

using SharpDX;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

using GTTrackEditor.Views;
using GTTrackEditor.Controls;

namespace GTTrackEditor
{
    public class ModelHandler : BaseViewModel
    {
        public MainWindow Parent { get; set; }

        public Gizmo Gizmo { get; set; } = new();

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
                Position = new Point3D(300, 300, 300),
                LookDirection = new Vector3D(-3, -3, -3),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 4000,
                NearPlaneDistance = 0.001,
            };

            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;
            DirectionalLightDirection = new Vector3D(-2, -5, -2);

            PlainMaterial.DiffuseColor = new(1.0f, 1.0f, 1.0f, 1.0f);
            NoneditMaterial.DiffuseColor = new(0.8f, 0.8f, 0.8f, 0.4f);
            EditMaterial.DiffuseColor = new(1.0f, 0.8f, 0.0f, 1.0f);


            Grid = GenerateGrid(Vector3.UnitY, -1000, 1000, -1000, 1000, 100f);
        }

        /// <summary>
        /// Generates a square grid with a custom step
        /// </summary>
        /// <returns></returns>
        public static LineGeometry3D GenerateGrid(Vector3 plane, int min0 = 0, int max0 = 10, int min1 = 0, int max1 = 10, float step = 1f)
        {
            LineBuilder grid = new();
            if (plane == Vector3.UnitX)
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(0, i, min1), new Vector3(0, i, max1));
                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(0, min0, i), new Vector3(0, max0, i));
                }
            }
            else if (plane == Vector3.UnitY)
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(i, 0, min1), new Vector3(i, 0, max1));

                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(min0, 0, i), new Vector3(max0, 0, i));
                }
            }
            else
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(i, min1, 0), new Vector3(i, max1, 0));
                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(min0, i, 0), new Vector3(max0, i, 0));
                }
            }

            return grid.ToLineGeometry3D();
        }

        /// <summary>
        /// Fired when we are clicking on anything on the viewport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            // Ignore all except left, because we might be changing camera state
            if (((MouseButtonEventArgs)e.OriginalInputEventArgs).ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.HitTestResult != null)
            {
                object target = e.HitTestResult.ModelHit;
                if (e.HitTestResult.ModelHit is StartingGridModel3D m)
                {
                    if (!Gizmo.Active || target != Gizmo.EditItem)
                    {
                        EnterEditMode(e.HitTestResult.ModelHit);
                        Parent.PropertyGrid.SelectedObject = m;
                    }
                }
            }
            else
            {
                if (Gizmo.Active)
                {
                    ExitEditMode();
                }
            }
        }

        /// <summary>
        /// Fired when we are clicking on an editable object.
        /// </summary>
        /// <param name="modelHit"></param>
        private void EnterEditMode(object modelHit)
        {
            Gizmo.SetActive(modelHit as Element3D);

            Parent.GizmoManipulator.Visibility = Visibility.Visible;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = (modelHit as GeometryModel3D).Geometry.Bound.Center; // Must update this before updating target
            Parent.GizmoManipulator.Target = modelHit as Element3D;
            Parent.GizmoManipulator.EnableScaling = false;
            Parent.GizmoManipulator.EnableXRayGrid = false;
            Parent.GizmoManipulator.EnableRotation = false;
        }

        /// <summary>
        /// Fired when we are currently moving the gizmo.
        /// </summary>
        /// <param name="modelHit"></param>
        public void UpdateEditMode()
        {
            Vector3 newPos = (Gizmo.EditItem as GeometryModel3D).BoundsWithTransform.Center;
            Parent.tb_SelectedItemPosition.Text = newPos.ToString();
        }

        /// <summary>
        /// Fired when exiting the edit mode.
        /// </summary>
        private void ExitEditMode()
        {
            Gizmo.SetInactive();
            Parent.GizmoManipulator.Visibility = Visibility.Hidden;
            Parent.GizmoManipulator.IsEnabled = false;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = Vector3.Zero;
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
