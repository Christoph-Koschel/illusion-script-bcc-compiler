using System.IO;

namespace IllusionScript.Compiler.BCC
{
    public static class StreamExtension
    {
        public static void WriteBytes(this StreamWriter writer, byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                writer.BaseStream.WriteByte(b);
            }
        }

        public static void WriteBytes(this StreamWriter writer, byte[] bytes, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                byte b = bytes[i];
                writer.BaseStream.WriteByte(b);
            }
        }
    }
}