﻿using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;
using GTTrackEditor.Components;
using GTTrackEditor.Components.Minimap;

using PDTools.Files;
using PDTools.Files.Courses.Minimap;

using System.Collections.ObjectModel;

namespace GTTrackEditor.Views;

public class CourseMapView : TrackEditorViewBase
{
    public string FileName { get; set; }

    public CourseMapFile MinimapData { get; private set; }

    public MinimapFaceComponent SectionFaceComponent { get; set; } = new();
    public MinimapFaceComponent RoadFaceComponent { get; set; } = new();
    public MinimapFaceComponent PitlaneFaceComponent { get; set; } = new();

    public CourseMapView()
    {
        TreeViewName = "Course Map";
    }

    public void SetMinimapData(CourseMapFile runway)
    {
        MinimapData = runway;
    }

    public bool Loaded()
    {
        return MinimapData is not null;
    }

    public void Init()
    {
        Components.Clear();
        SectionFaceComponent.Faces.Clear();
        RoadFaceComponent.Faces.Clear();
        PitlaneFaceComponent.Faces.Clear();

        if (MinimapData.SectionFaces.Count > 0)
        {
            SectionFaceComponent.IsSectorFace = true;
            SectionFaceComponent.Name = "Sectors";
            SectionFaceComponent.Init(MinimapData.SectionFaces);
            Components.Add(SectionFaceComponent);
        }

        if (MinimapData.UnkFaces.Count > 0)
        {
            RoadFaceComponent.Name = "Road";
            RoadFaceComponent.Init(MinimapData.UnkFaces);
            Components.Add(RoadFaceComponent);
        }

        if (MinimapData.PitLaneFaces.Count > 0)
        {
            PitlaneFaceComponent.IsPitlaneFace = true;
            PitlaneFaceComponent.Name = "Pitlane";
            PitlaneFaceComponent.Init(MinimapData.PitLaneFaces);
            Components.Add(PitlaneFaceComponent);
        }
    }
}

