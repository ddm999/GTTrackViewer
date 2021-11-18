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
using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using PDTools.Files.Courses.Runway;
using PDTools.Files.Courses.AutoDrive;

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
                "Autodrive Files|*.ad";

            if (openFileDialog.ShowDialog() == true)
            {
#if !DEBUG
                try
                {
#endif
                    using var file = File.Open(openFileDialog.FileName, FileMode.Open);

                    if (openFileDialog.FileName.EndsWith(".rwy"))
                    {
                        HandleRunway(file, openFileDialog.FileName);
                    }
                    else if (openFileDialog.FileName.EndsWith(".ad"))
                    {

                    }
                    else if (openFileDialog.FileName.EndsWith("x"))
                    {
                        //HandleCourseData(ref sr, openFileDialog.FileName);
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

        /*
        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ExplorerVisibleToggle_Buttons(e.Source as ListBoxItem);
        }

        private void ExplorerContextMenu_Visibility_Click(object sender, RoutedEventArgs e)
        {
            ExplorerVisibleToggle_Buttons(((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem);
        }

        private void ExplorerContextMenu_Edit_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = ((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem;
            byte index = ExplorerItemToIndex(lbi);
            UpdateTrackModel();
            ExplorerVisibleRecalculate();
        }
        */

        void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;

            ContextMenu menu = new ContextMenu() { };

            if (item.Header is IHideable hideable)
            {
                MenuItem visibilityItem = new();
                visibilityItem.DataContext = item.Header;

                if (hideable.IsVisible)
                {
                    visibilityItem.Header = "Hide Element";
                    visibilityItem.Click += Component_Hide;
                }
                else
                {
                    visibilityItem.Header = "Show Element";
                    visibilityItem.Click += Component_Show;
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
            MemoryStream ms = new MemoryStream();
            ModelHandler.RunwayView.RunwayData.ToStream(ms);
        }

        private void Component_Hide(object sender, RoutedEventArgs e)
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

        private void Component_Show(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.DataContext is IHideable hideable)
            {
                hideable.Show();
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;

            ModelHandler.SetEditTarget(item.Header);
        }

        /*
        private void ExplorerContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = ((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem;
            byte id = ExplorerItemToIndex(lbi);
            Span<byte> span = File.ReadAllBytes(_courseDataFileName);
            SpanReader sr = new(span, endian: Endian.Big);
            SpanWriter sw = new(span, endian: Endian.Big);

            ModelHandler.CourseDataView.CourseData.DeleteModel(ref sr, ref sw, id);
            File.WriteAllBytes(_courseDataFileName, span.ToArray());

            lbi.Visibility = Visibility.Hidden;
            UpdateTrackModel();
        }
        */

        private void ScriptButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "right click on that button instead of left clicking", "c# sucks", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
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
                state = $"( {sb} )";
            }

            Title = $"GT Track Editor {state}";
        }

        /*
        private void ExplorerVisibleToggle(ListBoxItem lbi, byte i)
        {
            if (modelVisibility[i])
            {
                modelVisibility[i] = false;
                lbi.Foreground = lbi.Foreground == _editBrush ? _editHiddenBrush : _hiddenBrush;
            }
            else
            {
                modelVisibility[i] = true;
                lbi.Foreground = lbi.Foreground == _editHiddenBrush ? _editBrush : _visibleBrush;
            }
        }

        private static byte ExplorerItemToIndex(ListBoxItem lbi)
        {
            string name = lbi.Content as string;
            switch (name[0])
            {
                case 'M': // MDL
                    return name[4] switch
                    {
                        '0' => 0,
                        '1' => 1,
                        '2' => 2,
                        '3' => 3,
                        '4' => 4,
                        '5' => 5,
                        _ => throw new InvalidDataException(),
                    };
                case 'R': // RWY
                    return name[4] switch
                    {
                        '0' => 6,
                        '1' => 7,
                        '2' => 8,
                        '3' => 9,
                        _ => throw new InvalidDataException(),
                    };
                case 'A': // AD
                    return name[4] switch
                    {
                        '0' => 10,
                        '1' => 11,
                        '2' => 12,
                        '3' => 13,
                        '4' => 14,
                        '5' => 15,
                        '6' => 16,
                        _ => throw new InvalidDataException(),
                    };
                default:
                    throw new InvalidDataException();
            }
        }

        private void ExplorerVisibleToggle_Buttons(ListBoxItem item)
        {
            ExplorerVisibleToggle(item, ExplorerItemToIndex(item));
            UpdateTrackModel();
        }

        private void UpdateTrackModel()
        {

        }
        */

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
                if (runway_other.VersionMajor == 2 && ModelHandler.RunwayView.RunwayData.VersionMajor >= 40U)
                {
                    ModelHandler.RunwayView.RunwayData.Merge(runway_other);
                    string newName = Path.GetFileNameWithoutExtension(fileName);
                    ModelHandler.RunwayView.Init();
                    ModelHandler.RunwayView.Render();
                    return;
                }
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

        private void HandleCourseData(ref SpanReader sr, string fileName)
        {
            PACB courseData = PACB.FromStream(ref sr);
            ModelHandler.CourseDataView.SetCourseData(courseData);
            _courseDataFileName = fileName;

            if (!ModelHandler.Views.Contains(ModelHandler.CourseDataView))
                ModelHandler.Views.Add(ModelHandler.CourseDataView);

            ModelHandler.CourseDataView.Init();
            ModelHandler.CourseDataView.Render();
        }
    }
}
