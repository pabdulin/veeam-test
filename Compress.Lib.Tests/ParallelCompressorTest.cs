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
            var inputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test.bin.comp";
            var outputFile = @"c:\dev\veeam-test\GZipTest\bin\Debug\netcoreapp3.1\test-decompressed.bin";
            var pc = new Compress.Lib.ParallelCompressor();
            using (var inStream = File.OpenRead(inputFile))
            using (var outStream = File.OpenWrite(outputFile))
            {
                pc.Decompress(inStream, outStream);
            }
        }
    }
}
