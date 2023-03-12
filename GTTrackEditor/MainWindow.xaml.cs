using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;
using System.Text;
using System.Linq;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Input;

using MahApps.Metro.Controls;

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

using SharpDX;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using GTTrackEditor.Components;
using GTTrackEditor.Views;
using GTTrackEditor.Interfaces;
using GTTrackEditor.ModelEntities;

using PDTools.Files.Courses.Runway;
using PDTools.Files.Courses.AutoDrive;
using PDTools.Files.Courses.Minimap;
using PDTools.Files.Courses.CourseData;
using PDTools.Files.Models.ModelSet3.ShapeStream;
using PDTools.Files.Models.ModelSet3;
using PDTools.Files.Models.ShapeStream;

namespace GTTrackEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string _courseDataFileName;
        private string _rwyFileName;
        private string _autoDriveFileName;

        Dictionary<byte, bool> modelVisibility = new();

        private readonly Brush _visibleBrush = new SolidColorBrush(Colors.Black);
        private readonly Brush _editBrush = new SolidColorBrush(Colors.Blue);
        private readonly Brush _editHiddenBrush = new SolidColorBrush(Colors.LightBlue);
        private readonly Brush _hiddenBrush = new SolidColorBrush(Colors.LightGray);

        public TrackEditorView ModelHandler { get; } = new();

        public MainWindow()
        {

            TrackEditorConfig.Init();
            InitializeComponent();

            // Grab reference set from XAML
            ModelHandler = (TrackEditorView)DataContext;
            ModelHandler.Parent = this;

            ReflectConfig();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TrackEditorConfig.Save();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Course Data Files (c***x)|*.*|" +
                "Runway Files|*.rwy|" +
                "Autodrive Files|*.ad|" +
                "Course Map Files|*.map|" +
                "ShapeStream Files |*.shapestream";

            if (openFileDialog.ShowDialog() == true)
            {
#if !DEBUG
                try
                {
#endif
                using var file = File.Open(openFileDialog.FileName, FileMode.Open);


                if (openFileDialog.FileName.EndsWith(".rwy") || openFileDialog.FileName.Contains("runway", StringComparison.OrdinalIgnoreCase))
                {
                    HandleRunway(file, openFileDialog.FileName);
                }
                else if (openFileDialog.FileName.EndsWith(".map"))
                {
                    HandleMinimap(file, openFileDialog.FileName);
                }
                else if (openFileDialog.FileName.EndsWith(".ad"))
                {

                }
                else if (openFileDialog.FileName.EndsWith("x"))
                {
                    HandleCourseData(file, openFileDialog.FileName);
                }
                else if (openFileDialog.FileName.EndsWith(".shapestream"))
                {
                    HandleShapeStream(file, openFileDialog.FileName);
                }
#if !DEBUG

                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error opening file: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
#endif
                UpdateTitle();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            window.ShowDialog();
        }

        void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;

            ContextMenu menu = new ContextMenu() { };
            if (item.Header is IModelCollection collection)
            {
                MenuItem visibilityItem = new();
                visibilityItem.DataContext = item.Header;

                visibilityItem.Header = "Add new element";
                visibilityItem.Click += Component_AddNewEntityClicked;

                menu.Items.Add(visibilityItem);
            }

            if (item.Header is BaseModelEntity)
            {
                MenuItem visibilityItem = new();
                visibilityItem.DataContext = item;

                visibilityItem.Header = "Delete";
                visibilityItem.Click += Entity_RemoveClicked;

                menu.Items.Add(visibilityItem);
            }

            if (item.Header is IHideable hideable)
            {
                MenuItem visibilityItem = new();
                visibilityItem.DataContext = item.Header;

                if (hideable.IsVisible)
                {
                    visibilityItem.Header = "Hide Element";
                    visibilityItem.Click += Component_HideClicked;
                }
                else
                {
                    visibilityItem.Header = "Show Element";
                    visibilityItem.Click += Component_ShowClicked;
                }
                menu.Items.Add(visibilityItem);
            }

            if (item.Header is RunwayView rwyView)
            {
                MenuItem exportItem = new();
                exportItem.DataContext = item.Header;
                exportItem.Header = "Export to .rwy";
                exportItem.Click += Runway_Export;

                menu.Items.Add(exportItem);
            }

            (sender as TreeViewItem).ContextMenu = menu;
        }

        private void Runway_Export(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.ValidateNames = true;

            if (dlg.ShowDialog() == true)
            {
                using var file = dlg.OpenFile();
                ModelHandler.RunwayView.RunwayData.ToStream(file);

                MessageBox.Show($"Runway successfully exported as {dlg.FileName}.", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Component_AddNewEntityClicked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.DataContext is IModelCollection collection)
            {
                collection.AddNew();
            }
        }

        private void Entity_RemoveClicked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item is null)
                return;

            if (item.DataContext is not TreeViewItem tvi)
                return;

            ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(tvi);
            if (parent is null)
                return;

            if (parent.DataContext is not IModelCollection collection)
                return;

            var elem = tvi.DataContext as Element3D;

            if (ModelHandler.Gizmo.Active && ModelHandler.Gizmo.EditItem == elem)
                ModelHandler.ExitEditMode();

            collection.Remove(tvi.DataContext as Element3D);
        }

        private void Component_HideClicked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.DataContext is IHideable hideable)
            {
                hideable.Hide();
            }

            Gizmo gizmo = ModelHandler.Gizmo;
            if (gizmo.Active && gizmo.EditItem.Visibility == Visibility.Hidden)
            {
                ModelHandler.ExitEditMode();
            }
        }

        private void Component_ShowClicked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.DataContext is IHideable hideable)
            {
                hideable.Show();
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item is null)
                return;

            if (item.Header is Element3D elem && elem.IsHitTestVisible)
                ModelHandler.SetEditTarget(item.Header);
        }

        private void ScriptMenu_Click(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            try
            {
#endif
                var script = (e.OriginalSource as MenuItem).Header as Scripts.ScriptBase;
                script.Execute(ModelHandler);
#if !DEBUG
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error executing script: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#endif
        }

        private void ToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            TrackEditorConfig.SetSetting("EnableGrid", ToggleGrid_Checkbox.IsChecked.GetValueOrDefault());
            Grid3D.IsRendering = (bool)ToggleGrid_Checkbox.IsChecked;
        }

        private void RotateMode_Click(object sender, RoutedEventArgs e)
        {
            TrackEditorConfig.SetSetting("RotateMode", RotateMode_Checkbox.IsChecked.GetValueOrDefault());
            _viewport.CameraMode = (bool)RotateMode_Checkbox.IsChecked ? CameraMode.Inspect : CameraMode.WalkAround;
        }

        private void ReflectConfig()
        {
            TrackEditorConfig.TryGetBool("EnableGrid", out bool val, true);
            ToggleGrid_Checkbox.IsChecked = val;
            Grid3D.IsRendering = val;

            TrackEditorConfig.TryGetBool("RotateMode", out val, false);
            RotateMode_Checkbox.IsChecked = val;
            _viewport.CameraMode = val ? CameraMode.Inspect : CameraMode.WalkAround;

        }

        private void UpdateTitle()
        {
            StringBuilder sb = new StringBuilder();

            // Build file state
            if (!string.IsNullOrEmpty(_courseDataFileName))
            {
                sb.Append(' ').Append(_courseDataFileName.Split("\\")[^1]);
                if (!string.IsNullOrEmpty(_rwyFileName))
                    sb.Append('/');
            }

            if (!string.IsNullOrEmpty(_rwyFileName))
            {
                sb.Append(' ').Append(_rwyFileName.Split("\\")[^1]);
                if (!string.IsNullOrEmpty(_autoDriveFileName))
                    sb.Append('/');
            }

            if (!string.IsNullOrEmpty(_autoDriveFileName))
            {
                sb.Append(' ').Append(_autoDriveFileName.Split("\\")[^1]);
                // Add more as needed
            }

            string state = string.Empty;
            // End
            if (sb.Length > 0)
            {
                state = $"({sb} )";
            }

            Title = $"GT Track Viewer {state}";
        }

        /// <summary>
        /// Used to update the point tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GizmoManipulator_Mouse3DMove(object sender, MouseMove3DEventArgs e)
        {
            if (ModelHandler.Gizmo.Active)
            {
                ModelHandler.UpdateEditMode();
            }
        }

        static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }

        private void HandleRunway(Stream stream, string fileName)
        {
            if (ModelHandler.RunwayView.Loaded())
            {
                RunwayFile runway_other = RunwayFile.FromStream(stream);
                // if mergable, merge: else replace
                ModelHandler.RunwayView.RunwayData.Merge(runway_other);
                string newName = Path.GetFileNameWithoutExtension(fileName);
                ModelHandler.RunwayView.Init();
                ModelHandler.RunwayView.Render();
                return;

            }

            RunwayFile runway = RunwayFile.FromStream(stream);
            ModelHandler.RunwayView.SetRunwayData(runway);
            _rwyFileName = fileName;

            if (!ModelHandler.Views.Contains(ModelHandler.RunwayView))
                ModelHandler.Views.Add(ModelHandler.RunwayView);

            ModelHandler.RunwayView.FileName = Path.GetFileNameWithoutExtension(fileName);
            ModelHandler.RunwayView.Init();
            ModelHandler.RunwayView.Render();
        }

        private void HandleMinimap(Stream stream, string fileName)
        {
            CourseMapFile runway = CourseMapFile.FromStream(stream);
            ModelHandler.MinimapView.SetMinimapData(runway);

            if (!ModelHandler.Views.Contains(ModelHandler.MinimapView))
                ModelHandler.Views.Add(ModelHandler.MinimapView);

            ModelHandler.MinimapView.FileName = Path.GetFileNameWithoutExtension(fileName);
            ModelHandler.MinimapView.Init();
            ModelHandler.MinimapView.Render();
        }

        private void HandleCourseData(Stream stream, string fileName)
        {
            CourseDataFile courseData = CourseDataFile.FromStream(stream);
            ModelHandler.CourseDataView.SetCourseData(courseData);
            _courseDataFileName = fileName;

            if (!ModelHandler.Views.Contains(ModelHandler.CourseDataView))
                ModelHandler.Views.Add(ModelHandler.CourseDataView);

            ModelHandler.CourseDataView.Init();
            ModelHandler.CourseDataView.Render();
        }

        private void HandleShapeStream(Stream stream, string fileName)
        {
            if (!ModelHandler.Views.Contains(ModelHandler.CourseDataView))
                throw new NotSupportedException("A Course Data file must first be loaded to load ShapeStream data!");

            ModelSet3 mdl = ModelHandler.CourseDataView.CourseData.MainModel;

            var ss = ShapeStream.FromStream(stream, mdl);
            mdl.ShapeStream = ss;

            ModelHandler.CourseDataView.Init();
            ModelHandler.CourseDataView.Render();
        }

        /// <summary>
        /// Fired when the property grid is previewed. Cancels edit mode to avoid possible conflicts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertyGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ModelHandler.Gizmo.Active)
                ModelHandler.ExitEditMode(deactivatePropertyGrid: false);
        }
    }
}
