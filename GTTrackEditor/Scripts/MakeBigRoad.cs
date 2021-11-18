using System;
using System.Numerics;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Scripts;
public class MakeBigRoad : ScriptBase
{
    public override bool RequiresRunway { get => true; }
    public override void OnExecute(TrackEditorView tev)
    {
        var rwy = tev.RunwayView.RunwayData;

        var baseUnk1 = rwy.RoadVerts.Vertices[0].Unk1;
        var baseUnk2 = rwy.RoadVerts.Vertices[0].Unk2;

        rwy.RoadVerts.Vertices.Clear();

        var baseVertCount = (uint)rwy.RoadVerts.Vertices.Count;

        var minX = rwy.BoundsMin.X;
        var maxX = rwy.BoundsMax.X;
        var minZ = rwy.BoundsMin.Z;
        var maxZ = rwy.BoundsMax.Z;

        uint xCount = 0;
        uint zCount = 0;
        for (float x = minX; x <= maxX; x += 5f)
        {
            for (float z = minZ; z <= maxZ; z += 5f)
            {
                Vector3 vec3 = new();
                vec3.X = x;
                vec3.Y = rwy.BoundsMin.Y;
                vec3.Z = z;

                RunwayRoadVert vert = new();
                vert.Vertex = vec3;
                vert.Unk1 = baseUnk1;
                vert.Unk2 = baseUnk2;
                rwy.RoadVerts.Vertices.Add(vert);
                if (x == minX)
                    zCount++;
            }
            xCount++;
        }

        var baseFlagsA = rwy.RoadTris[0].flagsA;
        var baseUnkBits = rwy.RoadTris[0].unkBits;
        var baseUnk = rwy.RoadTris[0].unk;
        var baseSurf = rwy.RoadTris[0].SurfaceType;

        rwy.RoadTris.Clear();

        for (uint i=0; i<zCount*(xCount-1); i++)
        {
            var firstVert = baseVertCount + i;
            if (i % zCount == zCount - 1)
                continue;

            for (int j=0; j<2; j++)
            {
                RunwayRoadTri tri = new();
                tri.SurfaceType = baseSurf;
                tri.flagsA = baseFlagsA;
                tri.unkBits = baseUnkBits;
                tri.unk = baseUnk;

                if (j==0)
                {
                    tri.VertA = firstVert;
                    tri.VertB = firstVert + 1;
                    tri.VertC = firstVert + zCount;
                }
                else
                {
                    tri.VertA = firstVert + zCount + 1;
                    tri.VertB = firstVert + zCount;
                    tri.VertC = firstVert + 1;
                }

                rwy.RoadTris.Add(tri);
            }
        }

        tev.RunwayView.Road.Hide();
        tev.RunwayView.Road.Show();
    }
}
