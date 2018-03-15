using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Linq;
public class MathUtils
{
    static public float nanoToSec = 1 / 1000000000f;

    // ---
    static public float FLOAT_ROUNDING_ERROR = 0.000001f; // 32 bits
    static public float PI = 3.1415927f;
    static public float PI2 = PI * 2;

    static public float E = 2.7182818f;

    static private int SIN_BITS = 14; // 16KB. Adjust for accuracy.
    static private int SIN_MASK = ~(-1 << SIN_BITS);
    static private int SIN_COUNT = SIN_MASK + 1;

    static private float radFull = PI * 2;
    static private float degFull = 360;
    static private float radToIndex = SIN_COUNT / radFull;
    static private float degToIndex = SIN_COUNT / degFull;

    /** multiply by this to convert from radians to degrees */
    static public float radiansToDegrees = 180f / PI;
    static public float radDeg = radiansToDegrees;
    /** multiply by this to convert from degrees to radians */
    static public float degreesToRadians = PI / 180;
    static public float degRad = degreesToRadians;

    public class Sin
    {
        static float[] table = new float[SIN_COUNT];

        public Sin()
        {
            for (int i = 0; i < SIN_COUNT; i++)
                table[i] = (float)Math.Sin((i + 0.5f) / SIN_COUNT * radFull);
            for (int i = 0; i < 360; i += 90)
                table[(int)(i * degToIndex) & SIN_MASK] = (float)Math.Sin(i * degreesToRadians);
        }




        /** Returns the sine in radians from a lookup table. */
        static public float sin(float radians)
        {
            return Sin.table[(int)(radians * radToIndex) & SIN_MASK];
        }

        /** Returns the cosine in radians from a lookup table. */
        static public float cos(float radians)
        {
            return Sin.table[(int)((radians + PI / 2) * radToIndex) & SIN_MASK];
        }

        /** Returns the sine in radians from a lookup table. */
        static public float sinDeg(float degrees)
        {
            return Sin.table[(int)(degrees * degToIndex) & SIN_MASK];
        }

        /** Returns the cosine in radians from a lookup table. */
        static public float cosDeg(float degrees)
        {
            return Sin.table[(int)((degrees + 90) * degToIndex) & SIN_MASK];
        }

        // ---

        /** Returns atan2 in radians, faster but less accurate than Math.atan2. Average error of 0.00231 radians (0.1323 degrees),
         * largest error of 0.00488 radians (0.2796 degrees). */
        static public float atan2(float y, float x)
        {
            if (x == 0f)
            {
                if (y > 0f) return PI / 2;
                if (y == 0f) return 0f;
                return -PI / 2;
            }
            float atan, z = y / x;
            if (Math.Abs(z) < 1f)
            {
                atan = z / (1f + 0.28f * z * z);
                if (x < 0f) return atan + (y < 0f ? -PI : PI);
                return atan;
            }
            atan = PI / 2 - z / (z * z + 0.28f);
            return y < 0f ? atan - PI : atan;
        }
    }
    // ---
    public class Random
    {
        static public Random random = new Random();

        /** Returns a random number between 0 (inclusive) and the specified value (inclusive). */
        static public int GetRandom(int range)
        {
            System.Random r = new System.Random();
            return r.Next(range);
        }

        /** Returns a random number between start (inclusive) and end (inclusive). */
        static public int GetRandom(int start, int end)
        {
            System.Random r = new System.Random();
            return r.Next(end) + start;
        }

        /** Returns a random number between 0 (inclusive) and the specified value (inclusive). */
        static public long GetRandom(long range)
        {
            System.Random r = new System.Random();
            return (long)(r.NextDouble() * range);
        }

        /** Returns a random number between start (inclusive) and end (inclusive). */
        static public long GetRandom(long start, long end)
        {
            System.Random r = new System.Random();

            return start + (long)(r.NextDouble() * (end - start));
        }

        /** Returns a random bool value. */
        static public bool randomBoolean()
        {
            System.Random r = new System.Random();

            return r.Next() % 2 == 0;
        }

