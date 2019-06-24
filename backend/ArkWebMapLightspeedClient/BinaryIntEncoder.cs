using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapLightspeedClient
{
    public static class BinaryIntEncoder
    {
        public static byte[] Int32ToBytes(int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return b;
        }

        public static int BytesToInt32(byte[] data)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToInt32(data);
        }

        public static int BytesToInt32(byte[] data, int offset)
        {
            byte[] buf = new byte[4];
            Array.Copy(data, offset, buf, 0, 4);
            return BytesToInt32(buf);
        }
    }
}
