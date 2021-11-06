using SharpDX;
using HelixToolkit.Wpf.SharpDX;

namespace GTTrackEditor
{
    public class Gizmo
    {
        public bool Active { get; set; }

        public Element3D EditItem { get; set; }

        public void SetActive(Element3D selectedObject)
        {
            Active = true;
            EditItem = selectedObject;
        }

        public void SetInactive()
        {
            Active = false;
            EditItem = null;
        }
    }
}
