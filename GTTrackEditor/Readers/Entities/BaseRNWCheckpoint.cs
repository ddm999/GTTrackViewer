using GTTrackEditor.Readers.Entities.Interfaces;
using SharpDX;

namespace GTTrackEditor.Readers.Entities;
public class BaseRNWCheckpoint : IFromStream, IToStream
{
    public Vector3 Left;
    public Vector3 Middle;
    public Vector3 Right;
    public float trackV;
}
