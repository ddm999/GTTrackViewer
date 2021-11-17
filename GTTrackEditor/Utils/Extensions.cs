using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using PDTools.Files;

using System.Numerics;
using SharpDX;

namespace GTTrackEditor.Utils
{
    public static class Extensions
    {
        public static SharpDX.Vector3 ToSharpDXVector(this System.Numerics.Vector3 vec)
        {
            Span<System.Numerics.Vector3> og = MemoryMarshal.CreateSpan(ref vec, 1);
            SharpDX.Vector3 newVec = MemoryMarshal.Cast<System.Numerics.Vector3, SharpDX.Vector3>(og)[0];
            return newVec;
        }

        public static SharpDX.Vector3 ToSharpDXVector(this Vec3R vec)
        {
            Span<Vec3R> og = MemoryMarshal.CreateSpan(ref vec, 1);
            SharpDX.Vector3 newVec = MemoryMarshal.Cast<Vec3R, SharpDX.Vector3>(og)[0];
            return newVec;
        }
    }
}
