using System;

namespace GTTrackEditor.Utils
{
    public class MathUtils
    {
        public static float PDRadToDeg(float rad)
        {
            if (rad < 0)
            {
                rad = MathF.PI + -rad;
            }

            return rad * 180f / MathF.PI;
        }
    }
}
