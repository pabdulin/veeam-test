using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Compress.Lib
{
    public class ParallelCompressor
    {
        public bool Compress(int blockSize, Stream inputUncompressed, Stream outputCompressed)
        {
            var blocksCount = (int)(inputUncompressed.Length / blockSize);
            blocksCount += (inputUncompressed.Length % blockSize > 0) ? 1 : 0;
            //var buf = new byte[blockSize];
            var offset = 0;
            var totalRead = 0;
            //var compressionThreads = new List<Thread>();
            var compressionResults = new List<ThreadCompressWorkData>();
            var index = 0;
            var writeIndex = 0;

            using (var outputWriter = new BinaryWriter(outputCompressed))
            {
                outputWriter.Write(blocksCount);
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

                        Thread t = new Thread(ThreadCompressWork);
                        //compressionThreads.Add(t);
                        var cr = new ThreadCompressWorkData(index, dataForCompression);
                        compressionResults.Add(cr);
                        t.Start(cr);
                        //t.Join();

                        //if(compressionThreads.Count == threadLimit)
                        //{
                        //    foreach(var compThread in compressionThreads)
                        //    {
                        //        compThread.Start();
                        //        compThread.Join();
                        //    }
                        //}

                        totalRead += actualRead;
                        index += 1;

                        //var completedThread = 
                    }
                    if (compressionResults.Count != 0)
                    {
                        var next = compressionResults.SingleOrDefault(cr => cr.Index == writeIndex && cr.CompressionResults != null);
                        if (next != null)
                        {
                            outputWriter.Write(next.CompressionResults.CompressedBlock);
                            compressionResults.Remove(next);
                            writeIndex += 1;
                        }
                    }
                }


                while (compressionResults.Count != 0)
                {
                    var next = compressionResults.SingleOrDefault(cr => cr.Index == writeIndex && cr.CompressionResults != null);
                    if (next != null)
                    {
                        outputWriter.Write(next.CompressionResults.CompressedBlock);
                        compressionResults.Remove(next);
                        writeIndex += 1;
                    }
                }

                outputWriter.Flush();
            }
            return true;
        }

        private void ThreadCompressWork(object obj)
        {
            var threadData = (ThreadCompressWorkData)obj;
            var compressedBlock = CompressBlock(threadData.DataForCompression);
            threadData.CompressionResults = new ThreadCompressionWorkResult(threadData.Index, compressedBlock);

            //threadData.OutputWriter.Write(compressedBlock);
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
