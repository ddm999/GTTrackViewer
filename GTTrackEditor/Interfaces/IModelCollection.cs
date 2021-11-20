using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelixToolkit.Wpf.SharpDX;

namespace GTTrackEditor.Interfaces
{
    public interface IModelCollection
    {
        /// <summary>
        /// Adds a new element to the collection.
        /// </summary>
        public void AddNew();

        /// <summary>
        /// Removes a specific element from the collection.
        /// </summary>
        /// <param name="element"></param>
        public void Remove(Element3D element);
    }
}
