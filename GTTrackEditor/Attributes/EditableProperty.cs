using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTTrackEditor.Attributes;

/// <summary>
/// Marks a property on a model entity on the track editor that can be edited (through the property grid).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EditableProperty : Attribute
{

}

