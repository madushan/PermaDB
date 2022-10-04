namespace Perma.Engine
{
    internal static class BufferWriter
    {
        public static void WriteBuffer(int value, byte[] buffer, int bufferOffset)
        {
            Buffer.BlockCopy(LittleEndian.GetBytes((int)value), 0, buffer, bufferOffset, 4);
        }

        public static void WriteBuffer(long value, byte[] buffer, int bufferOffset)
        {
            Buffer.BlockCopy(LittleEndian.GetBytes(value), 0, buffer, bufferOffset, 8);
        }
    }
}