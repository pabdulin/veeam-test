using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.IO.Compression;

namespace Compress.Lib
{
    public class ParallelCompressor
    {
        public bool Compress(int blockSize, Stream inputUncompressed, Stream outputCompressed)
        {
            var workBlocksCount = (int)(inputUncompressed.Length / blockSize);
            workBlocksCount += (inputUncompressed.Length % blockSize > 0) ? 1 : 0;
            var offset = 0;
            var totalRead = 0;
            var compressionResults = new List<ThreadCompressWorkData>();
            var currentWorkBlock = 0;
            var nextWorkBlockToWriteIndex = 0;

            using (var outputWriter = new BinaryWriter(outputCompressed))
            {
                outputWriter.Write(workBlocksCount);
                while (totalRead < inputUncompressed.Length)
                {
                    while ((totalRead < inputUncompressed.Length) && compressionResults.Count < Environment.ProcessorCount)
                    {
                        var buf = new byte[blockSize];
                        var actualRead = inputUncompressed.Read(buf, offset, blockSize);
                        byte[] dataForCompression;
                        if (actualRead < buf.Length)
                        {
                            dataForCompression = new byte[actualRead];
                            System.Array.Copy(buf, 0, dataForCompression, 0, actualRead);
                        }
                        else
                        {
                            dataForCompression = buf;
                        }

                        var compressionWork = new Thread(ThreadCompressWork);
                        var compressionWorkData = new ThreadCompressWorkData(currentWorkBlock, dataForCompression);
                        compressionResults.Add(compressionWorkData);
                        compressionWork.Start(compressionWorkData);
                        // TODO: remove debug
                        Console.WriteLine($"New thread for block index={currentWorkBlock} started");

                        totalRead += actualRead;
                        currentWorkBlock += 1;

                        nextWorkBlockToWriteIndex = WriteFinishedThreads(compressionResults, nextWorkBlockToWriteIndex, outputWriter, waitTillEnd: false);
                    }
                    var prevIndex = nextWorkBlockToWriteIndex;
                    nextWorkBlockToWriteIndex = WriteFinishedThreads(compressionResults, nextWorkBlockToWriteIndex, outputWriter, waitTillEnd: false);
                    if (nextWorkBlockToWriteIndex - prevIndex == 0)
                    {
                        // TODO: remove debug
                        Console.WriteLine($"Nothing to do, sleep a bit...");
                        Thread.Sleep(10);
                    }
                }
                WriteFinishedThreads(compressionResults, nextWorkBlockToWriteIndex, outputWriter, waitTillEnd: true);
                outputWriter.Flush();
            }
            return true;
        }

        private static int WriteFinishedThreads(
            List<ThreadCompressWorkData> compressionResults,
            int nextWorkBlockToWriteIndex,
            BinaryWriter outputWriter,
            bool waitTillEnd)
        {
            while (compressionResults.Count != 0)
            {
                var nextCompletedWork = compressionResults.FirstOrDefault(cr => cr.Index == nextWorkBlockToWriteIndex && cr.CompressedBlock != null);
                if (nextCompletedWork != null)
                {
                    outputWriter.Write(nextCompletedWork.CompressedBlock);
                    compressionResults.Remove(nextCompletedWork);
                    // TODO: remove debug
                    Console.WriteLine($"Thread for block index={nextWorkBlockToWriteIndex} completed and written to output stream");
                    nextWorkBlockToWriteIndex += 1;
                }
                else
                {
                    if (!waitTillEnd)
                    {
                        break;
                    }
                    else
                    {
                        // TODO: remove debug
                        Console.WriteLine($"Nothing to do, sleep a bit...");
                        Thread.Sleep(10);
                    }
                }
            }

            return nextWorkBlockToWriteIndex;
        }

        private System.Random r = new Random();

        private void ThreadCompressWork(object obj)
        {
            var threadData = (ThreadCompressWorkData)obj;
            var compressedBlock = CompressBlock(threadData.DataForCompression);
            // TODO: remove debug
            Thread.Sleep(100 + r.Next(20, 40));
            threadData.CompressedBlock = compressedBlock;
        }

        public bool Decompress(Stream inputComressed, Stream outputUncompressed)
        {
            using (var br = new BinaryReader(inputComressed))
            using (var bw = new BinaryWriter(outputUncompressed))
            {
                var blocksCount = br.ReadInt32();
                for (int i = 0; i < blocksCount; i += 1)
                {
                    var compressedBlockSize = br.ReadInt32();
                    var blockData = br.ReadBytes(compressedBlockSize);
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
            using (MemoryStream compressedOutput = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(compressedOutput, CompressionLevel.Optimal, leaveOpen: true))
                {
                    compressionStream.Write(dataUncompressed, 0, dataUncompressed.Length);

                }
                return compressedOutput.ToArray();
            }
        }

        private byte[] DecompressData(byte[] dataCompressed)
        {

            using (MemoryStream decompressedOutput = new MemoryStream())
            {
                using (MemoryStream compressedInput = new MemoryStream(dataCompressed))
                {
                    using (GZipStream decompressionStream = new GZipStream(compressedInput, CompressionMode.Decompress, leaveOpen: true))
                    {
                        decompressionStream.CopyTo(decompressedOutput);
                    }
                }
                return decompressedOutput.ToArray();
            }
        }
    }
}
