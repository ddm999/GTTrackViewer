using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

using GTTrackEditor.ModelEntities;
using GTTrackEditor.Utils;

using PDTools.Files.Courses;
using PDTools.Files.Courses.CourseData;
using PDTools.Files.Models;
using PDTools.Files.Models.ModelSet3;
using PDTools.Files.Textures;
using PDTools.Files.Models.ModelSet3.Meshes;
using System.ComponentModel;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Linq;
using PDTools.Files.Models.ModelSet3.Commands;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;

namespace GTTrackEditor.Components.ModelSet;

public class ModelSetMeshComponent : TrackComponentBase
{
    public ObservableElement3DCollection MeshEntities { get; set; } = new();

    public ModelSetMeshComponent()
    {

    }

    public void Init()
    {
        Name = "Mesh Group";
    }


    public override void RenderComponent()
    {
        
    }

    public override void Hide()
    {
        if (!IsVisible)
            return;

        foreach (Element3D i in MeshEntities)
            (i as ModelSetMeshEntity).Hide();
        IsVisible = false;
    }

    public override void Show()
    {
        if (IsVisible)
            return;

        foreach (Element3D i in MeshEntities)
            (i as ModelSetMeshEntity).Show();
        IsVisible = true;
    }
}

