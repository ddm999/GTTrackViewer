using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTTrackEditor.Components
{
    public abstract class TrackComponentBase
    {
        public abstract string Name { get; }

        /// <summary>
        /// Whether to render this component.
        /// </summary>
        private bool _render;
        public bool Render
        {
            get => _render;
            set
            {
                if (!value)
                    Hide();

                _render = value;
            }
        }

        public abstract void RenderComponent();

        public abstract void Hide();
    }
}
