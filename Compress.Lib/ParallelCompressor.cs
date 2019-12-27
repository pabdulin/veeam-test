using System.IO;

namespace Compress.Lib
{
    public class ParallelCompressor
    {
        public bool Compress(int blockSize, Stream inputUncompressed, Stream outputCompressed)
        {
            var blocks = (int)(inputUncompressed.Length / blockSize);
            blocks += (inputUncompressed.Length % blockSize > 0) ? 1 : 0;
            var buf = new byte[blockSize];
            var offset = 0;
            var totalRead = 0;
            byte[] dataForCompression;
            using (var bw = new BinaryWriter(outputCompressed))
            {
                bw.Write(blocks);
                while (totalRead < inputUncompressed.Length)
                {
                    var actualRead = inputUncompressed.Read(buf, offset, blockSize);
                    if (actualRead < buf.Length)
                    {
                        dataForCompression = new byte[actualRead];
                        System.Array.Copy(buf, 0, dataForCompression, 0, actualRead);
                    }
                    else
                    {
                        dataForCompression = buf;
                    }
                    var compressedBlock = CompressBlock(dataForCompression);
                    bw.Write(compressedBlock);
                    totalRead += actualRead;
                }
                bw.Flush();
            }
            return true;
        }

        public bool Decompress(Stream inputComressed, Stream outputUncompressed)
        {
            using (var br = new BinaryReader(inputComressed))
            using (var bw = new BinaryWriter(outputUncompressed))
            {
                var blocksCount = br.ReadInt32();
                for (int i = 0; i < blocksCount; i += 1)
                {
                    var blockSize = br.ReadInt32();
                    var blockData = br.ReadBytes(blockSize);
                    var uncompressedData = DecompressData(blockData);
                    bw.Write(uncompressedData);
                }
                bw.Flush();
            }
            return true;
        }

        public byte[] CompressBlock(byte[] dataUncompressed)
        {
            using (var ms = new MemoryStream())
            using (var result = new BinaryWriter(ms))
            {
                var compressedData = CompressData(dataUncompressed);
                result.Write(compressedData.Length);
                result.Write(compressedData);
                result.Flush();
                return ms.ToArray();
            }
        }

        private byte[] CompressData(byte[] dataUncompressed)
        {
            return dataUncompressed;
        }

        private byte[] DecompressData(byte[] dataCompressed)
        {
            return dataCompressed;
        }
    }
}
