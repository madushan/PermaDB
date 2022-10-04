namespace Perma.Engine
{
    internal class LittleEndian
    {
        public static int GetInt32(byte[] bytes)
        {
            if (false == BitConverter.IsLittleEndian)
            {
                byte[] bytesClone = new byte[bytes.Length];
                bytes.CopyTo(bytesClone, 0);
                Array.Reverse(bytesClone);
                return BitConverter.ToInt32(bytesClone, 0);
            }
            else
            {
                return BitConverter.ToInt32(bytes, 0);
            }
        }
        public static long GetInt64(byte[] bytes)
        {
            if (false == BitConverter.IsLittleEndian)
            {
                byte[] bytesClone = new byte[bytes.Length];
                bytes.CopyTo(bytesClone, 0);
                Array.Reverse(bytesClone);
                return BitConverter.ToInt64(bytesClone, 0);
            }
            else
            {
                return BitConverter.ToInt64(bytes, 0);
            }
        }

        public static uint GetUInt32(byte[] bytes)
        {
            if (false == BitConverter.IsLittleEndian)
            {
                byte[] bytesClone = new byte[bytes.Length];
                bytes.CopyTo(bytesClone, 0);
                Array.Reverse(bytesClone);
                return BitConverter.ToUInt32(bytesClone, 0);
            }
            else
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public static byte[] GetBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (false == BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static byte[] GetBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (false == BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (false == BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

    }
}