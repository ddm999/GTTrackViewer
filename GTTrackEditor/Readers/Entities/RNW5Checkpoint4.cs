using GTTrackEditor.Readers.Entities.Interfaces;
using SharpDX;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    public class RNW5Checkpoint4 : IFromStream, IToStream
    {
        public Vector3 Left;
        public Vector3 Middle;
        public Vector3 Right;
        public float trackV;

        public static RNW5Checkpoint4 FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            RNW5Checkpoint4 checkpoint = new();

            float x = sr.ReadSingle();
            float y = sr.ReadSingle();
            float z = sr.ReadSingle();
            checkpoint.Left = new(x, y, z);

            x = sr.ReadSingle();
            y = sr.ReadSingle();
            z = sr.ReadSingle();
            checkpoint.Middle = new(x, y, z);

            checkpoint.trackV = sr.ReadSingle();

            sr.Position = basePos + 0x28;

            x = sr.ReadSingle();
            y = sr.ReadSingle();
            z = sr.ReadSingle();
            checkpoint.Right = new(x, y, z);

            return checkpoint;
        }

        public void ToStream(ref SpanWriter sw)
        {
            sw.WriteSingle(Left.X);
            sw.WriteSingle(Left.Y);
            sw.WriteSingle(Left.Z);

            sw.WriteSingle(Middle.X);
            sw.WriteSingle(Middle.Y);
            sw.WriteSingle(Middle.Z);

            sw.WriteSingle(trackV);

            sw.WriteSingle(Middle.X);
            sw.WriteSingle(Middle.Y);
            sw.WriteSingle(Middle.Z);

            sw.WriteSingle(Right.X);
            sw.WriteSingle(Right.Y);
            sw.WriteSingle(Right.Z);

            sw.WriteSingle(trackV);
        }
    }
}
