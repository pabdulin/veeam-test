using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.Compression;

namespace Compress.Lib
{
    public class ParallelCompressor
    {
        private int _compressionBlockSize;
        private readonly object _threadLock = new object();
        private int _runningThreads;

        public ParallelCompressor(int compressionBlockSize)
        {
            _compressionBlockSize = compressionBlockSize;
        }

        public bool Compress(Stream inputUncompressed, Stream outputCompressed)
        {
            var workBlocksCount = (int)(inputUncompressed.Length / _compressionBlockSize);
            workBlocksCount += (inputUncompressed.Length % _compressionBlockSize > 0) ? 1 : 0;
            var compressionWorkThreads = new List<Thread>();
            _runningThreads = 0;

            using (var outputWriter = new BinaryWriter(outputCompressed))
            {
                outputWriter.Write(workBlocksCount);

                for (int currentWorkBlock = 0; currentWorkBlock < workBlocksCount; currentWorkBlock += 1)
                {
                    if (_runningThreads >= Environment.ProcessorCount)
                    {
                        MaxReasonableThreadsIdle();
                        continue;
                    }

                    var buf = new byte[_compressionBlockSize];
                    var actualRead = inputUncompressed.Read(buf, 0, _compressionBlockSize);
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
                }

                foreach (var compressionThread in compressionWorkThreads)
                {
                    if (compressionThread.IsAlive)
                    {
                        compressionThread.Join();
                    }
                }
            }

            return true;
        }

        private static void MaxReasonableThreadsIdle()
        {
            // TODO remove debug
            Console.WriteLine($"Nothing to do, sleep a bit...");
            Thread.Sleep(10);
        }

        private void ThreadCompressWork(object obj)
        {
            var threadData = (ThreadCompressWorkData)obj;
            // TODO: remove debug
            Console.WriteLine($"New thread for block index={threadData.BlockIndex} started");

            var compressedBlock = GetCompressedBlockData(threadData);
#if DEBUG
            EmulateWorkTime(threadData);
#endif
            threadData.CompressedBlock = compressedBlock;
            lock (_threadLock)
            {
                threadData.OutputWriter.Write(threadData.CompressedBlock);
                _runningThreads -= 1;
            }
            Console.WriteLine($"Thread for block index={threadData.BlockIndex} completed and written to output stream");
        }

        private static void EmulateWorkTime(ThreadCompressWorkData threadData)
        {
            Thread.Sleep(threadData.BlockIndex % 10);
        }

        public bool Decompress(Stream inputCompressed, Stream outputUncompressed)
        {
            var decompressionWorkThreads = new List<Thread>();
            _runningThreads = 0;

            using (var br = new BinaryReader(inputCompressed))
            using (var bw = new BinaryWriter(outputUncompressed))
            {
                var blocksCount = br.ReadInt32();

                for (int i = 0; i < blocksCount; i += 1)
                {
                    var blockIndex = br.ReadInt32();
                    var compressedBlockSize = br.ReadInt32();
                    var blockData = br.ReadBytes(compressedBlockSize);

                    var decompressionWork = new Thread(ThreadDecompressWork);
                    decompressionWorkThreads.Add(decompressionWork);
                    var decompressionWorkData = new ThreadDecompressWorkData(blockIndex, blockData, bw);
                    _runningThreads += 1;
                    decompressionWork.Start(decompressionWorkData);
                }

                foreach (var dt in decompressionWorkThreads)
                {
                    if (dt.IsAlive)
                    {
                        dt.Join();
                    }
                }
            }

            return true;
        }

        private void ThreadDecompressWork(object obj)
        {
            var threadData = (ThreadDecompressWorkData)obj;
            // TODO: remove debug
            Console.WriteLine($"New decompression thread for block={threadData.BlockIndex} started");

            var uncompressedData = DecompressData(threadData.CompressedData);
            lock (_threadLock)
            {
                threadData.OutputStream.Seek(threadData.BlockIndex * _compressionBlockSize, SeekOrigin.Begin);
                threadData.OutputStream.Write(uncompressedData);
                _runningThreads -= 1;
            }
            Console.WriteLine($"Thread for block={threadData.BlockIndex} completed and written to output stream");
        }

        public byte[] GetCompressedBlockData(ThreadCompressWorkData workData)
        {
            var compressedData = CompressData(workData.DataForCompression);
            using (var ms = new MemoryStream())
            using (var result = new BinaryWriter(ms))
            {
                result.Write(workData.BlockIndex);
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
