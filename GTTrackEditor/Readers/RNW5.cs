using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using Syroot.BinaryData.Core;
using Syroot.BinaryData.Memory;
using GTTrackEditor.Readers.Entities;

namespace GTTrackEditor.Readers
{
    public class RNW5
    {
        const string MAGIC = "5WNR";

        public uint Version;
        public Vec3 BoundsMin = new(0.0f, 0.0f, 0.0f);
        public Vec3 BoundsMax = new(0.0f, 0.0f, 0.0f);
        public float TrackV;
        public List<Vec3R> StartingGrid = new();
        public List<RNW5Checkpoint4> Checkpoints = new();
        public List<ushort> CheckpointList = new();
        public List<Vec3> RoadVerts = new();
        public List<RNW5RoadTri> RoadTris = new();
        public List<RNW5BoundaryVert> BoundaryVerts = new();
        public List<ushort> BoundaryList = new();
        public List<Vec3R> PitStops = new();
        public List<Vec3R> PitStopAdjacents = new();

        public static RNW5 FromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;

            string magic = sr.ReadStringRaw(4);
            if (magic != MAGIC)
                throw new InvalidDataException("Not a valid RNW5 file.");

            RNW5 rnw5 = new();

            sr.Position = basePos + 0x8;
            int fileSize = sr.ReadInt32();
            rnw5.Version = sr.ReadUInt16() * 10U;
            rnw5.Version += sr.ReadUInt16();

            sr.Position = basePos + 0x14;
            rnw5.TrackV = sr.ReadSingle();

            sr.Position = basePos + 0x20;
            rnw5.BoundsMin = Vec3.FromStream(ref sr);
            rnw5.BoundsMax = Vec3.FromStream(ref sr);

            sr.Position = basePos + 0x4C;
            uint startingGridCount = sr.ReadUInt32();
            int startingGridOffset = sr.ReadInt32();
            uint checkpointCount = sr.ReadUInt32();
            int checkpointOffset = sr.ReadInt32();
            uint checkpointListCount = sr.ReadUInt32();
            int checkpointListOffset = sr.ReadInt32();

            sr.Position = basePos + 0x7C;
            uint roadVertCount = sr.ReadUInt32();
            int roadVertOffset = sr.ReadInt32();
            uint roadTriCount = sr.ReadUInt32();
            int roadTriOffset = sr.ReadInt32();

            sr.Position = basePos + 0x9C;
            uint boundaryVertCount = sr.ReadUInt32();
            int boundaryVertOffset = sr.ReadInt32();
            uint boundaryListCount = sr.ReadUInt32();
            int boundaryListOffset = sr.ReadInt32();
            uint pitStopCount = sr.ReadUInt32();
            int pitStopOffset = sr.ReadInt32();

            sr.Position = basePos + 0xBC;
            uint pitStopAdjacentCount = sr.ReadUInt32();
            int pitStopAdjacentOffset = sr.ReadInt32();

            sr.Position = basePos + startingGridOffset;
            for (int i = 0; i < startingGridCount; i++)
            {
                rnw5.StartingGrid.Add(Vec3R.FromStream(ref sr));
            }

            for (int i = 0; i < checkpointCount; i++)
            {
                sr.Position = basePos + checkpointOffset + (i * 0x38);
                rnw5.Checkpoints.Add(RNW5Checkpoint4.FromStream(ref sr));
            }

            sr.Position = basePos + checkpointListOffset;
            for (int i = 0; i < checkpointListCount; i++)
            {
                rnw5.CheckpointList.Add(sr.ReadUInt16());
            }

            sr.Position = basePos + roadVertOffset;
            for (int i = 0; i < roadVertCount; i++)
            {
                rnw5.RoadVerts.Add(Vec3.FromStream(ref sr));
            }

            for (int i = 0; i < roadTriCount; i++)
            {
                sr.Position = basePos + roadTriOffset + (i * 0x10);
                rnw5.RoadTris.Add(RNW5RoadTri.FromStream(ref sr));
            }

            for (int i = 0; i < boundaryVertCount; i++)
            {
                sr.Position = basePos + boundaryVertOffset + (i * 0x10);
                rnw5.BoundaryVerts.Add(RNW5BoundaryVert.FromStream(ref sr));
            }

            sr.Position = basePos + boundaryListOffset;
            for (int i = 0; i < boundaryListCount; i++)
            {
                rnw5.BoundaryList.Add(sr.ReadUInt16());
            }

            sr.Position = basePos + pitStopOffset;
            for (int i = 0; i < pitStopCount; i++)
            {
                rnw5.PitStops.Add(Vec3R.FromStream(ref sr));
            }

            sr.Position = basePos + pitStopAdjacentOffset;
            for (int i = 0; i < pitStopAdjacentCount; i++)
            {
                rnw5.PitStopAdjacents.Add(Vec3R.FromStream(ref sr));
            }

            return rnw5;
        }

        public RNW5 MergeFromStream(ref SpanReader sr)
        {
            int basePos = sr.Position;
            
            string magic = sr.ReadStringRaw(4);
            if (magic != MAGIC)
                throw new InvalidDataException("Not a valid RNW5 file.");



            return this;
        }

        public void ToStream(ref SpanReader sr, ref SpanWriter sw)
        {
            int basePos = sr.Position;

            sr.Position = basePos + 0x8;
            int fileSize = sr.ReadInt32();

            sw.Position = basePos + 0x14;
            sw.WriteSingle(TrackV);

            sw.Position = basePos + 0x20;
            BoundsMin.ToStream(ref sw);
            BoundsMax.ToStream(ref sw);

            sr.Position = basePos + 0x4C;
            int startingGridCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, StartingGrid, 0x10, startingGridCount, fileSize);

            int checkpointCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, Checkpoints, 0x38, checkpointCount, fileSize);

            int checkpointListCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelperUInt16(
                ref sr, ref sw, CheckpointList, checkpointListCount, fileSize);

            sr.Position = basePos + 0x7C;
            int roadVertCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, RoadVerts, 0xC, roadVertCount, fileSize);

            int roadTriCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, RoadTris, 0x10, roadTriCount, fileSize);

            sr.Position = basePos + 0x9C;
            int boundaryVertCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, BoundaryVerts, 0x10, boundaryVertCount, fileSize);

            int boundaryListCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelperUInt16(
                ref sr, ref sw, BoundaryList, boundaryListCount, fileSize);

            int pitStopCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, PitStops, 0x10, pitStopCount, fileSize);

            int pitStopAdjacentCount = sr.ReadInt32();
            fileSize = SpanExtender.ExtendHelper(
                ref sr, ref sw, PitStopAdjacents, 0x10, pitStopAdjacentCount, fileSize);

            // write final filesize
            sw.Position = basePos + 0x8;
            sw.WriteInt32(fileSize);
        }
    }
}
