using System.Text;

namespace Perma.Engine
{
    internal class StringSerializer
    {
        public byte[] Serialize(string value)
        {
            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(value);
            byte[] data = new byte[4 + stringBytes.Length];

            BufferWriter.WriteBuffer((int)stringBytes.Length, data, 0);
            Buffer.BlockCopy(src: stringBytes, srcOffset: 0, dst: data, dstOffset: 4, count: stringBytes.Length);
            return data;
        }

        public string Deserializer(byte[] data)
        {
            int breedLength = BufferReader.ReadBufferInt32(data, 0);
            
            string text = System.Text.Encoding.UTF8.GetString(data, 4, breedLength);
            return text;
        }
    }
}