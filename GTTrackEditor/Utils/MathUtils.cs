using System;

namespace GTTrackEditor.Utils
{
    public class MathUtils
    {
        /// <summary>
        /// Converts (-π to π) to degrees. 
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static float Atan2RadToDeg(float rad)
        {
            float degrees = (rad * 180) / MathF.PI;
            if (rad < 0)
                degrees += 360f;

            return degrees;
        }
    }
}