        /** Returns true if a random value between 0 and 1 is less than the specified value. */
        static public bool randomBoolean(float chance)
        {
            System.Random r = new System.Random();

            return r.NextDouble() < chance;
        }

        /** Returns random number between 0.0 (inclusive) and 1.0 (exclusive). */
        static public float randomFloat()
        {
            System.Random r = new System.Random();

            return Convert.ToSingle(r.NextDouble());
        }

        /** Returns a random number between 0 (inclusive) and the specified value (exclusive). */
        static public float randomFloat(float range)
        {
            System.Random r = new System.Random();

            return Convert.ToSingle(r.NextDouble()) * range;
        }

        /** Returns a random number between start (inclusive) and end (exclusive). */
        static public float randomFloat(float start, float end)
        {
            System.Random r = new System.Random();

            return start + Convert.ToSingle(r.NextDouble()) * (end - start);
        }

        /** Returns -1 or 1, randomly. */
        static public int randomSign()
        {
            System.Random r = new System.Random();

            return 1 | (r.Next() >> 31);
        }

        /** Returns a triangularly distributed random number between -1.0 (exclusive) and 1.0 (exclusive), where values around zero are
         * more likely.
         * <p>
         * This is an optimized version of {@link #randomTriangular(float, float, float) randomTriangular(-1, 1, 0)} */
        public static float randomTriangular()
        {
            System.Random r = new System.Random();

            return Convert.ToSingle(r.NextDouble()) - Convert.ToSingle(r.NextDouble());
        }

        /** Returns a triangularly distributed random number between {@code -max} (exclusive) and {@code max} (exclusive), where values
         * around zero are more likely.
         * <p>
         * This is an optimized version of {@link #randomTriangular(float, float, float) randomTriangular(-max, max, 0)}
         * @param max the upper limit */
        public static float randomTriangular(float max)
        {
            System.Random r = new System.Random();

            return (Convert.ToSingle(r.NextDouble()) - Convert.ToSingle(r.NextDouble())) * max;
        }

        /** Returns a triangularly distributed random number between {@code min} (inclusive) and {@code max} (exclusive), where the
         * {@code mode} argument defaults to the midpoint between the bounds, giving a symmetric distribution.
         * <p>
         * This method is equivalent of {@link #randomTriangular(float, float, float) randomTriangular(min, max, (min + max) * .5f)}
         * @param min the lower limit
         * @param max the upper limit */
        public static float randomTriangular(float min, float max)
        {
            return randomTriangular(min, max, (min + max) * 0.5f);
        }

        /** Returns a triangularly distributed random number between {@code min} (inclusive) and {@code max} (exclusive), where values
         * around {@code mode} are more likely.
         * @param min the lower limit
         * @param max the upper limit
         * @param mode the point around which the values are more likely */
        public static float randomTriangular(float min, float max, float mode)
        {
            System.Random r = new System.Random();

            float u = Convert.ToSingle(r.NextDouble());
            float d = max - min;
            if (u <= (mode - min) / d) return min + (float)Math.Sqrt(u * d * (mode - min));
            return max - (float)Math.Sqrt((1 - u) * d * (max - mode));
        }

        // ---

        /** Returns the next power of two. Returns the specified value if the value is already a power of two. */
        static public int nextPowerOfTwo(int value)
        {
            if (value == 0) return 1;
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }
    }

    static public bool isPowerOfTwo(int value)
    {
        return value != 0 && (value & value - 1) == 0;
    }

    // ---

