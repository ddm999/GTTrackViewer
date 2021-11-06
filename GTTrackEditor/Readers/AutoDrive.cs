using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Syroot.BinaryData.Memory;
using Syroot.BinaryData.Core;

using GTTrackEditor.Readers.Entities.AutoDrive;

namespace GTTrackEditor.Readers
{
    public class AutoDrive
    {
        public const int HeaderAlignment = 0x80;

        public EnemyLine EnemyLine { get; set; }

        public static AutoDrive FromStream(ref SpanReader sr)
        {
            AutoDrive ad = new AutoDrive();
            uint relocPtr = sr.ReadUInt32(); // If not 0, memory file most likely
            int enemyLineHeaderOffset = sr.ReadInt32();

            uint unkOffset1 = sr.ReadUInt32();
            uint unkOffset2 = sr.ReadUInt32();
            uint unkOffset3 = sr.ReadUInt32();
            uint unkOffset4 = sr.ReadUInt32();

            sr.Align(HeaderAlignment);

            if (sr.Position != enemyLineHeaderOffset)
            {
                Debug.WriteLine($"Abnormal padding for AutoDrive file header (ADLN expected at 0x40, got {enemyLineHeaderOffset})");
                sr.Position = enemyLineHeaderOffset;
                ad.EnemyLine = EnemyLine.FromStream(ref sr);
            }

            ad.EnemyLine = EnemyLine.FromStream(ref sr);
            return ad;
        }
    }
}
