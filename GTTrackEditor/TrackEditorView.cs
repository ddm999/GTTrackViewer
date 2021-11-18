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
using System.Windows.Controls;

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

        public Vector3D DirectionalLightDirection { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Color AmbientLightColor { get; private set; }

        public ObservableCollection<Scripts.ScriptBase> ScriptMenuItems { get; } = new();

        public TrackEditorView()
        {
            ScriptMenuItems.Add(new Scripts.MakeBigRoad());
            ScriptMenuItems.Add(new Scripts.RemoveBoundaries());

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

                // Discard/Update manipulator events - can't find a better way to determine it
                bool isManipulator = (((target as MeshGeometryModel3D)?.Parent as GroupModel3D)?.Parent as GroupModel3D)?.Parent is TransformManipulator3D;

                if (isManipulator && Gizmo.Active)
                    UpdateEditMode();
                else if (e.HitTestResult.ModelHit is BaseModelEntity teModel)
                {
                    if (!Gizmo.Active || target != Gizmo.EditItem)
                    {
                        SetEditTarget(teModel);
                    }
                }
                else
                {
                    // Hit another item that is not the current one
                    if (Gizmo.Active)
                    {
                        ClearExplorerItem();
                        ExitEditMode();
                    }
                }
            }
            else
            {
                // Didn't hit anything at all
                if (Gizmo.Active)
                {
                    ClearExplorerItem();
                    ExitEditMode();
                }
            }
        }

        public void SetEditTarget(object o)
        {
            if (o is BaseModelEntity teModel)
            {
                EnterEditMode(teModel);
                UpdateEditMode();

                SelectExplorerItem(teModel);

                PropertyDefinitionCollection list = new();
                foreach (var prop in teModel.GetType().GetProperties().Where(p => p.GetCustomAttribute<EditableProperty>() is not null))
                    list.Add(new() { Name = prop.Name });

                Gizmo.EditItem = teModel;
                Parent.PropertyGrid.PropertyDefinitions = list;
            }
            else
            {
                ExitEditMode();
            }
        }

        private void SelectExplorerItem(object o)
        {
            var tvi = FindTviFromObjectRecursive(Parent.ExplorerTree, o);
            if (tvi != null) 
                tvi.IsSelected = true;
            }

        private void ClearExplorerItem()
        {
            DeselectTviRecursive(Parent.FindName("ExplorerTree") as TreeView);
        }

        private static TreeViewItem FindTviFromObjectRecursive(ItemsControl ic, object o)
        {
            //Search for the object model in first level children (recursively)
            if (ic.ItemContainerGenerator.ContainerFromItem(o) is TreeViewItem tvi) 
                return tvi;

            foreach (object i in ic.Items)
            {
                //Get the TreeViewItem associated with the iterated object model
                if (ic.ItemContainerGenerator.ContainerFromItem(i) is not TreeViewItem tvi2) 
                    continue;

                tvi = FindTviFromObjectRecursive(tvi2, o);
                if (tvi != null) 
                    return tvi;
            }
            return null;
        }

        private static void DeselectTviRecursive(ItemsControl ic)
        {
            foreach (object i in ic.Items)
            {
                if (ic.ItemContainerGenerator.ContainerFromItem(i) is not TreeViewItem tvi) continue;
                tvi.IsSelected = false;
                DeselectTviRecursive(tvi);
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
            Parent.GizmoManipulator.EnableRotationX = teModel.CanRotateX;
            Parent.GizmoManipulator.EnableRotationY = teModel.CanRotateY;
            Parent.GizmoManipulator.EnableRotationZ = teModel.CanRotateZ;

            Parent.GizmoManipulator.EnableTranslationX = teModel.CanTranslateX;
            Parent.GizmoManipulator.EnableTranslationY = teModel.CanTranslateY;
            Parent.GizmoManipulator.EnableTranslationZ = teModel.CanTranslateZ;
        }

        /// <summary>
        /// Fired when we are currently moving the gizmo.
        /// </summary>
        /// <param name="modelHit"></param>
        public void UpdateEditMode()
        {
            Vector3 newPos = (Gizmo.EditItem as GeometryModel3D).BoundsWithTransform.Center;
            Parent.tb_SelectedItemPosition.Text = $"Object: {newPos}";

            if (Gizmo.EditItem is BaseModelEntity entity)
                entity.OnMove();
        }

        /// <summary>
        /// Fired when exiting the edit mode.
        /// </summary>
        public void ExitEditMode()
        {
            Gizmo.SetInactive();
            Parent.GizmoManipulator.Visibility = Visibility.Hidden;
            Parent.GizmoManipulator.IsEnabled = false;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = Vector3.Zero;

            Parent.PropertyGrid.SelectedObject = null;

            Parent.tb_SelectedItemPosition.Text = "No object selected";
        }
    }
}
