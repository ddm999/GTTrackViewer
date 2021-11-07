using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

namespace GTTrackEditor.Controls
{
    public abstract class TrackEditorModel : MeshGeometryModel3D
    {
        /// <summary>
        /// Whether the model is allowed to rotate.
        /// </summary>
        public abstract bool CanRotate { get; }

        /// <summary>
        /// Whether the model is allowed to be translated.
        /// </summary>
        public abstract bool CanTranslate { get; }

        /// <summary>
        /// Whether the model is allowed to be scaled.
        /// </summary>
        public abstract bool CanScale { get; }
    }
}
