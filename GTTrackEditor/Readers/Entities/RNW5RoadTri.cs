using Syroot.BinaryData.Memory;
using GTTrackEditor.Readers.Entities.Interfaces;

namespace GTTrackEditor.Readers.Entities
{
    public class RNW5RoadTri : IFromStream, IToStream
    {
        public ushort VertA;
        public ushort VertB;
        public ushort VertC;
        public ushort unk;
        public byte SurfaceType;
        public byte flagsA;
        public byte flagsB;
        public byte flagsC;

        public static RNW5RoadTri FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;
            RNW5RoadTri roadTri = new();

            sr.Position = basePos + 0x1;
            roadTri.VertA = sr.ReadUInt16();
            roadTri.SurfaceType = sr.ReadByte();

            sr.Position = basePos + 0x5;
            roadTri.VertB = sr.ReadUInt16();
            roadTri.flagsA = sr.ReadByte();

            sr.Position = basePos + 0x9;
            roadTri.VertC = sr.ReadUInt16();
            roadTri.flagsB = sr.ReadByte();
            roadTri.unk = sr.ReadUInt16();
            roadTri.flagsC = sr.ReadByte();

            return roadTri;
        }

        void ToStream(ref SpanWriter sw)
        {
            sw.WriteByte(0);
            sw.WriteUInt16(VertA);
            sw.WriteByte(SurfaceType);
            sw.WriteByte(0);
            sw.WriteUInt16(VertB);
            sw.WriteByte(flagsA);
            sw.WriteByte(0);
            sw.WriteUInt16(VertC);
            sw.WriteByte(flagsB);
            sw.WriteUInt16(unk);
            sw.WriteByte(flagsC);
            sw.WriteByte(0);
        }
    }
}
