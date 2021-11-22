using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;
using System.IO;

using GTTrackEditor.Components;
using GTTrackEditor.ModelEntities;
using GTTrackEditor.Interfaces;

using PDTools.Files.Courses.Minimap;

namespace GTTrackEditor.Components.Minimap
{
    public class MinimapFaceComponent : TrackComponentBase, IModelCollection
    {
        public List<CourseMapFace> Faces { get; set; }

        public ObservableCollection<Element3D> Meshes { get; set; } = new();
        public DiffuseMaterial StartingGridMaterial { get; set; } = new();

        public bool IsSectorFace { get; set; }
        public bool IsPitlaneFace { get; set; }
        public bool IsLine { get; set; }

        public MinimapFaceComponent()
        {
            StartingGridMaterial.DiffuseColor = new(0.404f, 0.404f, 0.404f, 1f);
        }

        public void Init(List<CourseMapFace> faces)
        {
            Faces = faces;
        }

        public override void RenderComponent()
        {
            if (IsSectorFace)
                BuildSectors();
            else if (IsPitlaneFace)
                BuildPitlane();
            else if (IsLine)
                BuildLine();
            else
            {
                BuildPoints();
            }

        }

        private void BuildLine()
        {
            StartingGridMaterial.DiffuseColor = new(1f, 1f, 1f, 1f);

            List<Vector3> buf = new List<Vector3>();
            for (int i = 0; i < Faces.Count; i++)
            {
                MeshBuilder meshBuilder = new(false, false);
                Color4Collection colors = new();

                var face = Faces[i];
                for (int j = 0; j < face.Points.Count; j++)
                {
                    var point = face.Points[j];
                    Vector3 current = new Vector3((float)point.X * 0.25f, (float)point.Y * 0.25f, (float)point.Z * 0.25f);

                    buf.Add(current);
                }

                
                meshBuilder.AddTube(buf, 3f, 18, false);
                var mesh = new CourseMapFaceModelEntity()
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = StartingGridMaterial,

                    Face = face,
                    IsHitTestVisible = false,
                };

                Meshes.Add(mesh);
                buf.Clear();
            }

            /* 
            for (int i = 0; i < Faces.Count; i++)
            {
                LineBuilder lineBuilder = new LineBuilder();
            
                Color4Collection colors = new();
                Vector3 last = Vector3.Zero;
            
                var face = Faces[i];
                for (int j = 0; j < face.Points.Count; j++)
                {
                    var point = face.Points[j];
                    Vector3 current = new Vector3((float)point.X * 0.25f, (float)point.Y * 0.25f, (float)point.Z * 0.25f);
            
                    if (last != Vector3.Zero)
                        lineBuilder.AddLine(last, current);
            
                    last = current;
                }
                var mesh = new LineGeometryModel3D()
                {
                    Geometry = lineBuilder.ToLineGeometry3D(),
                    Color = System.Windows.Media.Colors.White,
            
                    IsHitTestVisible = false,
                };
            
                Meshes.Add(mesh);
            }
            */
        }

        private void BuildPitlane()
        {
            StartingGridMaterial.DiffuseColor = new(1f, 1f, 0f, 1f);
            List<Vector3> buf = new List<Vector3>();

            // TODO: Don't use spheres, link instead
            for (int i = 0; i < Faces.Count; i++)
            {
                var face = Faces[i];
                MeshBuilder meshBuilder = new(false, false);
                Color4Collection colors = new();

                for (int j = 0; j < face.Points.Count; j++)
                {
                    var point = face.Points[j];
                    Vector3 current = new Vector3((float)point.X * 0.25f, (float)point.Y * 0.25f, (float)point.Z * 0.25f);

                    meshBuilder.AddSphere(current, radius: 0.5f);
                }

                var mesh = new CourseMapFaceModelEntity()
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = StartingGridMaterial,

                    Face = face,
                    IsHitTestVisible = false,
                };

                Meshes.Add(mesh);
                buf.Clear();
            }
        }

        private void BuildSectors()
        {
            StartingGridMaterial.DiffuseColor = new(0.404f, 0.404f, 0.404f, 1f);

            List<Vector3> buf = new List<Vector3>(4);
            Vector3 last = Vector3.Zero;
            for (int i = 0; i < Faces.Count; i++)
            {
                var face = Faces[i];
                MeshBuilder meshBuilder = new(false, false);
                Color4Collection colors = new();

                for (int j = 0; j < face.Points.Count; j++)
                {
                    var point = face.Points[j];

                    Vector3 current = new Vector3((float)point.X * 0.25f, (float)point.Y * 0.25f, (float)point.Z * 0.25f);

                    if (j == 0)
                    {
                        buf.Add(current);
                        last = current;
                        continue;
                    }

                    float dist = Vector3.Distance(last, current);
                    if (dist < 20.0f)
                    {
                        buf.Add(current);
                    }
                    else
                    {
                        ProcessSector(meshBuilder, buf);
                        buf.Add(current); // Don't forget
                    }

                    last = current;
                }

                ProcessSector(meshBuilder, buf);

                var mesh = new CourseMapFaceModelEntity()
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = StartingGridMaterial,

                    Face = face,
                    IsHitTestVisible = false,
                };

                Meshes.Add(mesh);
                buf.Clear();
            }
        }

        private void BuildPoints()
        {
            StartingGridMaterial.DiffuseColor = new(1f, 1f, 1f, 1f);
            List<Vector3> buf = new List<Vector3>();

            // TODO: Don't use spheres, link instead
            for (int i = 0; i < Faces.Count; i++)
            {
                var face = Faces[i];
                MeshBuilder meshBuilder = new(false, false);
                Color4Collection colors = new();

                for (int j = 0; j < face.Points.Count; j++)
                {
                    var point = face.Points[j];
                    Vector3 current = new Vector3((float)point.X * 0.25f, (float)point.Y * 0.25f, (float)point.Z * 0.25f);

                    meshBuilder.AddSphere(current, radius: 0.5f);
                }

                var mesh = new CourseMapFaceModelEntity()
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = StartingGridMaterial,

                    Face = face,
                    IsHitTestVisible = false,
                };

                Meshes.Add(mesh);
                buf.Clear();
            }
        }

        private void ProcessSector(MeshBuilder meshBuilder, List<Vector3> buf)
        {
            if (buf.Count == 4)
            {
                meshBuilder.AddQuad(buf[0], buf[2], buf[3], buf[1]);
                buf.Clear();
            }
            else if (buf.Count == 2)
            {
                meshBuilder.AddTube(buf, 5f, 18, true); // TODO: Change to something else
            }
            buf.Clear();
        }

        public override void Hide()
        {
            if (!IsVisible)
                return;

            foreach (Element3D i in Meshes)
                (i as CourseMapFaceModelEntity).Hide();
            TreeViewItemColor = Brushes.Gray;
            IsVisible = false;
        }

        public override void Show()
        {
            if (IsVisible)
                return;

            foreach (Element3D i in Meshes)
                (i as CourseMapFaceModelEntity).Show();
            TreeViewItemColor = Brushes.White;
            IsVisible = true;
        }

        public void AddNew()
        {
            throw new NotImplementedException();
        }

        public void Remove(Element3D element)
        {
            CourseMapFaceModelEntity model = element as CourseMapFaceModelEntity;
            Faces.Remove(model.Face);

            Meshes.Remove(element);
        }
    }
}
