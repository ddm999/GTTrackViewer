using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

namespace GTTrackEditor.Controls
{
    public class StartingGridModel3D : MeshGeometryModel3D
    {
        public Vec3R StartingGridPoint { get; set; }

        public int StartingIndex { get; set; }
    }
}
