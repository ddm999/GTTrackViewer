using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities
{
    public class MDL3Set
    {
        public Point3D origin;
        public List<Point3D> bounds;

        public static MDL3Set FromStream(ref SpanReader sr, int mdlBasePos = 0)
        {
            int setBasePos = sr.Position;

            MDL3Set set = new();

            sr.Position = setBasePos + 0x4;
            float x = sr.ReadSingle();
            float y = sr.ReadSingle();
            float z = sr.ReadSingle();
            set.origin = new(x, y, z);

            int boundsCount = sr.ReadInt32();
            if (boundsCount == 0)
                return set;
            
            int boundsOffset = sr.ReadInt32();

            set.bounds = new(boundsCount);
            for (int i=0; i<boundsCount; i++)
            {
                sr.Position = mdlBasePos + boundsOffset + i * 0xC;
                float bx = sr.ReadSingle();
                float by = sr.ReadSingle();
                float bz = sr.ReadSingle();
                set.bounds.Add(new(bx, by, bz));
            }
            
            return set;
        }
    }
}
