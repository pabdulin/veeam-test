using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Compress.Lib.Tests
{
    [TestClass]
    public class ParallelCompressorTest
    {
        [TestMethod]
        public void TestDecompress()
        {
            var inputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test-compressed.out";
            var outputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test-decompressed.out";
            var pc = new Compress.Lib.ParallelCompressor();
            using (var inStream = File.OpenRead(inputFile))
            using (var outStream = File.OpenWrite(outputFile))
            {
                pc.Decompress(inStream, outStream);
            }
        }

        [TestMethod]
        public void TestCompress()
        {
            var inputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test.bin";
            var outputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test-compressed.out";
            var pc = new Compress.Lib.ParallelCompressor();
            using (var inStream = File.OpenRead(inputFile))
            using (var outStream = File.OpenWrite(outputFile))
            {
                pc.Compress(1024 * 1024, inStream, outStream);
            }
        }
    }
}
