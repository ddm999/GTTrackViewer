using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Media;

using GTTrackEditor.Components;

namespace GTTrackEditor.Views;

public abstract class TrackEditorViewBase
{
    /// <summary>
    /// Name for the tree view.
    /// </summary>
    public string TreeViewName { get; set; }

    /// <summary>
    /// Components to render for this view.
    /// </summary>
    public ObservableCollection<TrackComponentBase> Components { get; } = new();

    public void Render()
    {
        foreach (var component in Components)
            component.RenderComponent();
    }
}

