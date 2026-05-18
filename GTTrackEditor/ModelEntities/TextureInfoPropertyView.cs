using System.ComponentModel;
using PDTools.Files.Textures.PS3;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace GTTrackEditor.ModelEntities;

public class TextureInfoPropertyView
{
    private readonly PGLUCellTextureInfo _info;

    public TextureInfoPropertyView(string samplerName, PGLUCellTextureInfo info)
    {
        SamplerName = samplerName;
        _info = info;
    }

    [Browsable(true)] [PropertyOrder(0)]  public string SamplerName { get; }
    [Browsable(true)] [PropertyOrder(1)]  public uint ImageId => _info.ImageId;
    [Browsable(true)] [PropertyOrder(2)]  public ushort Width => _info.Width;
    [Browsable(true)] [PropertyOrder(3)]  public ushort Height => _info.Height;
    [Browsable(true)] [PropertyOrder(4)]  public string Format => _info.FormatBits.ToString();
    [Browsable(true)] [PropertyOrder(5)]  public string WrapS => _info.WrapS.ToString();
    [Browsable(true)] [PropertyOrder(6)]  public string WrapT => _info.WrapT.ToString();
    [Browsable(true)] [PropertyOrder(7)]  public string MagFilter => _info.Mag.ToString();
    [Browsable(true)] [PropertyOrder(8)]  public string MinFilter => _info.Min.ToString();
    [Browsable(true)] [PropertyOrder(9)]  public byte MipmapLevels => _info.MipmapLevelLast;
    [Browsable(true)] [PropertyOrder(10)] public string MaxAniso => _info.MaxAniso.ToString();
}
