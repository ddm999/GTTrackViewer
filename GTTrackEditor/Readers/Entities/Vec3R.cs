using SharpDX;
using Syroot.BinaryData.Memory;
using GTTrackEditor.Readers.Entities.Interfaces;

namespace GTTrackEditor.Readers.Entities
{
    public class Vec3R : IToStream, IFromStream
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float R { get; set; }

        public Vec3R()
        {
            X = 0.0f; Y = 0.0f; Z = 0.0f; R = 0.0f;
        }

        public Vec3R(float x, float y, float z, float r)
        {
            X = x; Y = y; Z = z; R = r;
        }

        public Vector3 ToVector3()
        {
            return new(X, Y, Z);
        }

        public static Vec3R FromStream(ref SpanReader sr)
        {
            float x = sr.ReadSingle();
            float y = sr.ReadSingle();
            float z = sr.ReadSingle();
            float r = sr.ReadSingle();
            return new(x, y, z, r);
        }

        public void ToStream(ref SpanWriter sw)
        {
            sw.WriteSingle(X);
            sw.WriteSingle(Y);
            sw.WriteSingle(Z);
            sw.WriteSingle(R);
        }
    }
}
