using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using GTTrackEditor.Attributes;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a modifiable track object mesh.
/// </summary>
public class ModObjectMeshModel : BaseModelEntity
{
    /// <summary>
    /// Mesh index inside the track model. Defaults to current count + 1.
    /// Will automatically insert around other meshes.
    /// </summary>
    [EditableProperty]
    public int MeshIndex { get; set; }

    /// <summary>
    /// Index of material to use for this mesh.
    /// Note: materials are of unknown format, so you'll have to figure them out with trial and error.
    /// </summary>
    [EditableProperty]
    public int MaterialIndex { get; set; } = 0;

    /// <summary>
    /// Can this mesh be seen in-game? Prefer this over deleting meshes.
    /// Will be automatically set by hiding/showing this element.
    /// </summary>
    [EditableProperty]
    public new bool Visible { get; set; }
}

