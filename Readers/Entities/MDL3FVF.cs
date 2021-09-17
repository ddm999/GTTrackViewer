using System;
using System.Collections.Generic;
using GTTrackEditor.Readers.Entities.Interfaces;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    class MDL3FVF : IFromStream
    {
        public byte dataLength;
        public List<string> contents = new(4);

        public static MDL3FVF FromStream(ref SpanReader sr, int mdlBasePos=0)
        {
            int fvfBasePos = sr.Position;

            MDL3FVF fvf = new();

            sr.Position = fvfBasePos + 0x8;
            int contentsOffset = sr.ReadInt32();

            sr.Position = fvfBasePos + 0x19;
            fvf.dataLength = sr.ReadByte();

            for (short i=3; i>=0; i--) // contents are stored backwards
            {
                sr.Position = mdlBasePos + contentsOffset + (i*0x8);
                int contentItemStrOffset = sr.ReadInt32();
                if (contentItemStrOffset == 0)
                    continue;
                sr.Position = mdlBasePos + contentItemStrOffset;
                fvf.contents.Add(sr.ReadString0());
            }

            return fvf;
        }
    }
}
