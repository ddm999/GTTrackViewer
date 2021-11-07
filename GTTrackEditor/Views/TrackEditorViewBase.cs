using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

using GTTrackEditor.Components;

namespace GTTrackEditor.Views
{
    public abstract class TrackEditorViewBase
    {
        /// <summary>
        /// Name for the tree view.
        /// </summary>
        public abstract string TreeViewName { get; }

        /// <summary>
        /// Components to render for this view.
        /// </summary>
        public ObservableCollection<TrackComponentBase> Components { get; } = new();
    }
}
