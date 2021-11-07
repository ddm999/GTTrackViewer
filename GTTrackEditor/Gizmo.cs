using SharpDX;
using HelixToolkit.Wpf.SharpDX;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace GTTrackEditor
{
    public class Gizmo : UserControl
    {
        public bool Active { get; set; }

        public Element3D EditItem
        {
            get => (Element3D)GetValue(EditItemProperty);
            set => SetValue(EditItemProperty, value);
        }

        public static readonly DependencyProperty EditItemProperty = DependencyProperty.Register("EditItem", typeof(Element3D), typeof(Gizmo));

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
