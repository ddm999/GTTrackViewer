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
    public class EnemyLine
    {
        public const string Magic = "ADLN";

        public List<AutoDriveInfo> AutoDriveInfos { get; set; } = new();

        public static EnemyLine FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            if (sr.ReadStringRaw(4) != Magic)
                throw new InvalidDataException("Not a valid ADLN header.");

            EnemyLine line = new EnemyLine();

            int relocPtr = sr.ReadInt32();
            sr.ReadInt32();
            int adlnFullSize = sr.ReadInt32();
            int unkCount = sr.ReadInt32();
            int autoDriveInfoCount = sr.ReadInt32();
            int autoDriveOffset = sr.ReadInt32();

            // List
            for (int i = 0; i < autoDriveInfoCount; i++)
            {
                sr.Position = (basePos + autoDriveOffset) + (sizeof(int) * i);
                int autoDriveInfoOffset = sr.ReadInt32();

                sr.Position = basePos + autoDriveInfoOffset;
                AutoDriveInfo driveInfo = AutoDriveInfo.FromStream(ref sr);
                line.AutoDriveInfos.Add(driveInfo);
            }

            return line;
        }
    }
}
