using System.IO;

namespace Compress.Lib
{
    public class ThreadDecompressWorkData
    {
        public int BlockIndex;
        public byte[] CompressedData;
        public BinaryWriter OutputStream;

        public ThreadDecompressWorkData(int blockIndex, byte[] blockData, BinaryWriter bw)
        {
            BlockIndex = blockIndex;
            CompressedData = blockData;
            OutputStream = bw;
        }
    }
}