using System.IO;

namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public readonly int BlockIndex;
        public readonly byte[] DataForCompression;
        public readonly int DataSize;
        public readonly BinaryWriter OutputWriter;

        public ThreadCompressWorkData(int index, byte[] dataForCompression, int dataSize, BinaryWriter outputWriter)
        {
            BlockIndex = index;
            DataForCompression = dataForCompression;
            DataSize = dataSize;
            OutputWriter = outputWriter;
        }
    }
}