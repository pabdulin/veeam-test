namespace Compress.Lib
{
    internal class ThreadCompressionWorkResult
    {
        public int Index { get; private set; }
        public byte[] CompressedBlock { get; private set; }

        public ThreadCompressionWorkResult(int index, byte[] compressedBlock)
        {
            Index = index;
            CompressedBlock = compressedBlock;
        }
    }
}