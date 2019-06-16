using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer
{
    public static class SnowflakeGenerator
    {
        public static DateTime epoch = new DateTime(2019, 1, 1, 0, 0, 0); //Zero on the snowflake
        public static byte increment;

        /// <summary>
        /// Breaks the standard a little by having the workerID and increment be 8 bytes instead of 5.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="workerId"></param>
        /// <returns></returns>
        public static ulong GenerateSnowflake(DateTime time, byte workerId)
        {
            //Get time, then reverse it so we can edit the last 2 bytes
            Console.WriteLine((time - epoch).TotalMilliseconds);
            byte[] target = NumberToBytes((long)Math.Round((time - epoch).TotalMilliseconds));
            Array.Reverse(target);

            //Set the last two
            target[7] = increment++;
            target[6] = workerId;

            //Convert to a long and return it
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(target);
            return BitConverter.ToUInt64(target);
        }

        private static byte[] NumberToBytes(long n)
        {
            byte[] b = BitConverter.GetBytes(n);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }
    }
}
