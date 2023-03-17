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
using System.ComponentModel;
using System.Windows.Controls;

using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

using GTTrackEditor.Views;
using GTTrackEditor.ModelEntities;

using Xceed.Wpf.Toolkit.PropertyGrid;

using GTTrackEditor.Utils;
using System.Windows.Media;

namespace GTTrackEditor
{
    public class TrackEditorView : BaseViewModel
    {
        public MainWindow Parent { get; set; }

        public Gizmo Gizmo { get; set; }

        public Element3D _propertyGridSelectedItem { get; set; }
        public Element3D PropertyGridSelectedItem
        {
            get => _propertyGridSelectedItem;
            set
            {
                _propertyGridSelectedItem = value;
                OnPropertyChanged(nameof(PropertyGridSelectedItem));
            }
        }

        public ObservableCollection<TrackEditorViewBase> Views { get; set; } = new();

        public RunwayView RunwayView { get; } = new();
        public CourseDataView CourseDataView { get; } = new();
        public AutodriveView AutodriveView { get; } = new();
        public CourseMapView MinimapView { get; } = new();

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
                else if (e.HitTestResult.ModelHit is BaseModelEntity teModel && teModel.IsHitTestVisible)
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

                var item = GetTreeViewItem(Parent.TreeViewRoot, teModel);
                item.IsSelected = true;

                PropertyDefinitionCollection list = new();
                foreach (var prop in teModel.GetType().GetProperties().Where(p => p.GetCustomAttribute<BrowsableAttribute>() is not null))
                    list.Add(new() { Name = prop.Name });

                Gizmo.EditItem = teModel;
                Parent.PropertyGrid.PropertyDefinitions = list;
            }
            else
            {
                ExitEditMode();
            }
        }

        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview
        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the
                // virtualizing case even if the item is marked
                // expanded we still need to do this step in order to
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter,
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                MyVirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as MyVirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(i);

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
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
            PropertyGridSelectedItem = teModel;

            Parent.GizmoManipulator.Visibility = Visibility.Visible;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = teModel.Geometry.Bound.Center; // Must update this before updating target
            Parent.GizmoManipulator.Target = teModel;

            Parent.GizmoManipulator.EnableScaling = teModel.CanScale;
            Parent.GizmoManipulator.EnableRotationX = teModel.PitchRotationAllowed;
            Parent.GizmoManipulator.EnableRotationY = teModel.YawRotationAllowed;
            Parent.GizmoManipulator.EnableRotationZ = teModel.RollRotationAllowed;

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
            BaseModelEntity entity = Gizmo.EditItem as BaseModelEntity;
            Vector3 newPos = entity.BoundsWithTransform.Center;
            Parent.tb_SelectedItemPosition.Text = $"Object: {newPos} | {entity.YawAngle}";

            entity.OnManipulation();
        }

        /// <summary>
        /// Fired when exiting the edit mode.
        /// </summary>
        public void ExitEditMode(bool deactivatePropertyGrid = true)
        {
            Gizmo.SetInactive();
            Parent.GizmoManipulator.Visibility = Visibility.Hidden;
            Parent.GizmoManipulator.IsEnabled = false;
            Parent.GizmoManipulator.Target = null;
            Parent.GizmoManipulator.CenterOffset = Vector3.Zero;

            if (deactivatePropertyGrid)
                PropertyGridSelectedItem = null;

            Parent.tb_SelectedItemPosition.Text = "No object selected";
        }
    }
}
