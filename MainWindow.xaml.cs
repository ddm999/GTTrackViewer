using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Input;
using SharpDX;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf.SharpDX;

namespace GTTrackEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string packFileName;
        PACB packFile;
        string rwyFileName;
        RNW5 runway;
        Dictionary<byte, bool> modelVisibility = new();
        short editModel = -1;
        int editMesh = -1;
        int editVertex = -1;

        private readonly Brush visibleBrush = new SolidColorBrush(Colors.Black);
        private readonly Brush editBrush = new SolidColorBrush(Colors.Blue);
        private readonly Brush editHiddenBrush = new SolidColorBrush(Colors.LightBlue);
        private readonly Brush hiddenBrush = new SolidColorBrush(Colors.LightGray);

        private void LoadPack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                packFileName = openFileDialog.FileName;
                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(packFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    packFile = PACB.FromStream(ref sr);
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

                    if (packFile.Models.ContainsKey(m))
                    {
                        switch (m)
                        {
                            case 0:
                                ExplorerListBoxItemTrack.Content = $"MDL 0 Track ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItemTrack.Visibility = Visibility.Visible;
                                ExplorerListBoxItemTrack.Foreground = visibleBrush;
                                break;
                            case 1:
                                ExplorerListBoxItem1.Content = $"MDL 1 Skydome A ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItem1.Visibility = Visibility.Visible;
                                ExplorerListBoxItem1.Foreground = hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 2:
                                ExplorerListBoxItem2.Content = $"MDL 2 Distant Terrain ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItem2.Visibility = Visibility.Visible;
                                ExplorerListBoxItem2.Foreground = visibleBrush;
                                break;
                            case 3:
                                ExplorerListBoxItem3.Content = $"MDL 3 Mirrors Track ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItem3.Visibility = Visibility.Visible;
                                ExplorerListBoxItem3.Foreground = hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 4:
                                ExplorerListBoxItem4.Content = $"MDL 4 Skydome B ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItem4.Visibility = Visibility.Visible;
                                ExplorerListBoxItem4.Foreground = hiddenBrush;
                                modelVisibility[m] = false;
                                break;
                            case 5:
                                ExplorerListBoxItem5.Content = $"MDL 5 ({packFile.Models[m].MeshCount} meshes / {packFile.Models[m].TriCount} tris)";
                                ExplorerListBoxItem5.Visibility = Visibility.Visible;
                                ExplorerListBoxItem5.Foreground = visibleBrush;
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
            if (openFileDialog.ShowDialog() == true)
            {
                rwyFileName = openFileDialog.FileName;
                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(rwyFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    runway = RNW5.FromStream(ref sr);
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
                            ExplorerListBoxItem6.Content = $"RWY 0 Positions ({runway.StartingGrid.Count} starts, {runway.PitStops.Count} pits, {runway.PitStopAdjacents.Count} pit adjacent)";
                            ExplorerListBoxItem6.Visibility = Visibility.Visible;
                            ExplorerListBoxItem6.Foreground = visibleBrush;
                            break;
                        case 7:
                            ExplorerListBoxItem7.Content = $"RWY 1 Checkpoints ({runway.Checkpoints.Count} checkpoints)";
                            ExplorerListBoxItem7.Visibility = Visibility.Visible;
                            ExplorerListBoxItem7.Foreground = visibleBrush;
                            break;
                        case 8:
                            ExplorerListBoxItem8.Content = $"RWY 2 Road ({runway.RoadTris.Count} tris)";
                            ExplorerListBoxItem8.Visibility = Visibility.Visible;
                            ExplorerListBoxItem8.Foreground = visibleBrush;
                            break;
                        case 9:
                            ExplorerListBoxItem9.Content = $"RWY 3 Boundaries ({runway.BoundaryVerts.Count} verts)";
                            ExplorerListBoxItem9.Visibility = Visibility.Visible;
                            ExplorerListBoxItem9.Foreground = visibleBrush;
                            break;
                    }
                }

                UpdateTrackModel();
                UpdateTitle();
            }
        }

        private void UpdateTitle()
        {
            if (packFileName != null)
            {
                if (rwyFileName != null)
                    Title = $"GT Track Editor ( {packFileName.Split("\\")[^1]} / {rwyFileName.Split("\\")[^1]} )";
                else
                    Title = $"GT Track Editor ( {packFileName.Split("\\")[^1]} )";
            }
            else
            {
                if (rwyFileName != null)
                    Title = $"GT Track Editor ( {rwyFileName.Split("\\")[^1]} )";
                else
                    Title = "GT Track Editor";
            }
        }

        private void ExplorerVisibleRecalculate()
        {
            for (byte i = 0; i < ExplorerListBox.Items.Count; i++)
            {
                ListBoxItem lbi = ExplorerListBox.Items[i] as ListBoxItem;
                if (editModel == i)
                {
                    if (modelVisibility.ContainsKey(i) && modelVisibility[i])
                        lbi.Foreground = editBrush;
                    else
                        lbi.Foreground = editHiddenBrush;
                }
                else
                {
                    if (modelVisibility.ContainsKey(i) && modelVisibility[i])
                        lbi.Foreground = visibleBrush;
                    else
                        lbi.Foreground = hiddenBrush;
                }
            }
        }

        private void ExplorerVisibleToggle(ListBoxItem lbi, byte i)
        {
            if (modelVisibility[i])
            {
                modelVisibility[i] = false;
                lbi.Foreground = lbi.Foreground == editBrush ? editHiddenBrush : hiddenBrush;
            }
            else
            {
                modelVisibility[i] = true;
                lbi.Foreground = lbi.Foreground == editHiddenBrush ? editBrush : visibleBrush;
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

                if (packFile != null)
                {
                    for (byte m = 0; m < 6; m++)
                    {
                        if (!modelVisibility[m] || !packFile.Models.ContainsKey(m))
                            continue;

                        for (ushort i = 0; i < packFile.Models[m].MeshCount; i++)
                        {
                            /*if (modelVisibility[m].Item2[i] == false)
                                continue;*/

                            MDL3Mesh mesh = packFile.Models[m].Meshes[i];
                            if (mesh.Tristrip == false)
                            {
                                if (editModel == m)
                                {
                                    for (ushort n = 0; n < mesh.Tris.Count; n++)
                                    {
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].A] / 50);
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].B] / 50);
                                        editTrilist.Add(mesh.Verts[mesh.Tris[n].C] / 50);
                                    }
                                }
                                else if (editModel != -1)
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

                if (runway != null)
                {
                    // model 6: RWY Vec3Rs
                    if (modelVisibility[6])
                    {
                        if (runway.StartingGrid != null)
                            ModelHandler.GreenModel = ModelHandler.Vec3Rs(runway.StartingGrid);
                        if (runway.PitStops != null)
                            ModelHandler.RedModel = ModelHandler.Vec3Rs(runway.PitStops);
                        if (runway.PitStopAdjacents != null)
                            ModelHandler.YellowModel = ModelHandler.Vec3Rs(runway.PitStopAdjacents);
                    }
                    else
                    {
                        ModelHandler.GreenModel = new();
                        ModelHandler.RedModel = new();
                        ModelHandler.YellowModel = new();
                    }

                    // model 7: RWY Checkpoints
                    if (modelVisibility[7] && runway.Checkpoints != null)
                        ModelHandler.Checkpoints(runway.Checkpoints);
                    else
                        ModelHandler.BlueModel = new();

                    // model 8: RWY Road
                    if (modelVisibility[8] && runway.RoadVerts != null)
                        ModelHandler.RoadSurface(runway.RoadTris, runway.RoadVerts);
                    else
                        ModelHandler.RoadModel = new();

                    // model 9: RWY Boundaries
                    if (modelVisibility[9] && runway.BoundaryVerts != null)
                    {
                        int i = 0;
                        List<List<Vector3>> boundaries = new();
                        List<Vector3> boundary = new();
                        while (i < runway.BoundaryVerts.Count)
                        {
                            RNW5BoundaryVert vert = runway.BoundaryVerts[i];
                            boundary.Add(vert.ToVector3() / 50);

                            if (vert.counter < 0) // boundary end
                            {
                                boundaries.Add(boundary);
                                boundary = new();
                            }
                            i++;
                        }
                        ModelHandler.Boundaries(boundaries);
                        Line3D.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Line3D.Visibility = Visibility.Hidden;
                    }
                }

                ModelHandler.Trilists(trilist, noneditTrilist, editTrilist);

                Edit3D.Geometry = ModelHandler.EditModel;
                Plain3D.Geometry = ModelHandler.PlainModel;
                Nonedit3D.Geometry = ModelHandler.NoneditModel;
                Red3D.Geometry = ModelHandler.RedModel;
                Green3D.Geometry = ModelHandler.GreenModel;
                Blue3D.Geometry = ModelHandler.BlueModel;
                Yellow3D.Geometry = ModelHandler.YellowModel;
                Road3D.Geometry = ModelHandler.RoadModel;
                Line3D.Geometry = ModelHandler.BoundaryModel;
                Grid3D.Geometry = ModelHandler.Grid;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error rendering track model: {ex.Message}";
                MessageBox.Show(this, $"{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            editModel = editModel == index ? (short)-1 : index;
            UpdateTrackModel();
            ExplorerVisibleRecalculate();
        }

        private void ExplorerContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = ((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem;
            byte id = ExplorerItemToIndex(lbi);
            Span<byte> span = File.ReadAllBytes(packFileName);
            SpanReader sr = new(span, endian: Endian.Big);
            SpanWriter sw = new(span, endian: Endian.Big);

            packFile.DeleteModel(ref sr, ref sw, id);
            File.WriteAllBytes(packFileName, span.ToArray());

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
            Grid3D.IsRendering = (bool)ToggleGrid.IsChecked;
        }

        private void RotateMode_Click(object sender, RoutedEventArgs e)
        {
            View1.CameraMode = (bool)RotateMode.IsChecked ? HelixToolkit.Wpf.SharpDX.CameraMode.Inspect : HelixToolkit.Wpf.SharpDX.CameraMode.WalkAround;
        }

        private void Edit3D_Mouse3DDown(object sender, MouseDown3DEventArgs e)
        {
            // ignore everything except left mouse
            if (MouseButton.Left != ((MouseButtonEventArgs)e.OriginalInputEventArgs).ChangedButton)
                return;



            //if (editMesh != -1 && editVertex != -1)
            //{
            //    ModelHandler.ManipulatedPos(hitPos);
            //    TempManipulated3D.Visibility = Visibility.Visible;
            //    TempManipulated3D.Geometry = ModelHandler.ManipulatedModel;
            //}
        }

        private void ImportRunway_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
            {
                rwyFileName = openFileDialog.FileName;

                try
                {
                    ReadOnlySpan<byte> span = File.ReadAllBytes(rwyFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    runway = runway.MergeFromStream(ref sr);
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
            SaveFileDialog saveFileDialog = new();
            if (saveFileDialog.ShowDialog() == true)
            {
                rwyFileName = saveFileDialog.FileName;

                try
                {
                    Span<byte> span = File.ReadAllBytes(rwyFileName);
                    SpanReader sr = new(span, endian: Endian.Big);
                    SpanWriter sw = new(span, endian: Endian.Big);
                    runway.ToStream(ref sr, ref sw);
                    File.WriteAllBytes(rwyFileName, span.ToArray());
                }
            }
        }
    }
}
