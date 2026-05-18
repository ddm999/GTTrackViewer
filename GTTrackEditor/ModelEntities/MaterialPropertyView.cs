using System.ComponentModel;
using PDTools.Files.Models.PS3.ModelSet3.Materials;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace GTTrackEditor.ModelEntities;

public class MaterialPropertyView
{
    private readonly MDL3Material _mat;

    public MaterialPropertyView(MDL3Material mat) => _mat = mat;

    [Browsable(true)]
    [PropertyOrder(0)]
    public string Name => _mat.Name;

    [Browsable(true)]
    [PropertyOrder(1)]
    public short MaterialDataID => _mat.MaterialDataID;

    [Browsable(true)]
    [PropertyOrder(2)]
    public short CellGcmParamsID => _mat.CellGcmParamsID;

    [Browsable(true)]
    [PropertyOrder(3)]
    public string Flags => $"0x{_mat.Flags:X4}";

    [Browsable(true)]
    [PropertyOrder(4)]
    public int TextureCount => _mat.ImageEntries.Count;
}
