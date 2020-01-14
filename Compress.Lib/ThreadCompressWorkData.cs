namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public int Index { get; }
        public byte[] DataForCompression { get; private set; }
        public volatile byte[] CompressedBlock;

        public ThreadCompressWorkData(int index, byte[] dataForCompression)
        {
            Index = index;
            DataForCompression = dataForCompression;
        }
    }
}