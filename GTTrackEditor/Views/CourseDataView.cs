using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTTrackEditor.Utils;
using GTTrackEditor.Components;
using GTTrackEditor.Components.CourseData;
using GTTrackEditor.Readers;

namespace GTTrackEditor.Views;

public class CourseDataView : TrackEditorViewBase
{
    public PACB CourseData { get; set; }

    public CourseDataMeshComponent CourseModels { get; set; } = new();

    public void SetCourseData(PACB courseData)
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
        Components.Clear();

        CourseModels.Init(CourseData);

        Components.Add(CourseModels);
    }
}

