using System.IO;

namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public int Index { get; }
        public byte[] DataForCompression { get; private set; }
        internal ThreadCompressionWorkResult CompressionResults { get; set; }

        public ThreadCompressWorkData(int index, byte[] dataForCompression)
        {
            Index = index;
            DataForCompression = dataForCompression;
        }
    }
}