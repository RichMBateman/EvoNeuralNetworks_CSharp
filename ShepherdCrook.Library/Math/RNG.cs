using System;

namespace ShepherdCrook.Library.Math
{
    /// <summary>
    /// Helper class for generating random numbers.
    /// </summary>
    public static class RNG
    {
        private static readonly Random m_random = new Random();

        #region Api

        /// <summary>
        /// Returns a random number between 0 and 1.
        /// </summary>
        public static Double Rnd()
        {
            return m_random.NextDouble();
        }

        /// <summary>
        /// Returns a non zero integer less than the max exclusive.
        /// </summary>
        public static Int32 Rnd(Int32 maxExclusive)
        {
            return m_random.Next(maxExclusive);
        }

        /// <summary>
        /// Returns a random integer within a range.
        /// </summary>
        public static Int32 Rnd(Int32 minInclusive, Int32 maxExclusive)
        {
            return m_random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a random floating number between min and max.
        /// </summary>
        public static Double Rnd(Double min, Double max)
        {
            return min + (m_random.NextDouble() * (max - min));
        }

        /// <summary>
        /// Returns either -1 or +1 (equal chance of each).
        /// </summary>
        public static Int32 Rnd1s()
        {
            return (Rnd() <= 0.5 ? -1 : +1);
        }

        /// <summary>
        /// Returns a random radian angle (a value from 0 - 2PI, i.e., between 0 and 360 degrees)
        /// </summary>
        public static Double RndRadianAngle()
        {
            return RNG.Rnd(0, 2 * System.Math.PI);
        }

        #endregion
    }
}
