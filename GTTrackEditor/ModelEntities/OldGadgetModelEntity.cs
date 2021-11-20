using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;

using PDTools.Files;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a gadget model entity.
/// </summary>
public class OldGadgetModelEntity : BaseModelEntity
{
    public override bool PitchRotationAllowed => false;
    public override bool RollRotationAllowed => false;

    public override void OnManipulation()
    {

    }

    public override void UpdateValues()
    {
        
    }
}

