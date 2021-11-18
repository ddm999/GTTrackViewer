using System;

// TODO: eventually migrate this script system to a proper one
//        based on how UndertaleModTool handles scripts

namespace GTTrackEditor.Scripts;
public abstract class ScriptBase
{
    public virtual bool RequiresAutodrive { get => false; }
    public virtual bool RequiresCourseData { get => false; }
    public virtual bool RequiresRunway { get => false; }

    public void Execute(TrackEditorView tev)
    {
        if (RequiresAutodrive && !tev.AutodriveView.Loaded())
            throw new NullReferenceException("This script requires an Autodrive file to be loaded.");
        if (RequiresCourseData && !tev.CourseDataView.Loaded())
            throw new NullReferenceException("This script requires a Course Data file to be loaded.");
        if (RequiresRunway && !tev.RunwayView.Loaded())
            throw new NullReferenceException("This script requires a Runway file to be loaded.");

        OnExecute(tev);
    }

    public abstract void OnExecute(TrackEditorView tev);
}
