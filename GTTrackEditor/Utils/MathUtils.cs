using System;

namespace GTTrackEditor.Utils
{
    public class MathUtils
    {
        public static float PDRadToDeg(float rad)
        {
            float degrees = (rad * 180) / MathF.PI;
            if (rad < 0)
            {
                degrees += 360;
            }

            return degrees;
        }
    }
}
