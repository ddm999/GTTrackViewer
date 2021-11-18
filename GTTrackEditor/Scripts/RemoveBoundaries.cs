using System;
using System.Numerics;
using PDTools.Files.Courses.Runway;

namespace GTTrackEditor.Scripts;
public class RemoveBoundaries : ScriptBase
{
    public override string Name => "Remove Boundaries";

    public override bool RequiresRunway { get => true; }
    public override void OnExecute(TrackEditorView tev)
    {
        var rwy = tev.RunwayView.RunwayData;

        rwy.BoundaryVerts.Clear();
        rwy.BoundaryFaces.Clear();

        tev.RunwayView.Boundary.Hide();
        tev.RunwayView.Boundary.Show();
    }
}