    static public short clamp(short value, short min, short max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    static public int clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    static public long clamp(long value, long min, long max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    static public float clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    static public double clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    // ---

    /** Linearly interpolates between fromValue to toValue on progress position. */
    static public float lerp(float fromValue, float toValue, float progress)
    {
        return fromValue + (toValue - fromValue) * progress;
    }

    /** Linearly interpolates between two angles in radians. Takes into account that angles wrap at two pi and always takes the
     * direction with the smallest delta angle.
     * 
     * @param fromRadians start angle in radians
     * @param toRadians target angle in radians
     * @param progress interpolation value in the range [0, 1]
     * @return the interpolated angle in the range [0, PI2[ */
    public static float lerpAngle(float fromRadians, float toRadians, float progress)
    {
        float delta = ((toRadians - fromRadians + PI2 + PI) % PI2) - PI;
        return (fromRadians + delta * progress + PI2) % PI2;
    }

    /** Linearly interpolates between two angles in degrees. Takes into account that angles wrap at 360 degrees and always takes
     * the direction with the smallest delta angle.
     * 
     * @param fromDegrees start angle in degrees
     * @param toDegrees target angle in degrees
     * @param progress interpolation value in the range [0, 1]
     * @return the interpolated angle in the range [0, 360[ */
    public static float lerpAngleDeg(float fromDegrees, float toDegrees, float progress)
    {
        float delta = ((toDegrees - fromDegrees + 360 + 180) % 360) - 180;
        return (fromDegrees + delta * progress + 360) % 360;
    }

    // ---

    static private int BIG_ENOUGH_INT = 16 * 1024;
    static private double BIG_ENOUGH_FLOOR = BIG_ENOUGH_INT;
    static private double CEIL = 0.9999999;

    static private double BIG_ENOUGH_ROUND = BIG_ENOUGH_INT + 0.5f;

    /** Returns the largest integer less than or equal to the specified float. This method will only properly floor floats from
     * -(2^14) to (Float.MAX_VALUE - 2^14). */
    static public int floor(float value)
    {
        return (int)(value + BIG_ENOUGH_FLOOR) - BIG_ENOUGH_INT;
    }

    /** Returns the largest integer less than or equal to the specified float. This method will only properly floor floats that are
     * positive. Note this method simply casts the float to int. */
    static public int floorPositive(float value)
    {
        return (int)value;
    }

    /** Returns the smallest integer greater than or equal to the specified float. This method will only properly ceil floats from
     * -(2^14) to (Float.MAX_VALUE - 2^14). */
    static public int ceil(float value)
    {
        return BIG_ENOUGH_INT - (int)(BIG_ENOUGH_FLOOR - value);
    }

    /** Returns the smallest integer greater than or equal to the specified float. This method will only properly ceil floats that
     * are positive. */
    static public int ceilPositive(float value)
    {
        return (int)(value + CEIL);
    }

    /** Returns the closest integer to the specified float. This method will only properly round floats from -(2^14) to
     * (Float.MAX_VALUE - 2^14). */
    static public int round(float value)
    {
        return (int)(value + BIG_ENOUGH_ROUND) - BIG_ENOUGH_INT;
    }

    /** Returns the closest integer to the specified float. This method will only properly round floats that are positive. */
    static public int roundPositive(float value)
    {
        return (int)(value + 0.5f);
    }

    /** Returns true if the value is zero (using the default tolerance as upper bound) */
    static public bool isZero(float value)
    {
        return Math.Abs(value) <= FLOAT_ROUNDING_ERROR;
    }

    /** Returns true if the value is zero.
     * @param tolerance represent an upper bound below which the value is considered zero. */
    static public bool isZero(float value, float tolerance)
    {
        return Math.Abs(value) <= tolerance;
    }

    /** Returns true if a is nearly equal to b. The function uses the default floating error tolerance.
     * @param a the first value.
     * @param b the second value. */
    static public bool isEqual(float a, float b)
    {
        return Math.Abs(a - b) <= FLOAT_ROUNDING_ERROR;
    }

    /** Returns true if a is nearly equal to b.
     * @param a the first value.
     * @param b the second value.
     * @param tolerance represent an upper bound below which the two values are considered equal. */
    static public bool isEqual(float a, float b, float tolerance)
    {
        return Math.Abs(a - b) <= tolerance;
    }

    /** @return the logarithm of value with base a */
    static public float log(float a, float value)
    {
        return (float)(Math.Log(value) / Math.Log(a));
    }

    /** @return the logarithm of value with base 2 */
    static public float log2(float value)
    {
        return log(2, value);
    }
}
