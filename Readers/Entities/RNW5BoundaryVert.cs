using System;
using System.Collections.Generic;
using System.Linq;
using GTTrackEditor.Readers.Entities.Interfaces;
using SharpDX;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    class RNW5BoundaryVert : IFromStream, IToStream
    {
        public float X;
        public float Y;
        public float Z;
        public short counter;
        public ushort flags;

        public Vector3 ToVector3()
        {
            return new(X, Y, Z);
        }

        public static RNW5BoundaryVert FromStream(ref SpanReader sr)
        {
            RNW5BoundaryVert boundaryVert = new();
            boundaryVert.X = sr.ReadSingle();
            boundaryVert.Y = sr.ReadSingle();
            boundaryVert.Z = sr.ReadSingle();
            boundaryVert.counter = sr.ReadInt16();
            boundaryVert.flags = sr.ReadUInt16();
            return boundaryVert;
        }

        public void ToStream(ref SpanWriter sw)
        {
            sw.WriteSingle(X);
            sw.WriteSingle(Y);
            sw.WriteSingle(Z);
            sw.WriteInt16(counter);
            sw.WriteUInt16(flags);
        }
    }
}
