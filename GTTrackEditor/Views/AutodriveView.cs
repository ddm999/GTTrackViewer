using DemoCore;

using GTTrackEditor.Readers;
using GTTrackEditor.Readers.Entities;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Windows;

using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

using PDTools.Files.Courses.AutoDrive;

namespace GTTrackEditor.Views;

public class AutodriveView : TrackEditorViewBase
{
    public AutoDriveFile AutodriveData { get; private set; }

    public MeshGeometry3D AutoDriveModel { get; set; } = new();
    public DiffuseMaterial DrivingLineMaterial { get; set; } = new();

    public BillboardSingleText3D AutoDriveText { get; set; }
    public ObservableElement3DCollection AutoDrivePointText { get; set; } = new ObservableElement3DCollection();

    public bool RenderLine1 { get; set; }
    public bool RenderLeftLane { get; set; }
    public bool RenderRightLane { get; set; }
    public bool RenderPitExitLane { get; set; }
    public bool RenderRestrictedArea { get; set; }
    public bool RenderLearningSection { get; set; }
    public bool RenderDefaultLane { get; set; }

    public bool DrawText { get; set; }

    private MeshBuilder _meshBuilder;

    public AutodriveView()
    {
        TreeViewName = "Autodrive";
        DrivingLineMaterial.DiffuseColor = new Color4(0.231f, 0.788f, 0.929f, 0.8f);
    }

    public bool Loaded()
    {
        return AutodriveData is not null;
    }

    public void SetAutodriveData(AutoDriveFile ad)
    {
        AutodriveData = ad;
    }

    public void Render()
    {
        AutoDrivePointText.Clear();

        AutoDriveModel.ClearAllGeometryData();
        _meshBuilder = new MeshBuilder();
        if (RenderLine1)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[0]);
        if (RenderLeftLane)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[1]);
        if (RenderRightLane)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[2]);
        if (RenderPitExitLane)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[3]);
        if (RenderRestrictedArea)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[4]);
        if (RenderLearningSection)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[5]);
        if (RenderDefaultLane)
            AddAdLine(AutodriveData.EnemyLine.AutoDriveInfos[6]);

        MeshGeometry3D newMesh = _meshBuilder.ToMeshGeometry3D();
        newMesh.AssignTo(AutoDriveModel);
    }

    public void AddAdLine(AutoDriveInfo adInfo)
    {
        for (int i = 0; i < adInfo.AttackInfos.Count; i++)
        {
            AttackInfo atk = adInfo.AttackInfos[i];
            Vector3 attackPoint = new(-(atk.Position.X), atk.Position.Z, atk.Position.Y);

            if (DrawText)
            {
                Vector3 textLoc = attackPoint + new Vector3(0, 0.015f, -0.04f);
                AutoDrivePointText.Add(new BillboardTextModel3D()
                {
                    FixedSize = false,
                    Geometry = new BillboardSingleText3D(0.08f, 0.08f)
                    {
                        TextInfo = new TextInfo($"[{i}/{adInfo.AttackInfos.Count}] {atk.Unk2}", textLoc),
                        FontSize = 24,
                        FontColor = Color4.Black,
                        //Padding = new Thickness(24),
                    }
                });
            }

            float r = atk.Angle;

            _meshBuilder.AddSphere(attackPoint, 0.020f);
            Vector3 attackDest = attackPoint;
            attackDest.X += (0 * MathF.Cos(-r)) - (0.1f * MathF.Sin(-r));
            attackDest.Z += (0 * MathF.Sin(-r)) + (0.1f * MathF.Cos(-r));
            _meshBuilder.AddArrow(attackPoint, attackDest, 0.01f, 2.0f, 18);
        }
    }
}

