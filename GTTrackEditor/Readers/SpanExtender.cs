using GTTrackEditor.Readers.Entities.Interfaces;
using Syroot.BinaryData.Memory;
using System;
using System.Collections.Generic;

namespace GTTrackEditor.Readers
{
    class SpanExtender
    {
        public static void ExtendSpan(ref SpanReader sr, int toAdd)
        {
            Span<byte> newspan = new byte[sr.Length + toAdd];
            sr.Span.CopyTo(newspan);
            sr = new(newspan, sr.Endian);
        }
        public static void ExtendSpan(ref SpanWriter sw, int toAdd)
        {
            Span<byte> newspan = new byte[sw.Length + toAdd];
            sw.Span.CopyTo(newspan);
            sw = new(newspan, sw.Endian);
        }
        public static void ExtendSpan(ref SpanReader sr, ref SpanWriter sw, int toAdd)
        {
            Span<byte> newspan = new byte[sr.Length + toAdd];
            sr.Span.CopyTo(newspan);
            sr = new(newspan, sr.Endian);
            sw = new(newspan, sw.Endian);
        }

        public static int ExtendHelper<T>(ref SpanReader sr, ref SpanWriter sw, List<T> list, int itemSize, int oldCount, int fileSize) where T : IToStream
        {
            int offsetPos = sr.Position;

            if (oldCount < list.Count)
            {
                int addSize = list.Count * itemSize;
                ExtendSpan(ref sr, ref sw, addSize);

                sw.Position = offsetPos;
                sw.WriteInt32(fileSize); // new offset = old filesize

                for (int i = 0; i < list.Count; i++)
                {
                    sw.Position = fileSize + (i * itemSize);
                    list[i].ToStream(ref sw);
                }

                fileSize += addSize;
            }

            sr.Position = offsetPos + 0x4;

            return fileSize;
        }

        public static int ExtendHelperUInt16(ref SpanReader sr, ref SpanWriter sw, List<ushort> list, int oldCount, int fileSize)
        {
            int offsetPos = sr.Position;

            if (oldCount < list.Count)
            {
                int addSize = list.Count * 0x2;
                ExtendSpan(ref sr, ref sw, addSize);

                sw.Position = offsetPos;
                sw.WriteInt32(fileSize); // new offset = old filesize

                sw.Position = fileSize;
                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteUInt16(list[i]);
                }

                fileSize += addSize;
            }

            sr.Position = offsetPos + 0x4;

            return fileSize;
        }
    }
}
