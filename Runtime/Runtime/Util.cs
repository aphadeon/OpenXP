using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenXP.Runtime
{
    static class Util
    {
        //our private, specially seeded System.Random instance
        private static Random random = new Random(GenerateSeed());

        //a quality seed is paramount :D
        public static int GenerateSeed()
        {
            return new Random(BitConverter.ToInt32(Shuffle(Guid.NewGuid().ToByteArray()), 0)).Next();
        }

        //generates a random int
        public static int RandomInt()
        {
            return random.Next();
        }

        //generates a random float
        public static float RandomFloat()
        {
            return (float) random.NextDouble();
        }

        //generates a random double
        public static double RandomDouble()
        {
            return random.NextDouble();
        }

        //generates a random long
        public static long RandomLong()
        {
            long result = random.Next();
            result = (result << 32);
            result = result | (long)random.Next();
            return result;
        }

        //generates an int of less than max
        public static int RandomInt(int max)
        {
            return random.Next(max);
        }

        //generates a float of less than max
        public static float RandomFloat(float max)
        {
            return (float) random.NextDouble() % max;
        }

        //generates a random double of less than max
        public static double RandomDouble(double max)
        {
            return random.NextDouble() % max;
        }

        //generates a long of less than max
        public static long RandomLong(long max)
        {
            return RandomLong() % max;
        }

        //generates a random int of at least min and less than max
        public static int RandomInt(int min, int max)
        {
            if (min == max) return min;
            if (max < min) Swap(ref min, ref max);
            return min + (random.Next(max - min));
        }

        //generates a random float of at least min and less than max
        public static float RandomFloat(float min, float max)
        {
            if (min == max) return min;
            if (max < min) Swap(ref min, ref max);
            return min + RandomFloat(max - min);
        }

        //generates a random float of at least min and less than max
        public static double RandomDouble(float min, float max)
        {
            if (min == max) return min;
            if (max < min) Swap(ref min, ref max);
            return min + RandomDouble(max - min);
        }

        //generates a random long of at least min and less than max
        public static long RandomLong(long min, long max)
        {
            if (min == max) return min;
            if (max < min) Swap(ref min, ref max);
            long result = random.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)random.Next((Int32)min, (Int32)max);
            return result;
        }

        //generates a random boolean
        public static bool RandomBool()
        {
            return random.NextDouble() >= 0.5;
        }

        //chooses a random entry from an array
        public static T Choose<T>(T[] array)
        {
            return array[RandomInt(array.Count())];
        }

        //chooses a random entry from a list
        public static T Choose<T>(IList<T> list)
        {
            return list[RandomInt(list.Count)];
        }

        //shuffles an array and returns it
        public static T[] Shuffle<T>(T[] array)
        {
            //we use shuffle as part of quality seed generation.
            //the following line generates a simple seeded random for generating our primary seed
            //otherwise, if that's all said and done, let's use our nice seeded global random
            var r = (random == null ? new Random(new Random().Next()) : random);
            for (int i = array.Length - 1; i > 0; i--)
            {
                Swap(ref array[i], ref array[r.Next(i + 1)]);
            }
            return array;
        }

        //shuffles a list and returns it
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            //iterate in reverse order, to avoid collection modification errors
            for(int i = list.Count - 1; i > 0; i--)
            {
                //can't use Swap on lists, have to do it manually
                int n = RandomInt(i + 1);
                T temp = list[i];
                list[i] = list[n];
                list[n] = temp;
            }
            return list;
        }

        //swap two variables
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
