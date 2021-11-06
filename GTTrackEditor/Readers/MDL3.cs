using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;

using GTTrackEditor.Readers.Entities;
using System.Linq;

namespace GTTrackEditor.Readers
{
    class MDL3
    {
        const string MAGIC = "MDL3";
        const string MAGIC_LE = "3LDM";

        public Dictionary<ushort, MDL3Set> Sets;
        public Dictionary<ushort, MDL3FVF> FVFs;
        public Dictionary<ushort, MDL3MeshInfo> MeshInfos;
        public Dictionary<ushort, MDL3Mesh> Meshes;

        public ushort SetCount;
        public ushort FVFCount;
        public ushort MeshInfoCount;
        public ushort MeshCount;
        public int TriCount = 0;

        public static MDL3 FromStream(ref SpanReader sr, int txsPos = 0)
        {
            int basePos = sr.Position;

            string magic = sr.ReadStringRaw(4);
            if (magic != MAGIC && magic != MAGIC_LE)
                throw new InvalidDataException("Not a valid MDL3 file.");

            MDL3 mdl3 = new();

            int fileSize = sr.ReadInt32();
            sr.Position = basePos + 0x10;
            mdl3.SetCount = sr.ReadUInt16();
            ushort gtbeCount = sr.ReadUInt16();
            mdl3.MeshCount = sr.ReadUInt16();
            mdl3.MeshInfoCount = sr.ReadUInt16();
            mdl3.FVFCount = sr.ReadUInt16();
            ushort boneCount = sr.ReadUInt16();

            sr.Position = basePos + 0x30;
            int modelSetOffset = sr.ReadInt32();
            int gtbeOffset = sr.ReadInt32();
            int meshOffset = sr.ReadInt32();
            int meshInfoOffset = sr.ReadInt32();
            int fvfOffset = sr.ReadInt32();

            sr.Position = basePos + 0x48;
            int txsOffset = sr.ReadInt32();
            int shdsOffset = sr.ReadInt32();
            int boneOffset = sr.ReadInt32();

            sr.Position = basePos + 0x6C;
            int txsFirstDataOffset = sr.ReadInt32();

            mdl3.Sets = new(mdl3.SetCount);
            for (ushort i=0; i<mdl3.SetCount; i++)
            {
                sr.Position = basePos + modelSetOffset + i * 0x30;
                mdl3.Sets.Add(i, MDL3Set.FromStream(ref sr, basePos));
            }

            mdl3.FVFs = new(mdl3.FVFCount);
            for (ushort i=0; i<mdl3.FVFCount; i++)
            {
                sr.Position = basePos + fvfOffset + i * 0x78;
                mdl3.FVFs.Add(i, MDL3FVF.FromStream(ref sr, basePos));
            }

            mdl3.MeshInfos = new(mdl3.MeshInfoCount);
            for (ushort i=0; i<mdl3.MeshInfoCount; i++)
            {
                sr.Position = basePos + meshInfoOffset + i * 0x8;
                mdl3.MeshInfos.Add(i, MDL3MeshInfo.FromStream(ref sr, basePos));
            }

            mdl3.Meshes = new(mdl3.MeshCount);
            for (ushort i=0; i<mdl3.MeshCount; i++)
            {
                sr.Position = basePos + meshOffset + i * 0x30;
                mdl3.Meshes.Add(i, MDL3Mesh.FromStream(ref sr, mdl3.FVFs, basePos));
                mdl3.TriCount += mdl3.Meshes[i].Tris.Count;
            }

            if (txsOffset < sr.Length)
            {
                sr.Position = basePos + txsOffset;
                // mdl3.Textures = TXS3.FromStream(ref sr);
            }

            return mdl3;
        }
    }
}
