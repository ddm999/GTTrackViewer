using Syroot.BinaryData.Memory;
using GTTrackEditor.Readers.Entities.Interfaces;

namespace GTTrackEditor.Readers.Entities
{
    class RNW5RoadTri : IFromStream, IToStream
    {
        public ushort vertA;
        public ushort vertB;
        public ushort vertC;
        public ushort unk;
        public byte surface;
        public byte flagsA;
        public byte flagsB;
        public byte flagsC;

        public static RNW5RoadTri FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;
            RNW5RoadTri roadTri = new();

            sr.Position = basePos + 0x1;
            roadTri.vertA = sr.ReadUInt16();
            roadTri.surface = sr.ReadByte();

            sr.Position = basePos + 0x5;
            roadTri.vertB = sr.ReadUInt16();
            roadTri.flagsA = sr.ReadByte();

            sr.Position = basePos + 0x9;
            roadTri.vertC = sr.ReadUInt16();
            roadTri.flagsB = sr.ReadByte();
            roadTri.unk = sr.ReadUInt16();
            roadTri.flagsC = sr.ReadByte();

            return roadTri;
        }

        void ToStream(ref SpanWriter sw)
        {
            sw.WriteByte(0);
            sw.WriteUInt16(vertA);
            sw.WriteByte(surface);
            sw.WriteByte(0);
            sw.WriteUInt16(vertB);
            sw.WriteByte(flagsA);
            sw.WriteByte(0);
            sw.WriteUInt16(vertC);
            sw.WriteByte(flagsB);
            sw.WriteUInt16(unk);
            sw.WriteByte(flagsC);
            sw.WriteByte(0);
        }
    }
}
