namespace Perma.Engine
{
    internal static class BufferReader
    {
        internal static int ReadBufferInt32(byte[] buffer, int bufferOffset)
        {
            byte[] intBuffer = new byte[4];
            Buffer.BlockCopy(buffer, bufferOffset, intBuffer, 0, 4);
            return LittleEndian.GetInt32(intBuffer);
        }

        internal static long ReadBufferInt64(byte[] buffer, int bufferOffset)
        {
            byte[] longBuffer = new byte[8];
            Buffer.BlockCopy(buffer, bufferOffset, longBuffer, 0, 8);
            return LittleEndian.GetInt64(longBuffer);
        }

        internal static byte[] ReadHeader(byte[] buffer)
        {
            byte[] header = new byte[CommonConstants.HeaderSize];
            Buffer.BlockCopy(buffer, 0, header, 0, CommonConstants.HeaderSize);
            return header;
        }
    }
}