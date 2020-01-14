using System.IO;

namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public int BlockIndex;
        public byte[] DataForCompression;
        public readonly BinaryWriter OutputWriter;
        public byte[] CompressedBlock;

        public ThreadCompressWorkData(int index, byte[] dataForCompression, System.IO.BinaryWriter outputWriter)
        {
            BlockIndex = index;
            DataForCompression = dataForCompression;
            OutputWriter = outputWriter;
        }
    }
}