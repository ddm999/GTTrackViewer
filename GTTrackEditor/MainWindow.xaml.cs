using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;
using System.Text;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

using PDTools.Files.Textures.PS3;
using PDTools.Files.Courses.Runway;
using PDTools.Files.Courses.AutoDrive;
using PDTools.Files.Courses.Minimap;
using PDTools.Files.Courses.PS3;
using PDTools.Files.Models.PS3.ModelSet3.ShapeStream;
using PDTools.Files.Models.PS3.ModelSet3;
using PDTools.Files.Models.ShapeStream;
using GTTrackEditor.Utils;

using ICSharpCode.SharpZipLib.Zip.Compression;
using Syroot.BinaryData;
using SharpDX.Win32;

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

        private async void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Course Data Files (c***x)|*.*|" +
                "Runway Files|*.rwy|" +
                "Autodrive Files|*.ad|" +
                "Course Map Files|*.map|" +
                "ShapeStream Files |*.shapestream";

            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                LoadingBar.Visibility = Visibility.Visible;
                try
                {
                    // TODO: Dispose stream when done!

                    if (openFileDialog.FileName.EndsWith(".rwy") || openFileDialog.FileName.Contains("runway", StringComparison.OrdinalIgnoreCase))
                        await HandleRunwayAsync(openFileDialog.FileName);
                    else if (openFileDialog.FileName.EndsWith(".map"))
                        await HandleMinimapAsync(openFileDialog.FileName);
                    else if (openFileDialog.FileName.EndsWith(".ad"))
                    {
                    }
                    else if (openFileDialog.FileName.EndsWith("x"))
                        await HandleCourseDataAsync(openFileDialog.FileName);
                    else if (openFileDialog.FileName.EndsWith(".shapestream"))
                        await HandleShapeStreamAsync(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error opening file: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                    LoadingBar.Visibility = Visibility.Collapsed;
                }
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

        private async void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item is null)
                return;

            if (item.Header is ModelEntities.ModelSetMeshEntity meshEntity && meshEntity.IsHitTestVisible)
            {
                ModelHandler.SetEditTarget(meshEntity);
                var diffuseEntry = meshEntity.MaterialEntry?.ImageEntries.Find(e => e.SamplerName == "diffuseMapSampler");
                if (diffuseEntry?.TextureInfo != null)
                    await LoadTexturePreviewAsync(diffuseEntry, meshEntity);
                else
                    ModelHandler.TexturePreviewSource = null;
            }
            else if (item.Header is Element3D elem && elem.IsHitTestVisible)
                ModelHandler.SetEditTarget(item.Header);
            else if (item.Header is GTTrackEditor.Components.ModelSet.ModelSetMaterialEntry matEntry)
            {
                ModelHandler.SetPropertyTarget(new ModelEntities.MaterialPropertyView(matEntry.Material));
                var diffuseEntry = matEntry.ImageEntries.Find(e => e.SamplerName == "diffuseMapSampler");
                if (diffuseEntry?.TextureInfo != null)
                    await LoadTexturePreviewAsync(diffuseEntry);
                else
                    ModelHandler.TexturePreviewSource = null;
            }
            else if (item.Header is GTTrackEditor.Components.ModelSet.ResolvedTextureEntry texEntry && texEntry.TextureInfo != null)
            {
                ModelHandler.SetPropertyTarget(new ModelEntities.TextureInfoPropertyView(texEntry.SamplerName, texEntry.TextureInfo));
                await LoadTexturePreviewAsync(texEntry);
            }
        }

        private async Task LoadTexturePreviewAsync(
            GTTrackEditor.Components.ModelSet.ResolvedTextureEntry texEntry,
            ModelEntities.ModelSetMeshEntity meshEntity = null)
        {
            byte[] ddsBytes = await Task.Run(() => FetchTextureDds(texEntry));
            if (ddsBytes is null) return;

            var (pixelData, width, height, pixelFormat, stride) = await Task.Run(() =>
            {
                using var ms = new MemoryStream(ddsBytes);
                var dds = Pfim.Pfimage.FromStream(ms);
                var fmt = dds.Format switch
                {
                    Pfim.ImageFormat.Rgba32 => PixelFormats.Bgra32,
                    Pfim.ImageFormat.Rgb24  => PixelFormats.Bgr24,
                    _ => throw new NotSupportedException($"Unsupported Pfim format: {dds.Format}")
                };
                return (dds.Data.ToArray(), dds.Width, dds.Height, fmt, dds.Stride);
            });

            var textureBitmap = BitmapSource.Create(width, height, 96, 96, pixelFormat, null, pixelData, stride);
            textureBitmap.Freeze();

            if (meshEntity?.Geometry is HelixToolkit.SharpDX.Core.MeshGeometry3D mesh3D &&
                mesh3D.TextureCoordinates?.Count > 0 && mesh3D.Indices?.Count > 0)
            {
                ModelHandler.TexturePreviewSource = DrawUVOverlay(textureBitmap, mesh3D.TextureCoordinates, mesh3D.Indices);
            }
            else
            {
                ModelHandler.TexturePreviewSource = textureBitmap;
            }
        }

        private static BitmapSource DrawUVOverlay(
            BitmapSource texture,
            HelixToolkit.SharpDX.Core.Vector2Collection uvCoords,
            HelixToolkit.SharpDX.Core.IntCollection indices)
        {
            int w = texture.PixelWidth;
            int h = texture.PixelHeight;

            var geo = new System.Windows.Media.StreamGeometry();
            using (var ctx = geo.Open())
            {
                for (int i = 0; i + 2 < indices.Count; i += 3)
                {
                    int a = indices[i], b = indices[i + 1], c = indices[i + 2];
                    if (a >= uvCoords.Count || b >= uvCoords.Count || c >= uvCoords.Count) continue;
                    ctx.BeginFigure(new System.Windows.Point(uvCoords[a].X * w, uvCoords[a].Y * h), isFilled: false, isClosed: true);
                    ctx.LineTo(new System.Windows.Point(uvCoords[b].X * w, uvCoords[b].Y * h), isStroked: true, isSmoothJoin: false);
                    ctx.LineTo(new System.Windows.Point(uvCoords[c].X * w, uvCoords[c].Y * h), isStroked: true, isSmoothJoin: false);
                }
            }
            geo.Freeze();

            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 220, 0)), 1.0);
            pen.Freeze();

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(texture, new Rect(0, 0, w, h));
                dc.DrawGeometry(null, pen, geo);
            }

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            return rtb;
        }

        private static byte[] FetchTextureDds(GTTrackEditor.Components.ModelSet.ResolvedTextureEntry texEntry)
        {
            var modelSet = texEntry.OwnerModelSet;
            var info = texEntry.TextureInfo;

            var bufferInfo = (CellTextureBuffer)modelSet.TextureSet.Buffers[(int)info.ImageId];
            info.BufferInfo = bufferInfo;

            if (bufferInfo.ImageOffset == 0 && bufferInfo.ImageSize == 0) return null;

            long vramStartPos = modelSet.ParentCourseData?.Entries[1].DataStart ?? 0;
            byte origMip = info.MipmapLevelLast;
            info.MipmapLevelLast = 1;
            try
            {
                return modelSet.TextureSet.GetExternalImageDataOfTexture(modelSet.Stream, info, vramStartPos);
            }
            finally
            {
                info.MipmapLevelLast = origMip;
            }
        }

        private void ScriptMenu_Click(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            try
            {
#endif
                var script = (e.OriginalSource as MenuItem)?.DataContext as Scripts.ScriptBase;
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

        private async Task HandleRunwayAsync(string fileName)
        {
            if (ModelHandler.RunwayView.Loaded())
            {
                RunwayFile runway_other = await Task.Run(() => {
                    using var stream = new FileStream(fileName, FileMode.Open);
                    return RunwayFile.FromStream(stream);
                });
                ModelHandler.RunwayView.RunwayData.Merge(runway_other);
                ModelHandler.RunwayView.Init();
                await ModelHandler.RunwayView.RenderAsync();
                return;
            }

            RunwayFile runway = await Task.Run(() => {
                using var stream = new FileStream(fileName, FileMode.Open);
                return RunwayFile.FromStream(stream);
            });
            ModelHandler.RunwayView.SetRunwayData(runway);
            _rwyFileName = fileName;

            if (!ModelHandler.Views.Contains(ModelHandler.RunwayView))
                ModelHandler.Views.Add(ModelHandler.RunwayView);

            ModelHandler.RunwayView.FileName = Path.GetFileNameWithoutExtension(fileName);
            ModelHandler.RunwayView.Init();
            await ModelHandler.RunwayView.RenderAsync();
        }

        private async Task HandleMinimapAsync(string fileName)
        {
            CourseMapFile minimap = await Task.Run(() => {
                using var stream = new FileStream(fileName, FileMode.Open);
                return CourseMapFile.FromStream(stream);
            });
            ModelHandler.MinimapView.SetMinimapData(minimap);

            if (!ModelHandler.Views.Contains(ModelHandler.MinimapView))
                ModelHandler.Views.Add(ModelHandler.MinimapView);

            ModelHandler.MinimapView.FileName = Path.GetFileNameWithoutExtension(fileName);
            ModelHandler.MinimapView.Init();
            await ModelHandler.MinimapView.RenderAsync();
        }

        private async Task HandleCourseDataAsync(string fileName)
        {
            if (ModelHandler.CourseDataView.Loaded())
                ModelHandler.CourseDataView.Unload(_viewport);

            CourseDataFile courseData = await Task.Run(() => CourseDataFile.Open(fileName));
            ModelHandler.CourseDataView.SetCourseData(courseData);
            _courseDataFileName = fileName;

            if (!ModelHandler.Views.Contains(ModelHandler.CourseDataView))
                ModelHandler.Views.Add(ModelHandler.CourseDataView);

            ModelHandler.CourseDataView.Init();
            await ModelHandler.CourseDataView.RenderAsync();

            foreach (var component in ModelHandler.CourseDataView.ModelSetComponent.ModelComponents)
            {
                var group = new GroupModel3D();
                group.ItemsSource = component.MeshEntities;
                _viewport.Items.Add(group);
                ModelHandler.CourseDataView.ModelSetComponent.RenderingGroups.Add(group);
            }
        }

        private async Task HandleShapeStreamAsync(string fileName)
        {
            if (!ModelHandler.Views.Contains(ModelHandler.CourseDataView))
                throw new NotSupportedException("A Course Data file must first be loaded to load ShapeStream data!");

            ModelSet3 modelSet = ModelHandler.CourseDataView.CourseData.MainModelSet;

            var ss = await Task.Run(() => {
                using var stream = new FileStream(fileName, FileMode.Open);
                return ShapeStream.FromStream(stream, modelSet);
            });
            modelSet.ShapeStream = ss;

            ModelHandler.CourseDataView.Init();
            await ModelHandler.CourseDataView.RenderAsync();

            foreach (var component in ModelHandler.CourseDataView.ModelSetComponent.ModelComponents)
            {
                var group = new GroupModel3D();
                group.ItemsSource = component.MeshEntities;
                _viewport.Items.Add(group);
                ModelHandler.CourseDataView.ModelSetComponent.RenderingGroups.Add(group);
            }
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Wavefront .obj geometry format|*.obj";

            if (saveFileDialog.ShowDialog() == false)
                return;

            using var file = File.Open(saveFileDialog.FileName, FileMode.Create);
            StreamWriter sw = new(file);

            ModelSet3 mdl = ModelHandler.CourseDataView.CourseData.MainModelSet;

            var baseVerts = 1;
            for (short i = 0; i < mdl.Shapes.Count; i++)
            {
                var mesh = mdl.Shapes[i];

                var verts = mdl.GetVerticesOfShape((ushort)i);
                var tris = mdl.GetTrisOfMesh((ushort)i);

                if (tris is null || tris.Count == 0)
                    continue; // Most likely tristrip - not supported for now

                sw.WriteLine($"o mesh{i}");

                for (int j = 0; j < verts.Length; j++)
                {
                    sw.WriteLine($"v {verts[j].X} {verts[j].Y} {verts[j].Z}");
                }

                for (int j = 0; j < tris.Count; j++)
                {
                    sw.WriteLine($"f {tris[j].A + baseVerts} {tris[j].B + baseVerts} {tris[j].C + baseVerts}");
                }

                baseVerts += verts.Length;
            }

            sw.Close();
        }

        private void ShapeStream_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ShapeStream export is extremely prone-to-breaking and exports everything into a single mesh!\n" +
                            "The output MUST be used with edited dynamic_sky_spa (c227) files, as it assumes the FVF format from there!\n" +
                            "This is absolutely NOT guaranteed to work! You have been warned!",
                            "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "GT6 .shapestream streaming geometry format|*.shapestream";

            if (saveFileDialog.ShowDialog() == false)
                return;

            byte[] rawBuffer = new byte[0x100000];
            MemoryStream ms = new MemoryStream(rawBuffer);
            BinaryStream bs = new BinaryStream(ms, ByteConverter.Big);

            var mdl = ModelHandler.CourseDataView.CourseData.MainModelSet;

            List<string> infos = new();
            for (short i = 0; i < mdl.Shapes.Count; i++)
            {
                infos.Add($"Mesh {i}:");
                var baseOffset = bs.Position;
                infos.Add($"  OffsetWithinShapeStream: {baseOffset:X}h");
                // TODO: Optimize this
                var verts = mdl.GetVerticesOfShape((ushort)i);
                var tris = mdl.GetTrisOfMesh((ushort)i);
                var uvs = mdl.GetUVsOfMesh((ushort)i);
                var bbox = mdl.GetBBoxOfShape((ushort)i);
                var norms = mdl.GetNormalsOfShape((ushort)i);

                // leave space for header
                bs.Position = baseOffset + 0x80;

                infos.Add($"  FVFIndex: 0");
                infos.Add($"  MaterialIndex: 0");

                // STEP 1: vertices
                for (var j = 0; j < verts.Length; j++)
                {
                    bs.WriteSingle(verts[j].X); // position
                    bs.WriteSingle(verts[j].Y);
                    bs.WriteSingle(verts[j].Z);

                    uint normValue = ((norms[j].Item3 & 0b111_11111111) << 21) | ((norms[j].Item2 & 0b111_11111111) << 10) | (norms[j].Item1 & 0b11_11111111);
                    bs.WriteUInt32(normValue); // normal
                    if (i == 0)
                    {
                        bs.WriteSingle(uvs[j].X); // uv
                        bs.WriteSingle(uvs[j].Y);
                    }
                }

                infos.Add($"  VertCount: {verts.Length}");

                // STEP 2: tris
                bs.Align(0x80);
                var triOffset = bs.Position - baseOffset;

                foreach (var t in tris)
                {
                    bs.WriteUInt16(t.A);
                    bs.WriteUInt16(t.B);
                    bs.WriteUInt16(t.C);
                }
                infos.Add($"  TriLength: {tris.Count * 3}");
                infos.Add($"  TriCount: {tris.Count}");

                // STEP 3: bbox
                bs.Align(0x10);
                var bboxOffset = bs.Position - baseOffset;
                foreach (var v in bbox)
                {
                    bs.WriteSingle(v.X);
                    bs.WriteSingle(v.Y);
                    bs.WriteSingle(v.Z);
                }

                // STEP 4: header
                var meshSize = bs.Position - baseOffset;

                bs.Position = baseOffset + 0x0;
                bs.WriteUInt32((uint)meshSize);
                bs.Position = baseOffset + 0xC;
                bs.WriteUInt32(0x80); // verticesOffset
                bs.WriteUInt32((uint)triOffset);
                bs.Position = baseOffset + 0x18;
                bs.WriteUInt32((uint)bboxOffset);

                bs.Position = baseOffset + meshSize;
                bs.Align(0x80);
            }

            // STEP 5: deflate
            byte[] outBuffer = new byte[0x100000];
            var deflater = new Deflater(-1, true);
            deflater.SetInput(rawBuffer, 0, (int)bs.Position);
            deflater.Finish();
            var dataLength = deflater.Deflate(outBuffer, 0, 0x100000);

            byte[] outData = new byte[dataLength];
            Array.Copy(outBuffer, outData, dataLength);
            infos.Add($"ShapeStreamDataSize: {dataLength:X}h");
            infos.Add($"ShapeStreamMeshCount: {mdl.Shapes.Count}");

            using var file = File.Open(saveFileDialog.FileName, FileMode.Create);
            file.Write(outData);

            var infoPath = Path.ChangeExtension(saveFileDialog.FileName, ".shapeinfo");
            using var infoFile = File.Open(infoPath, FileMode.Create);
            {
                using StreamWriter sw = new(infoFile);
                foreach (var s in infos)
                    sw.WriteLine(s);
            }

            //MessageBox.Show("Created ShapeStream.\n" +
            //                $"Course data: {baseVerts} verts / {allTris.Count * 3} triLen / " +
            //                $"{allTris.Count} tris / {dataLength:X}h ShapeStreamDataSize");
        }
    }
}
