using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using GTTrackEditor.Attributes;

using PDTools.Files;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a gadget model entity.
/// </summary>
public class OldGadgetModelEntity : BaseModelEntity
{
    public override bool CanRotateX => false;
    public override bool CanRotateZ => false;

    public override void OnMove()
    {

    }
}

