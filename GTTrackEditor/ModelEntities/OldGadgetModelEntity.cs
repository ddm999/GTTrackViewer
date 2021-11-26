using System.Numerics;

using HelixToolkit.Wpf.SharpDX;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a gadget model entity.
/// </summary>
public class OldGadgetModelEntity : BaseModelEntity
{
    public override bool PitchRotationAllowed => false;
    public override bool RollRotationAllowed => false;

    public RunwayGadgetOld GadgetData { get; set; }

    public override void OnManipulation()
    {
        UpdateValues();
    }

    public override void UpdateValues()
    {
        GadgetData.Position = new Vector3(X, Y, Z);
    }
}

