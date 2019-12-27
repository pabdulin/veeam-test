using System;
using System.IO;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var blockSize = 1024 * 1024;
            if (args.Length == 3)
            {
                var op = args[0];
                var inputFile = args[1];
                var outputFile = args[2];
                if (string.Compare(op, "compress", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var pc = new Compress.Lib.ParallelCompressor();
                    using (var inStream = File.OpenRead(inputFile))
                    using (var outStream = File.OpenWrite(outputFile))
                    {
                        pc.Compress(blockSize, inStream, outStream);
                    }
                }
                else if (string.Compare(op, "decompress", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var pc = new Compress.Lib.ParallelCompressor();
                    using (var inStream = File.OpenRead(inputFile))
                    using (var outStream = File.OpenWrite(outputFile))
                    {
                        pc.Decompress(inStream, outStream);
                    }
                }
            }
            else
            {
                Console.WriteLine("Unexpected arguments count. Usage:");
                Console.WriteLine("GZipTest [de]compress input.file output.file");
            }
        }
    }
}
