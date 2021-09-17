using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers
{
    class PACB
    {
        const string MAGIC = "PACB";
        public Dictionary<byte, MDL3> Models = new();
        
        public static PACB FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            string magic = sr.ReadStringRaw(4);
            if (magic != MAGIC)
                throw new InvalidDataException("Not a valid PACB file.");

            PACB pacb = new();

            sr.Position = basePos + 0x3C;
            uint tocEntryCount = sr.ReadUInt32();
            if (tocEntryCount == 0)
                throw new InvalidDataException("Contents is empty.");

            sr.Position = basePos + 0x40;
            if (sr.ReadUInt32() != 0)
                throw new InvalidDataException("First contents entry is not course data.");

            sr.Position = basePos + 0x48;
            int dataStart = sr.ReadInt32();
            int dataLength = sr.ReadInt32();
            int mdlStart = 0;

            sr.Position = basePos + dataStart + 0xAC;
            int texStart = basePos + dataStart + sr.ReadInt32();

            for (byte i=0; i<6; i++)
            {
                sr.Position = basePos + dataStart + (0x4*i) + 0x4;
                mdlStart = sr.ReadInt32();

                if (mdlStart == 0)
                    continue;

                sr.Position = mdlStart + basePos + dataStart;
                try {
                    pacb.Models.Add(i, MDL3.FromStream(ref sr, texStart));
                } catch (InvalidDataException) {

                }
            }

            return pacb;
        }

        public void DeleteModel(ref SpanReader sr, ref SpanWriter sw, byte modelID)
        {
            int basePos = sr.Position;

            sr.Position = basePos + 0x48;
            int dataStart = sr.ReadInt32();

            sw.Position = dataStart + 0x4 + (modelID * 0x4);
            sw.WriteInt32(0);

            _ = Models.Remove(modelID);
        }
    }
}
