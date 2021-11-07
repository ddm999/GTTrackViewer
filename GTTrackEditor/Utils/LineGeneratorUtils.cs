using HelixToolkit.SharpDX.Core;

using SharpDX;

namespace GTTrackEditor.Utils
{
    public class LineGeneratorUtils
    {
        /// <summary>
        /// Generates a square grid with a custom step
        /// </summary>
        /// <returns></returns>
        public static LineGeometry3D GenerateGrid(Vector3 plane, int min0 = 0, int max0 = 10, int min1 = 0, int max1 = 10, float step = 1f)
        {
            LineBuilder grid = new();
            if (plane == Vector3.UnitX)
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(0, i, min1), new Vector3(0, i, max1));
                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(0, min0, i), new Vector3(0, max0, i));
                }
            }
            else if (plane == Vector3.UnitY)
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(i, 0, min1), new Vector3(i, 0, max1));

                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(min0, 0, i), new Vector3(max0, 0, i));
                }
            }
            else
            {
                for (float i = min0; i <= max0; i += step)
                {
                    grid.AddLine(new Vector3(i, min1, 0), new Vector3(i, max1, 0));
                }
                for (float i = min1; i <= max1; i += step)
                {
                    grid.AddLine(new Vector3(min0, i, 0), new Vector3(max0, i, 0));
                }
            }

            return grid.ToLineGeometry3D();
        }
    }
}
