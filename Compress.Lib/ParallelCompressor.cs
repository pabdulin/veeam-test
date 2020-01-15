using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.Compression;

namespace Compress.Lib
{
    public class ParallelCompressor
    {
        private const int ArchiveMagic = 0x00076ED2;
        private const int DefaultBlockSize = 1024 * 1024;
        private readonly object _threadLock = new object();
        private int _runningThreads;

        public bool Compress(Stream inputUncompressed, Stream outputCompressed, int compressionBlockSize = DefaultBlockSize)
        {
            var workBlocksCount = (int)(inputUncompressed.Length / compressionBlockSize);
            workBlocksCount += (inputUncompressed.Length % compressionBlockSize > 0) ? 1 : 0;
            var compressionWorkThreads = new List<Thread>();

            using (var outputWriter = new BinaryWriter(outputCompressed))
            {
                outputWriter.Write(ArchiveMagic);
                outputWriter.Write(compressionBlockSize);
                outputWriter.Write(workBlocksCount);

                _runningThreads = 0;
                var currentWorkBlock = 0;
                while (currentWorkBlock <= workBlocksCount)
                {
                    if (_runningThreads >= Environment.ProcessorCount)
                    {
                        MaxReasonableThreadsIdle();
                        continue;
                    }

                    var buf = new byte[compressionBlockSize];
                    var actualRead = inputUncompressed.Read(buf, 0, compressionBlockSize);
                    byte[] dataForCompression;
                    if (actualRead < buf.Length)
                    {
                        dataForCompression = new byte[actualRead];
                        Array.Copy(buf, 0, dataForCompression, 0, actualRead);
                    }
                    else
                    {
                        dataForCompression = buf;
                    }

                    var compressionWork = new Thread(ThreadCompressWork);
                    compressionWorkThreads.Add(compressionWork);
                    var compressionWorkData = new ThreadCompressWorkData(currentWorkBlock, dataForCompression, outputWriter);
                    _runningThreads += 1;
                    compressionWork.Start(compressionWorkData);
                    currentWorkBlock += 1;
                }
                AwaitRemainingRunningThreads(compressionWorkThreads);
            }

            return true;
        }

        public bool Decompress(Stream inputCompressed, Stream outputUncompressed)
        {
            var decompressionWorkThreads = new List<Thread>();

            using (var compressedInput = new BinaryReader(inputCompressed))
            using (var decompressedOutput = new BinaryWriter(outputUncompressed))
            {
                var magic = compressedInput.ReadInt32();
                if (magic != ArchiveMagic)
                {
                    throw new ArgumentException("Input stream doesn't appear to be a valid archive data", nameof(inputCompressed));
                }
                var blockSize = compressedInput.ReadInt32();
                var blocksCount = compressedInput.ReadInt32();

                _runningThreads = 0;
                var currentWorkBlock = 0;
                while (currentWorkBlock <= blocksCount)
                {
                    if (_runningThreads >= Environment.ProcessorCount)
                    {
                        MaxReasonableThreadsIdle();
                        continue;
                    }

                    var blockIndex = compressedInput.ReadInt32();
                    var compressedBlockSize = compressedInput.ReadInt32();
                    var blockData = compressedInput.ReadBytes(compressedBlockSize);

                    var decompressionWork = new Thread(ThreadDecompressWork);
                    decompressionWorkThreads.Add(decompressionWork);
                    var decompressionWorkData = new ThreadDecompressWorkData(blockIndex, blockData, decompressedOutput, blockSize);
                    _runningThreads += 1;
                    decompressionWork.Start(decompressionWorkData);
                    currentWorkBlock += 1;
                }
                AwaitRemainingRunningThreads(decompressionWorkThreads);
            }

            return true;
        }

        private static void AwaitRemainingRunningThreads(List<Thread> threads)
        {
            foreach (var compressionThread in threads)
            {
                if (compressionThread.IsAlive)
                {
                    compressionThread.Join();
                }
            }
        }

        private static void MaxReasonableThreadsIdle()
        {
            Thread.Sleep(10);
        }

        private void ThreadCompressWork(object obj)
        {
            var threadData = (ThreadCompressWorkData)obj;
            using (MemoryStream compressedBlock = new MemoryStream())
            {
                GetCompressedBlockData(threadData, compressedBlock);
                compressedBlock.Seek(0, SeekOrigin.Begin);
                lock (_threadLock)
                {
                    compressedBlock.CopyTo(threadData.OutputWriter.BaseStream);
                    _runningThreads -= 1;
                }
            }
        }

        private void ThreadDecompressWork(object obj)
        {
            var threadData = (ThreadDecompressWorkData)obj;
            using (MemoryStream decompressedOutput = new MemoryStream())
            {
                GetDecompressedBlockData(threadData.CompressedData, decompressedOutput);
                decompressedOutput.Seek(0, SeekOrigin.Begin);
                lock (_threadLock)
                {
                    threadData.OutputStream.Seek(threadData.BlockIndex * threadData.BlockSize, SeekOrigin.Begin);
                    decompressedOutput.CopyTo(threadData.OutputStream.BaseStream);
                    _runningThreads -= 1;
                }
            }
        }

        private void GetCompressedBlockData(ThreadCompressWorkData workData, MemoryStream compressedBlockOutput)
        {
            using (MemoryStream compressedOutput = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(compressedOutput, CompressionLevel.Optimal, leaveOpen: true))
                {
                    compressionStream.Write(workData.DataForCompression, 0, workData.DataForCompression.Length);
                }
                int compressedDataLength = (int)compressedOutput.Length;
                using (var binaryWriter = new BinaryWriter(compressedBlockOutput, System.Text.Encoding.Default, leaveOpen: true))
                {
                    binaryWriter.Write(workData.BlockIndex);
                    binaryWriter.Write(compressedDataLength);
                    compressedOutput.Seek(0, SeekOrigin.Begin);
                    compressedOutput.CopyTo(binaryWriter.BaseStream);
                }
            }
        }

        private void GetDecompressedBlockData(byte[] dataCompressed, MemoryStream decompressedOutput)
        {
            using (MemoryStream compressedInput = new MemoryStream(dataCompressed))
            {
                using (GZipStream decompressionStream = new GZipStream(compressedInput, CompressionMode.Decompress, leaveOpen: true))
                {
                    decompressionStream.CopyTo(decompressedOutput);
                }
            }
        }
    }
}
