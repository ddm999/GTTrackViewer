using System;
using System.Collections.Generic;
using System.Linq;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    public class MDL3MeshInfo
    {
        public uint MeshIndex;
        public List<string> MeshParams;

        public static MDL3MeshInfo FromStream(ref SpanReader sr, int mdlBasePos=0)
        {
            MDL3MeshInfo meshInfo = new();
            int strOffset = sr.ReadInt32();
            meshInfo.MeshIndex = sr.ReadUInt32();

            sr.Position = mdlBasePos + strOffset;

            string meshParamString = sr.ReadString0();
            // first will be empty so skip it
            meshInfo.MeshParams = new(meshParamString.Split("|").Skip(1));

            return meshInfo;
        }
    }
}
