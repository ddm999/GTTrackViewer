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
    public class AutoDriveInfo
    {
        public List<AttackInfo> AttackInfos { get; set; } = new();

        public static AutoDriveInfo FromStream(ref SpanReader sr)
        {
            AutoDriveInfo adInfo = new AutoDriveInfo();

            int basePos = sr.Position;
            int attackInfoCount = sr.ReadInt32();

            int dataOffset = basePos + 0x40;
            for (int i = 0; i < attackInfoCount; i++)
            {
                sr.Position = dataOffset + (i * 0x40);
                AttackInfo attackInfo = AttackInfo.FromStream(ref sr);
                adInfo.AttackInfos.Add(attackInfo);
            }

            return adInfo;
        }
    }
}
