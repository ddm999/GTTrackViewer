using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities.Interfaces
{
    interface IToStream
    {
        public void ToStream(ref SpanWriter sw)
        {
            return;
        }
    }
}
