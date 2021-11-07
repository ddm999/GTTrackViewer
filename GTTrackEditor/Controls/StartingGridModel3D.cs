using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using GTTrackEditor.Attributes;

namespace GTTrackEditor.Controls
{
    public class StartingGridModel3D : TrackEditorModel
    {
        public Vec3R StartingGridPoint { get; set; }

        /// <summary>
        /// Position on the grid (0 indexed).
        /// </summary>
        [EditableProperty]
        public int StartingIndex { get; set; }


        public override bool CanRotate => true;
        public override bool CanTranslate => true;
        public override bool CanScale => false;
    }
}
