using DemoCore;
using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace GTTrackEditor
{
    class ModelHandler : BaseViewModel
    {
        static MeshBuilder meshBuilder;

        public static MeshGeometry3D PlainModel { get; set; }
        public static MeshGeometry3D NoneditModel { get; set; }
        public static MeshGeometry3D EditModel { get; set; }
        public static LineGeometry3D Grid { get; set; }
        public static MeshGeometry3D RedModel { get; set; }
        public static MeshGeometry3D GreenModel { get; set; }
        public static MeshGeometry3D BlueModel { get; set; }
        public static MeshGeometry3D YellowModel { get; set; }
        public static MeshGeometry3D BoundaryModel { get; set; }
        public static MeshGeometry3D RoadModel { get; set; }
        public static MeshGeometry3D ManipulatedModel { get; set; }
        public static DiffuseMaterial PlainMaterial { get; set; } = new();
        public static DiffuseMaterial NoneditMaterial { get; set; } = new();
        public static DiffuseMaterial EditMaterial { get; set; } = new();
        public static DiffuseMaterial RedMaterial { get; set; } = new();
        public static DiffuseMaterial GreenMaterial { get; set; } = new();
        public static DiffuseMaterial BlueMaterial { get; set; } = new();
        public static DiffuseMaterial YellowMaterial { get; set; } = new();
        public static VertColorMaterial VCMaterial { get; set; } = new();
        public Vector3D DirectionalLightDirection { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Color AmbientLightColor { get; private set; }
        public static List<Color4> SurfaceTypeColors { get; set; } = new();
        public static Element3D ManipulatorTarget { get; set; }

        public ModelHandler()
        {
            EffectsManager = new DefaultEffectsManager();

            Camera = new PerspectiveCamera
            {
                Position = new Point3D(3, 3, 3),
                LookDirection = new Vector3D(-3, -3, -3),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 500,
                NearPlaneDistance = 0.001
            };

            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;
            DirectionalLightDirection = new Vector3D(-2, -5, -2);

            PlainMaterial.DiffuseColor = new(1.0f, 1.0f, 1.0f, 1.0f);
            NoneditMaterial.DiffuseColor = new(0.8f, 0.8f, 0.8f, 0.4f);
            EditMaterial.DiffuseColor = new(1.0f, 0.8f, 0.0f, 1.0f);

            RedMaterial.DiffuseColor = new(1.0f, 0.0f, 0.0f, 1.0f);
            GreenMaterial.DiffuseColor = new(0.0f, 1.0f, 0.0f, 1.0f);
            BlueMaterial.DiffuseColor = new(0.0f, 0.0f, 1.0f, 1.0f);
            YellowMaterial.DiffuseColor = new(1.0f, 1.0f, 0.0f, 1.0f);

            Grid = LineBuilder.GenerateGrid(new Vector3(0, 1, 0), -25, 25, -25, 25);

            SurfaceTypeColors.Add(new(0.5f, 0.5f, 0.5f, 1.0f)); // TARMAC
            SurfaceTypeColors.Add(new(0.8f, 0.2f, 0.2f, 1.0f)); // GUIDE
            SurfaceTypeColors.Add(new(0.2f, 0.8f, 0.2f, 1.0f)); // GREEN
            SurfaceTypeColors.Add(new(0.7f, 0.6f, 0.5f, 1.0f)); // GRAVEL
            SurfaceTypeColors.Add(new(0.4f, 0.3f, 0.2f, 1.0f)); // DIRT
            SurfaceTypeColors.Add(new(0.4f, 0.5f, 0.8f, 1.0f)); // WATER
            SurfaceTypeColors.Add(new(0.7f, 0.7f, 0.7f, 1.0f)); // STONE
            SurfaceTypeColors.Add(new(0.4f, 0.25f, 0.2f, 1.0f)); // WOOD
            SurfaceTypeColors.Add(new(0.8f, 0.75f, 0.65f, 1.0f)); // PAVE
            SurfaceTypeColors.Add(new(0.2f, 0.5f, 0.2f, 1.0f)); // GUIDE1
            SurfaceTypeColors.Add(new(0.2f, 0.7f, 0.3f, 1.0f)); // GUIDE2
            SurfaceTypeColors.Add(new(0.3f, 0.7f, 0.2f, 1.0f)); // GUIDE3
            SurfaceTypeColors.Add(new(0.8f, 0.65f, 0.75f, 1.0f)); // PEBBLE
            SurfaceTypeColors.Add(new(0.8f, 0.8f, 0.2f, 1.0f)); // BEACH
        }

        public static MeshGeometry3D Vec3Rs(List<Vec3R> vec3rs)
        {
            meshBuilder = new(false, false);
            for (int i = 0; i < vec3rs.Count; i++)
            {
                Vector3 pos = vec3rs[i].ToVector3() / 50;
                float r = vec3rs[i].R;

                meshBuilder.AddSphere(pos, 0.025f);
                Vector3 pos2 = pos;
                pos2.X += (0 * MathF.Cos(-r)) - (0.1f * MathF.Sin(-r));
                pos2.Z += (0 * MathF.Sin(-r)) + (0.1f * MathF.Cos(-r));
                meshBuilder.AddArrow(pos, pos2, 0.01f, 3.0f, 18);
            }
            MeshGeometry3D model = meshBuilder.ToMesh();
            model.Normals = model.CalculateNormals();
            return model;
        }

        public static void Checkpoints(List<RNW5Checkpoint4> checkpoints)
        {
            meshBuilder = new(false, false);
            for (int i = 0; i < checkpoints.Count; i++)
            {
                meshBuilder.AddQuad(
                    checkpoints[i].left / 50,
                    checkpoints[i].middle / 50,
                    (checkpoints[i].middle / 50) + new Vector3(0.0f, 0.2f, 0.0f),
                    (checkpoints[i].left / 50) + new Vector3(0.0f, 0.2f, 0.0f)
                    );
                meshBuilder.AddQuad(
                    checkpoints[i].middle / 50,
                    checkpoints[i].right / 50,
                    (checkpoints[i].right / 50) + new Vector3(0.0f, 0.2f, 0.0f),
                    (checkpoints[i].middle / 50) + new Vector3(0.0f, 0.2f, 0.0f)
                    );
            }
            BlueModel = meshBuilder.ToMesh();
            BlueModel.Normals = BlueModel.CalculateNormals();
        }

        public static void RoadSurface(List<RNW5RoadTri> roadTris, List<Vec3> roadVerts)
        {
            meshBuilder = new(false, false);
            Color4Collection colors = new();
            for (int n = 0; n < roadTris.Count; n++)
            {
                meshBuilder.AddTriangle(
                    roadVerts[roadTris[n].vertA].ToVector3() / 50,
                    roadVerts[roadTris[n].vertB].ToVector3() / 50,
                    roadVerts[roadTris[n].vertC].ToVector3() / 50);
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
                colors.Add(SurfaceTypeColors[roadTris[n].surface]);
            }
            RoadModel = meshBuilder.ToMesh();
            RoadModel.Colors = colors;
        }

        public static void Boundaries(List<List<Vector3>> boundaries)
        {
            meshBuilder = new(false, false);
            for (int n = 0; n < boundaries.Count; n++)
            {
                List<Vector3> vec3s = boundaries[n];
                meshBuilder.AddTube(vec3s, 0.005f, 18, true);
            }
            BoundaryModel = meshBuilder.ToMesh();
        }

        public static void Trilists(List<Vector3> tris, List<Vector3> neTris, List<Vector3> eTris)
        {
            meshBuilder = new(false, false);
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
            meshBuilder = new(false, false);
            meshBuilder.AddSphere(pos, 0.005f);
            ManipulatedModel = meshBuilder.ToMesh();
            ManipulatedModel.Normals = ManipulatedModel.CalculateNormals();
        }
    }
}
