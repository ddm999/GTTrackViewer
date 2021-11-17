using System;
using System.Collections.Generic;
using SharpDX;
using Syroot.BinaryData.Memory;


namespace GTTrackEditor.Readers.Entities
{
    public class MDL3Mesh
    {
        public struct Tri
        {
            public ushort A;
            public ushort B;
            public ushort C;

            public Tri(ushort a, ushort b, ushort c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        public List<Vector3> Verts;
        public List<Tri> Tris;
        public Vector3[] BBox;
        public ushort MaterialIndex;
        public bool Tristrip = false;

        public static MDL3Mesh FromStream(ref SpanReader sr, Dictionary<ushort, MDL3FVF> fvfs = null, int mdlBasePos = 0)
        {
            int meshBasePos = sr.Position;

            MDL3Mesh mesh = new();

            ushort flags = sr.ReadUInt16();
            ushort fvfIndex = sr.ReadUInt16();
            mesh.MaterialIndex = sr.ReadUInt16();

            sr.Position = meshBasePos + 0x8;
            int vertCount = sr.ReadInt32();
            int vertOffset = sr.ReadInt32();

            sr.Position = meshBasePos + 0x14;
            int triLength = sr.ReadInt32();
            int triOffset = sr.ReadInt32();

            sr.Position = meshBasePos + 0x26;
            ushort triCount = sr.ReadUInt16();
            int bboxOffset = sr.ReadInt32();

            mesh.Verts = new(vertCount);
            if (fvfs != null)
            {
                byte fvfDataLength = fvfs[fvfIndex].dataLength;

                if (vertCount > 0 && vertOffset != 0)
                {
                    for (int i=0; i<vertCount; i++)
                    {
                        sr.Position = mdlBasePos + vertOffset + i * fvfDataLength;
                        float x = sr.ReadSingle();
                        float y = sr.ReadSingle();
                        float z = sr.ReadSingle();
                        mesh.Verts.Add(new(x, y, z));
                    }
                }
            }

            mesh.Tris = new(triCount);
            if (triLength > 0 && triOffset != 0)
            {
                sr.Position = mdlBasePos + triOffset;
                for (int i=0; i<triCount; i++)
                {
                    ushort a = sr.ReadUInt16();
                    ushort b = sr.ReadUInt16();
                    ushort c = sr.ReadUInt16();
                    if (a < vertCount && b < vertCount && c < vertCount)
                    {
                        mesh.Tris.Add(new(a, b, c));
                    }
                    else
                    {
                        mesh.Tristrip = true;
                        break;
                    }   
                }
            }
            
            if (bboxOffset != 0)
            {
                mesh.BBox = new Vector3[8];
                sr.Position = bboxOffset;
                for (int i = 0; i < 8; i++)
                {
                    mesh.BBox[i].X = sr.ReadSingle();
                    mesh.BBox[i].Y = sr.ReadSingle();
                    mesh.BBox[i].Z = sr.ReadSingle();
                }
            }

            return mesh;
        }
    }
}
