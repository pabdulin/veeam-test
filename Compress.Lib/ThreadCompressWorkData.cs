using System.IO;

namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public readonly int BlockIndex;
        public readonly byte[] DataForCompression;
        public readonly BinaryWriter OutputWriter;

        public ThreadCompressWorkData(int index, byte[] dataForCompression, BinaryWriter outputWriter)
        {
            BlockIndex = index;
            DataForCompression = dataForCompression;
            OutputWriter = outputWriter;
        }
    }
}