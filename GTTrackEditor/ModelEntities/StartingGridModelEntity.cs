using System.ComponentModel;

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using PDTools.Files;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.ModelEntities;

/// <summary>
/// Represents a starting grid model entity.
/// </summary>
public class StartingGridModelEntity : BaseModelEntity
{
    public RunwayStartingGridPosition StartingGridPoint { get; set; }

    /// <summary>
    /// Position on the grid (0 indexed).
    /// </summary>
    [Browsable(true)]
    public int StartingIndex { get; set; }

    public override bool CanRotateX => false;
    public override bool CanRotateZ => false;

    
    public override void OnMove()
    {
        BoundingBox box = BoundsWithTransform;
        StartingGridPoint.Position = new Vec3R(box.Center.X, box.Center.Y, box.Center.Z, StartingGridPoint.Position.AngleRad);
    }
}

