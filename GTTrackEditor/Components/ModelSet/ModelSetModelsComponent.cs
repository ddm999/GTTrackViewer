using System.Collections.ObjectModel;

namespace GTTrackEditor.Components.ModelSet;

public class ModelSetModelsComponent
{
    public string Name { get; } = "Models";
    public ObservableCollection<ModelSetModelComponent> Models { get; set; }
}
