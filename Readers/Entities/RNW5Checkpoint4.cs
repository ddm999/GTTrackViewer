using GTTrackEditor.Readers.Entities.Interfaces;
using SharpDX;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    public class RNW5Checkpoint4 : IFromStream, IToStream
    {
        public Vector3 left;
        public Vector3 middle;
        public Vector3 right;
        public float trackV;

        public static RNW5Checkpoint4 FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            RNW5Checkpoint4 checkpoint = new();

            float x = sr.ReadSingle();
            float y = sr.ReadSingle();
            float z = sr.ReadSingle();
            checkpoint.left = new(x, y, z);

            x = sr.ReadSingle();
            y = sr.ReadSingle();
            z = sr.ReadSingle();
            checkpoint.middle = new(x, y, z);

            checkpoint.trackV = sr.ReadSingle();

            sr.Position = basePos + 0x28;

            x = sr.ReadSingle();
            y = sr.ReadSingle();
            z = sr.ReadSingle();
            checkpoint.right = new(x, y, z);

            return checkpoint;
        }

        public void ToStream(ref SpanWriter sw)
        {
            sw.WriteSingle(left.X);
            sw.WriteSingle(left.Y);
            sw.WriteSingle(left.Z);

            sw.WriteSingle(middle.X);
            sw.WriteSingle(middle.Y);
            sw.WriteSingle(middle.Z);

            sw.WriteSingle(trackV);

            sw.WriteSingle(middle.X);
            sw.WriteSingle(middle.Y);
            sw.WriteSingle(middle.Z);

            sw.WriteSingle(right.X);
            sw.WriteSingle(right.Y);
            sw.WriteSingle(right.Z);

            sw.WriteSingle(trackV);
        }
    }
}
