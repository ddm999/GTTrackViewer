﻿using System;
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
using System.Windows.Media.Animation;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

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

        private short _editModelIndex = -1;
        private int _editMeshIndex = -1;
        private int _editVertex = -1;

        private readonly Brush _visibleBrush = new SolidColorBrush(Colors.Black);
        private readonly Brush _editBrush = new SolidColorBrush(Colors.Blue);
        private readonly Brush _editHiddenBrush = new SolidColorBrush(Colors.LightBlue);
        private readonly Brush _hiddenBrush = new SolidColorBrush(Colors.LightGray);

        public ModelHandler ModelHandler { get; } = new();

        public MainWindow()
        {
            TrackEditorConfig.Init();
            InitializeComponent();

            // Grab reference set from XAML
            ModelHandler = (ModelHandler)DataContext;
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
                    StatusText.Text = $"Error opening course pack: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ExplorerListBoxItemTrack.Visibility = Visibility.Hidden;
                ExplorerListBoxItem1.Visibility = Visibility.Hidden;
                ExplorerListBoxItem2.Visibility = Visibility.Hidden;
                ExplorerListBoxItem3.Visibility = Visibility.Hidden;
                ExplorerListBoxItem4.Visibility = Visibility.Hidden;
                ExplorerListBoxItem5.Visibility = Visibility.Hidden;

                for (byte m = 0; m < 6; m++)
                {
                    if (modelVisibility.ContainsKey(m))
                        modelVisibility[m] = true;
                    else
                        modelVisibility.Add(m, true);

                    if (_courseData.Models.ContainsKey(m))
                    {
                        switch (m)
                        {
                            case 0:
                                ExplorerListBoxItemTrack.Content = $"MDL 0 Track ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItemTrack.Visibility = Visibility.Visible;
                                ExplorerListBoxItemTrack.Foreground = _visibleBrush;
                                break;
                            case 1:
                                ExplorerListBoxItem1.Content = $"MDL 1 Skydome A ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItem1.Visibility = Visibility.Visible;
                                ExplorerListBoxItem1.Foreground = _hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 2:
                                ExplorerListBoxItem2.Content = $"MDL 2 Distant Terrain ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItem2.Visibility = Visibility.Visible;
                                ExplorerListBoxItem2.Foreground = _visibleBrush;
                                break;
                            case 3:
                                ExplorerListBoxItem3.Content = $"MDL 3 Mirrors Track ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItem3.Visibility = Visibility.Visible;
                                ExplorerListBoxItem3.Foreground = _hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 4:
                                ExplorerListBoxItem4.Content = $"MDL 4 Skydome B ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItem4.Visibility = Visibility.Visible;
                                ExplorerListBoxItem4.Foreground = _hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 5:
                                ExplorerListBoxItem5.Content = $"MDL 5 ({_courseData.Models[m].MeshCount} meshes / {_courseData.Models[m].TriCount} tris)";
                                ExplorerListBoxItem5.Visibility = Visibility.Visible;
                                ExplorerListBoxItem5.Foreground = _visibleBrush;
                                break;
                        }
                    }
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
                    StatusText.Text = $"Error opening runway: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Btn_ImportRunway.IsEnabled = true;

                ExplorerListBoxItem6.Visibility = Visibility.Hidden;
                ExplorerListBoxItem7.Visibility = Visibility.Hidden;
                ExplorerListBoxItem8.Visibility = Visibility.Hidden;
                ExplorerListBoxItem9.Visibility = Visibility.Hidden;

                for (byte m = 6; m < 10; m++)
                {
                    if (modelVisibility.ContainsKey(m))
                        modelVisibility[m] = true;
                    else
                        modelVisibility.Add(m, true);

                    switch (m)
                    {
                        case 6:
                            ExplorerListBoxItem6.Content = $"RWY 0 Positions ({rwy.StartingGrid.Count} starts, {rwy.PitStops.Count} pits, {rwy.PitStopAdjacents.Count} pit adjacent)";
                            ExplorerListBoxItem6.Visibility = Visibility.Visible;
                            ExplorerListBoxItem6.Foreground = _visibleBrush;
                            ModelHandler.RunwayView.RenderStartingGrid = true;
                            break;
                        case 7:
                            ExplorerListBoxItem7.Content = $"RWY 1 Checkpoints ({rwy.Checkpoints.Count} checkpoints)";
                            ExplorerListBoxItem7.Visibility = Visibility.Visible;
                            ExplorerListBoxItem7.Foreground = _visibleBrush;
                            ModelHandler.RunwayView.RenderCheckpoints = true;
                            break;
                        case 8:
                            ExplorerListBoxItem8.Content = $"RWY 2 Road ({rwy.RoadTris.Count} tris)";
                            ExplorerListBoxItem8.Visibility = Visibility.Visible;
                            ExplorerListBoxItem8.Foreground = _visibleBrush;
                            ModelHandler.RunwayView.RenderRoad = true;
                            break;
                        case 9:
                            ExplorerListBoxItem9.Content = $"RWY 3 Boundaries ({rwy.BoundaryVerts.Count} verts)";
                            ExplorerListBoxItem9.Visibility = Visibility.Visible;
                            ExplorerListBoxItem9.Foreground = _visibleBrush;
                            ModelHandler.RunwayView.RenderBoundaries = true;
                            break;
                    }
                }

                ModelHandler.RunwayView.Render();
                UpdateTitle();
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
                    StatusText.Text = $"Error opening autodrive: {ex.Message}";
                    MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                for (byte m = 10; m < 17; m++)
                {
                    if (modelVisibility.ContainsKey(m))
                        modelVisibility[m] = true;
                    else
                        modelVisibility.Add(m, true);


                    switch (m)
                    {
                        case 10:
                            ExplorerListBoxItem10.Content = $"AD  0 Line ({ad.EnemyLine.AutoDriveInfos[0].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem10.Visibility = Visibility.Visible;
                            ExplorerListBoxItem10.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderLine1 = true;
                            break;
                        case 11:
                            ExplorerListBoxItem11.Content = $"AD  1 Left Lane ({ad.EnemyLine.AutoDriveInfos[1].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem11.Visibility = Visibility.Visible;
                            ExplorerListBoxItem11.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderLeftLane = true;
                            break;
                        case 12:
                            ExplorerListBoxItem12.Content = $"AD  2 Right Lane ({ad.EnemyLine.AutoDriveInfos[2].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem12.Visibility = Visibility.Visible;
                            ExplorerListBoxItem12.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderRightLane = true;
                            break;
                        case 13:
                            ExplorerListBoxItem13.Content = $"AD  3 Pit Exit Lane (?) ({ad.EnemyLine.AutoDriveInfos[3].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem13.Visibility = Visibility.Visible;
                            ExplorerListBoxItem13.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderPitExitLane = true;
                            break;
                        case 14:
                            ExplorerListBoxItem14.Content = $"AD  4 Restricted Area / Pit ({ad.EnemyLine.AutoDriveInfos[4].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem14.Visibility = Visibility.Visible;
                            ExplorerListBoxItem14.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderRestrictedArea = true;
                            break;
                        case 15:
                            ExplorerListBoxItem15.Content = $"AD  5 Learning Section ({ad.EnemyLine.AutoDriveInfos[5].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem15.Visibility = Visibility.Visible;
                            ExplorerListBoxItem15.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderLearningSection = true;
                            break;
                        case 16:
                            ExplorerListBoxItem16.Content = $"AD  6 Default Line ({ad.EnemyLine.AutoDriveInfos[6].AttackInfos.Count} Attack Points)";
                            ExplorerListBoxItem16.Visibility = Visibility.Visible;
                            ExplorerListBoxItem16.Foreground = _visibleBrush;
                            ModelHandler.AutodriveView.RenderDefaultLane = true;
                            break;
                    }

                }

                ModelHandler.AutodriveView.Render();
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
                    StatusText.Text = $"Error opening old runway: {ex.Message}";
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
            _editModelIndex = _editModelIndex == index ? (short)-1 : index;
            UpdateTrackModel();
            ExplorerVisibleRecalculate();
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
            View1.CameraMode = (bool)RotateMode_Checkbox.IsChecked ? CameraMode.Inspect : CameraMode.WalkAround;
        }

        #endregion

        private void ReflectConfig()
        {
            TrackEditorConfig.TryGetBool("EnableGrid", out bool val, true);
            ToggleGrid_Checkbox.IsChecked = val;
            Grid3D.IsRendering = val;

            TrackEditorConfig.TryGetBool("RotateMode", out val, false);
            RotateMode_Checkbox.IsChecked = val;
            View1.CameraMode = val ? CameraMode.Inspect : CameraMode.WalkAround;

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
            for (byte i = 0; i < ExplorerListBox.Items.Count; i++)
            {
                ListBoxItem lbi = ExplorerListBox.Items[i] as ListBoxItem;
                if (_editModelIndex == i)
                {
                    if (modelVisibility.ContainsKey(i) && modelVisibility[i])
                        lbi.Foreground = _editBrush;
                    else
                        lbi.Foreground = _editHiddenBrush;
                }
                else
                {
                    if (modelVisibility.ContainsKey(i) && modelVisibility[i])
                        lbi.Foreground = _visibleBrush;
                    else
                        lbi.Foreground = _hiddenBrush;
                }
            }
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
                                if (_editModelIndex == m)
                                {
                                    for (ushort n = 0; n < mesh.Tris.Count; n++)
                                    {
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].A] / 50);
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].B] / 50);
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].C] / 50);
                                    }
                                }
                                else if (_editModelIndex != -1)
                                {
                                    for (ushort n = 0; n < mesh.Tris.Count; n++)
                                    {
                                        noneditTrilist.Add(mesh.Verts[mesh.Tris[n].A] / 50);
                                        noneditTrilist.Add(mesh.Verts[mesh.Tris[n].B] / 50);
                                        noneditTrilist.Add(mesh.Verts[mesh.Tris[n].C] / 50);
                                    }
                                }
                                else
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
                }

                if (ModelHandler.RunwayView.Loaded())
                {
                    ModelHandler.RunwayView.Render();
                }

                if (ModelHandler.AutodriveView.Loaded())
                {
                    ModelHandler.AutodriveView.Render();
                }

                ModelHandler.Trilists(trilist, noneditTrilist, editTrilist);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error rendering track model: {ex.Message}";
                MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
