using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using GTTrackEditor.Attributes;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a starting grid model entity.
/// </summary>
public class StartingGridModelEntity : BaseModelEntity
{
    public Vec3R StartingGridPoint { get; set; }

    /// <summary>
    /// Position on the grid (0 indexed).
    /// </summary>
    [EditableProperty]
    public int StartingIndex { get; set; }

    public override bool CanRotateX => false;
    public override bool CanRotateZ => false;
}

