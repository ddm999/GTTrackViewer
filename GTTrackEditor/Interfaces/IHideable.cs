using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTTrackEditor.Interfaces;

    /// <summary>
    /// Represents an interface for showing and hiding items.
    /// </summary>
public interface IHideable
{
    /// <summary>
    /// Whether this element is visible or not.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Shows this element.
    /// </summary>
    public void Show();

    /// <summary>
    /// Hides this element.
    /// </summary>
    public void Hide();
}

