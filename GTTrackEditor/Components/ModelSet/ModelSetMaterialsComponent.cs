using System.Collections.Generic;
using PDTools.Files.Models.PS3.ModelSet3;
using PDTools.Files.Models.PS3.ModelSet3.Materials;
using PDTools.Files.Textures.PS3;

namespace GTTrackEditor.Components.ModelSet;

public class ResolvedTextureEntry
{
    public string SamplerName { get; set; }
    public PGLUCellTextureInfo TextureInfo { get; set; }  // null if TextureID has 0x8000 flag
    public ModelSet3 OwnerModelSet { get; set; }

    public string DisplayInfo => TextureInfo != null
        ? $"{TextureInfo.Width}×{TextureInfo.Height}"
        : "(external ref)";
}

public class ModelSetMaterialEntry
{
    public int Index { get; set; }
    public MDL3Material Material { get; set; }

    public string DisplayName => $"#{Index}: {Material.Name}";
    public List<ResolvedTextureEntry> ImageEntries { get; set; } = new();
}

public class ModelSetMaterialsComponent
{
    public string Name { get; } = "Materials";
    public List<ModelSetMaterialEntry> Materials { get; set; } = new();
}
