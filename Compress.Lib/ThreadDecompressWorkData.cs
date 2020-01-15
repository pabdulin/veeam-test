using System.IO;

namespace Compress.Lib
{
    public class ThreadDecompressWorkData
    {
        public readonly int BlockIndex;
        public readonly byte[] CompressedData;
        public readonly BinaryWriter OutputStream;
        public readonly int BlockSize;

        public ThreadDecompressWorkData(int blockIndex, byte[] blockData, BinaryWriter bw, int blockSize)
        {
            BlockIndex = blockIndex;
            CompressedData = blockData;
            OutputStream = bw;
            BlockSize = blockSize;
        }
    }
}