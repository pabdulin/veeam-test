using System.IO;

namespace Compress.Lib
{
    public class ThreadDecompressWorkData
    {
        public readonly int BlockIndex;
        public readonly byte[] CompressedData;
        public readonly Stream OutputStream;
        public readonly int BlockSize;

        public ThreadDecompressWorkData(int blockIndex, byte[] compressedData, int blockSize, Stream outputStream)
        {
            BlockIndex = blockIndex;
            CompressedData = compressedData;
            OutputStream = outputStream;
            BlockSize = blockSize;
        }
    }
}