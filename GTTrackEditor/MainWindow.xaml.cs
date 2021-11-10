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

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Input;
using SharpDX;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using GTTrackEditor.Components;
using GTTrackEditor.Views;
using GTTrackEditor.Interfaces;

namespace GTTrackEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _courseDataFileName;
        private PACB _courseData;

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

        #region File Load Events
        private void LoadPack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(openFileDialog.FileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    _courseData = PACB.FromStream(ref sr);
                    _courseDataFileName = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    //StatusText.Text = $"Error opening course pack: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                UpdateTrackModel();
                UpdateTitle();
            }
        }

        private void LoadRunway_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Runway Files|*.rwy|All Files|*.*";

            RNW5 rwy;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(openFileDialog.FileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    rwy = RNW5.FromStream(ref sr);
                    ModelHandler.RunwayView.SetRunwayData(rwy);
                    _rwyFileName = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    //StatusText.Text = $"Error opening runway: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Btn_ImportRunway.IsEnabled = true;
                ModelHandler.Views.Add(ModelHandler.RunwayView);
                ModelHandler.RunwayView.Init();
            }
        }

        private void LoadAutoDrive_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Autodrive Files|*.ad|All Files|*.*";

            AutoDrive ad;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(openFileDialog.FileName);
                    SpanReader sr = new(span, endian: Endian.Little);
                    ad = AutoDrive.FromStream(ref sr);
                    ModelHandler.AutodriveView.SetAutodriveData(ad);
                    _autoDriveFileName = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    //StatusText.Text = $"Error opening autodrive: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                //ModelHandler.AutodriveView.Render();
            }
        }
        #endregion

        #region Events
        private void ImportRunway_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                _rwyFileName = openFileDialog.FileName;

                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(_rwyFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    var rwy = new RNW5().MergeFromStream(ref sr);
                    ModelHandler.RunwayView.SetRunwayData(rwy);
                }
                catch (Exception ex)
                {
                    //StatusText.Text = $"Error opening old runway: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void SaveRunway_Click(object sender, RoutedEventArgs e)
        {
            if (!ModelHandler.RunwayView.Loaded())
            {
                MessageBox.Show(this, $"No runway file loaded.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new();
            if (saveFileDialog.ShowDialog() == true)
            {
                _rwyFileName = saveFileDialog.FileName;

                try
                {
                    Span<byte> span = File.ReadAllBytes(_rwyFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    SpanWriter sw = new(span, endian: Endian.Big);
                    ModelHandler.RunwayView.RunwayData.ToStream(ref sr, ref sw);
                    File.WriteAllBytes(_rwyFileName, span.ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Unable to save runway: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            window.ShowDialog();
        }

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

        void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;

            ContextMenu menu = new ContextMenu() { };

            if (item.Header is IHideable hideable)
            {
                MenuItem visibilityItem = new MenuItem();
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

                (sender as TreeViewItem).ContextMenu = menu;
            }
        }

        private void Component_Hide(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.DataContext is IHideable hideable)
            {
                hideable.Hide();
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

        private void ExplorerContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = ((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem;
            byte id = ExplorerItemToIndex(lbi);
            Span<byte> span = File.ReadAllBytes(_courseDataFileName);
            SpanReader sr = new(span, endian: Endian.Big);
            SpanWriter sw = new(span, endian: Endian.Big);

            _courseData.DeleteModel(ref sr, ref sw, id);
            File.WriteAllBytes(_courseDataFileName, span.ToArray());

            lbi.Visibility = Visibility.Hidden;
            UpdateTrackModel();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
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

        #endregion

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

        private void ExplorerVisibleRecalculate()
        {
           
        }

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
            try
            {
                List<Vector3> trilist = new();
                List<Vector3> editTrilist = new();
                List<Vector3> noneditTrilist = new();

                if (_courseData != null)
                {
                    for (byte m = 0; m < 6; m++)
                    {
                        if (!modelVisibility[m] || !_courseData.Models.ContainsKey(m))
                            continue;

                        for (ushort i = 0; i < _courseData.Models[m].MeshCount; i++)
                        {
                            /*if (modelVisibility[m].Item2[i] == false)
                                continue;*/

                            MDL3Mesh mesh = _courseData.Models[m].Meshes[i];
                            if (mesh.Tristrip == false)
                            {
                                for (ushort n = 0; n < mesh.Tris.Count; n++)
                                {
                                    trilist.Add(mesh.Verts[mesh.Tris[n].A] / 50);
                                    trilist.Add(mesh.Verts[mesh.Tris[n].B] / 50);
                                    trilist.Add(mesh.Verts[mesh.Tris[n].C] / 50);
                                }
                            }
                        }
                    }
                }

                if (ModelHandler.RunwayView.Loaded())
                {
                    ModelHandler.RunwayView.Init();
                }

                if (ModelHandler.AutodriveView.Loaded())
                {
                    ModelHandler.AutodriveView.Render();
                }

                TrackEditorView.Trilists(trilist, noneditTrilist, editTrilist);
            }
            catch (Exception ex)
            {
                //StatusText.Text = $"Error rendering track model: {ex.Message}";
                MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
    }
}
