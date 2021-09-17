using SharpDX;
using Syroot.BinaryData.Memory;
using GTTrackEditor.Readers.Entities.Interfaces;

namespace GTTrackEditor.Readers
{
    class Vec3 : IFromStream, IToStream
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vec3()
        {
            X = 0.0f; Y = 0.0f; Z = 0.0f;
        }

        public Vec3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public Vector3 ToVector3()
        {
            return new(X, Y, Z);
        }

        public static Vec3 FromStream(ref SpanReader sr)
        {
            float x = sr.ReadSingle();
            float y = sr.ReadSingle();
            float z = sr.ReadSingle();
            return new(x, y, z);
        }

        public void ToStream(ref SpanWriter sw)
        {
            sw.WriteSingle(X);
            sw.WriteSingle(Y);
            sw.WriteSingle(Z);
        }
    }
}
