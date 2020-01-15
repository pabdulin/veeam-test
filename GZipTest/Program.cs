using Compress.Lib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public class Program
    {
        private const string CompressCommand = "compress";
        private const string DecompressCommand = "decompress";

        public static int Main(string[] args)
        {
            try
            {
                if (IsValidArguments(args))
                {
                    HandleValidCall(args);
                }
                else
                {
                    HandleInvalidCall();
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Where was an error while trying to execute the program. The error message was:");
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Detailed information is below:");
                Console.WriteLine(e.ToString());
            }
            return 1;
        }

        private static bool IsValidArguments(string[] args)
        {
            var knownCommands = new[] { CompressCommand, DecompressCommand };
            if (args.Length == 3)
            {
                var op = args[0];
                if (knownCommands.Contains(op, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Command '{op}' is unknown. Possible commands are: '{string.Join("', '", knownCommands)}'.");
                }
            }

            return false;
        }

        private static void HandleInvalidCall()
        {
            Console.WriteLine("Unexpected arguments. Usage:");
            Console.WriteLine("GZipTest [de]compress input.file output.file");
        }

        private static void HandleValidCall(string[] args)
        {
            var op = args[0];
            var inputFile = args[1];
            var outputFile = args[2];
            Stopwatch timer = new Stopwatch();
            Console.WriteLine($"Executing command '{op}' using up to {Environment.ProcessorCount} threads.");
            timer.Restart();
            if (string.Compare(op, CompressCommand, StringComparison.OrdinalIgnoreCase) == 0)
            {
                CompressFile(inputFile, outputFile);
            }
            else if (string.Compare(op, DecompressCommand, StringComparison.OrdinalIgnoreCase) == 0)
            {
                DecompressFile(inputFile, outputFile);
            }
            timer.Stop();
            Console.WriteLine($"Operation completed in {timer.Elapsed.ToString()}.");

        }

        private static void DecompressFile(string inputFile, string outputFile)
        {
            var pc = new ParallelCompressor();
            using (var inStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (var outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                pc.Decompress(inStream, outStream);
            }
        }

        private static void CompressFile(string inputFile, string outputFile)
        {
            var pc = new ParallelCompressor();
            using (var inStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (var outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                pc.Compress(inStream, outStream);
            }
        }
    }
}
