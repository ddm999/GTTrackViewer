using System;

namespace GTTrackEditor.Utils
{
    public class MathUtils
    {
        public static float Atan2RadToDeg(float rad)
        {
            float degrees = (rad * 180) / MathF.PI;
            if (rad < 0)
                degrees += 360f;

            return degrees;
        }
    }
}
