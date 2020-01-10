using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Compress.Lib.Tests
{
    [TestClass]
    public class ParallelCompressorTest
    {
        private const int blockSize = 1024 * 1024;

        [TestMethod]
        public void TestDecompress()
        {
            var inputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test_empty_0-compressed-copy.out";
            var outputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test_empty_0-decompressed.out";
            var pc = new Compress.Lib.ParallelCompressor();
            using (var inStream = File.OpenRead(inputFile))
            using (var outStream = File.OpenWrite(outputFile))
            {
                pc.Decompress(inputComressed: inStream, outputUncompressed: outStream);
            }
        }

        [TestMethod]
        public void TestCompress()
        {
            var inputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test_empty_0.bin";
            var outputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test_empty_0-compressed.out";
            var pc = new Compress.Lib.ParallelCompressor();
            using (var inStream = File.OpenRead(inputFile))
            using (var outStream = File.OpenWrite(outputFile))
            {
                pc.Compress(blockSize, inStream, outStream);
            }
        }
    }
}
