using DemoCore;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

using SharpDX;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

using GTTrackEditor.Views;
using GTTrackEditor.ModelEntities;

using Xceed.Wpf.Toolkit.PropertyGrid;

using GTTrackEditor.Utils;
using GTTrackEditor.Attributes;

namespace GTTrackEditor
{
    public class TrackEditorView : BaseViewModel
    {
        public MainWindow Parent { get; set; }

        public Gizmo Gizmo { get; set; }

        public ObservableCollection<TrackEditorViewBase> Views { get; set; } = new();

        public RunwayView RunwayView { get; } = new();
        public CourseDataView CourseDataView { get; } = new();
        public AutodriveView AutodriveView { get; } = new();

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

        public TrackEditorView()
        {
            EffectsManager = new DefaultEffectsManager();

            Camera = new PerspectiveCamera
            {
                Position = new Point3D(300, 300, 300),
                LookDirection = new Vector3D(-3, -3, -3),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 10000,
                NearPlaneDistance = 0.001,
            };

            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;
            DirectionalLightDirection = new Vector3D(-2, -5, -2);

            PlainMaterial.DiffuseColor = new(1.0f, 1.0f, 1.0f, 1.0f);
            NoneditMaterial.DiffuseColor = new(0.8f, 0.8f, 0.8f, 0.4f);
            EditMaterial.DiffuseColor = new(1.0f, 0.8f, 0.0f, 1.0f);

            Gizmo = new();
            Grid = LineGeneratorUtils.GenerateGrid(Vector3.UnitY, -1000, 1000, -1000, 1000, 100f);
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
                if (e.HitTestResult.ModelHit is BaseModelEntity teModel)
                {
                    if (!Gizmo.Active || target != Gizmo.EditItem)
                    {
                        EnterEditMode(teModel); 
                        UpdateEditMode();

                        PropertyDefinitionCollection list = new();
                        foreach (var prop in target.GetType().GetProperties().Where(p => p.GetCustomAttribute<EditableProperty>() is not null))
                            list.Add(new() { Name = prop.Name });

                        Parent.PropertyGrid.PropertyDefinitions = list;
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
        private void EnterEditMode(BaseModelEntity teModel)
        {
            Gizmo.SetActive(teModel);

            Parent.GizmoManipulator.Visibility = Visibility.Visible;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = teModel.Geometry.Bound.Center; // Must update this before updating target
            Parent.GizmoManipulator.Target = teModel;

            Parent.GizmoManipulator.EnableScaling = teModel.CanScale;
            Parent.GizmoManipulator.EnableRotation = teModel.CanRotate;
            Parent.GizmoManipulator.EnableTranslation = teModel.CanTranslate;
        }

        /// <summary>
        /// Fired when we are currently moving the gizmo.
        /// </summary>
        /// <param name="modelHit"></param>
        public void UpdateEditMode()
        {
            Vector3 newPos = (Gizmo.EditItem as GeometryModel3D).BoundsWithTransform.Center;
            //Parent.tb_SelectedItemPosition.Text = newPos.ToString();
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
            MeshBuilder meshBuilder = new(false, false);
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
            MeshBuilder meshBuilder = new(false, false);
            meshBuilder.AddSphere(pos, 0.005f);
            ManipulatedModel = meshBuilder.ToMesh();
            ManipulatedModel.Normals = ManipulatedModel.CalculateNormals();
        }
    }
}
