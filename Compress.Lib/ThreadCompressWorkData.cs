namespace Compress.Lib
{
    public class ThreadCompressWorkData
    {
        public int BlockIndex { get; }
        public byte[] DataForCompression { get; private set; }
        public volatile byte[] CompressedBlock;

        public ThreadCompressWorkData(int index, byte[] dataForCompression)
        {
            BlockIndex = index;
            DataForCompression = dataForCompression;
        }
    }
}