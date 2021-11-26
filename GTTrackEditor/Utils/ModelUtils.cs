using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;

using SharpDX;

using System;
using System.Collections.Generic;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Matrix3D = System.Windows.Media.Media3D.Matrix3D;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using MatrixTransform3D = System.Windows.Media.Media3D.MatrixTransform3D;

namespace GTTrackEditor.Utils
{
    public class ModelUtils
    {
        public static void Rotate(Element3D model, Point3D center, float angle)
        {
            Vector3D axis = new(0, 1, 0); // In case you want to rotate it about the x-axis
            Matrix3D transformationMatrix = model.Transform.Value; // Gets the matrix indicating the current transformation value
            transformationMatrix.RotateAt(new Quaternion(axis, angle), center); // Rotate from center
            model.Transform = new MatrixTransform3D(transformationMatrix); // Applies the transformation
        }
    }
}
