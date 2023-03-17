using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTTrackEditor.Utils;
using GTTrackEditor.Components;
using PDTools.Files.Courses.CourseData;
using GTTrackEditor.Components.ModelSet;

namespace GTTrackEditor.Views;

using HelixToolkit.Wpf.SharpDX;

using System.Collections.ObjectModel;

public class CourseDataView : TrackEditorViewBase
{
    public CourseDataFile CourseData { get; set; }
    public ModelSetComponent ModelSetComponent { get; set; } = new();

    public void SetCourseData(CourseDataFile courseData)
    {
        TreeViewName = "Course Data";
        CourseData = courseData;
    }

    public bool Loaded()
    {
        return CourseData is not null;
    }

    public void Init()
    {
        ModelSetComponent.Init(CourseData.MainModelSet);
        ModelSetComponent.Name = "Course Main Model";

        Components.Add(ModelSetComponent);
    }

    public bool IsSelected { get; set; }
    public bool IsExpanded { get; set; }

    public void Unload(Viewport3DX viewport)
    {
        foreach (var group in ModelSetComponent.RenderingGroups)
            viewport.Items.Remove(group);

        ModelSetComponent.RenderingGroups.Clear();
        Components.Clear();

        CourseData.Dispose();
        CourseData = null;
    }
}

