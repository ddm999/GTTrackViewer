using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using Syroot.BinaryData.Memory;
using Syroot.BinaryData.Core;

namespace GTTrackEditor.Readers.Entities.AutoDrive
{
    public class AttackInfo
    {
        public bool UnkBool { get; set; }
        public Vec3 Position { get; set; }

        public short UnkIndex { get; set; }
        public short UnkIndex2 { get; set; }
        public float Unk1 { get; set; }
        public float Unk2 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Angle { get; set; }

        public const int StrideSize = 0x40;

        public static AttackInfo FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            AttackInfo atkInfo = new AttackInfo();
            atkInfo.UnkBool = sr.ReadBoolean4();
            atkInfo.Position = new Vec3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            atkInfo.UnkIndex = sr.ReadInt16();
            atkInfo.UnkIndex2 = sr.ReadInt16();
            atkInfo.Unk1 = sr.ReadSingle();
            atkInfo.Unk2 = sr.ReadSingle();
            atkInfo.X2 = sr.ReadSingle();
            atkInfo.Y2 = sr.ReadSingle();
            atkInfo.Angle = sr.ReadSingle();

            return atkInfo;
        }
    }
}
