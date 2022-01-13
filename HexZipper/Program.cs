using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexZipper
{
    internal class Program
    {
        static int Main(string[] args)
        {
            List<string> inFilenames = new List<string>();
            List<string> outFilenames = new List<string>();
            List<int> byteLengths = new List<int>();
            bool isZip = true;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--help":
                    case "-h":
                        Usage();
                        return 0;
                    case "--num-bytes":
                    case "-n":
                        byteLengths.Add(int.Parse(args[++i]));
                        break;
                    case "--input":
                    case "-i":
                        inFilenames.Add(args[++i]);
                        break;
                    case "--output":
                    case "-o":
                        outFilenames.Add(args[++i]);
                        break;
                    case "--unzip":
                    case "-u":
                        isZip = false;
                        break;
                    default:
                        Console.WriteLine($"Unknown argument '{arg}'");
                        Console.WriteLine();
                        Usage();
                        return 1;
                }
            }

            if ((isZip && (
                  inFilenames.Count < 2 ||
                  inFilenames.Count != byteLengths.Count ||
                  outFilenames.Count > 0)) ||
                (!isZip && (
                  outFilenames.Count < 2 ||
                  outFilenames.Count != byteLengths.Count ||
                  inFilenames.Count > 0)))
            {
                Usage();
                return 1;
            }

            if (isZip)
            {
                var outFilename = outFilenames[0];
                var inStreams = inFilenames.Select(f => File.OpenRead(f));
                var readers = inStreams.Select(s => new BinaryReader(s)).ToArray();
                using (var outStream = File.OpenWrite(outFilename))
                using (var writer = new BinaryWriter(outStream))
                {
                    while (readers.Select((reader, index) => new
                    {
                        Reader = reader,
                        ByteLength = byteLengths[index]
                    }).All(info => info.Reader.BaseStream.Position + info.ByteLength - 1 < info.Reader.BaseStream.Length))
                    {
                        for (var i = 0; i < readers.Length; i++)
                        {
                            var reader = readers[i];
                            var byteLength = byteLengths[i];
                            var bytes = reader.ReadBytes(byteLength);
                            writer.Write(bytes);
                        }
                    }
                }

                var finishedClean = true;
                for (var i = 0; i < readers.Length; i++)
                {
                    using (var reader = readers[i])
                    {
                        var inFilename = inFilenames[i];
                        if (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                            finishedClean = false;
                            Console.WriteLine($"Did not reach the end of {inFilename}. {remaining} byte(s) remain.");
                        }
                    }
                }

                return finishedClean ? 0 : 1;
            }
            else
            {
                var inFilename = inFilenames[0];
                var outStreams = outFilenames.Select(f => File.OpenWrite(f));
                var writers = outStreams.Select(s => new BinaryWriter(s)).ToArray();
                using (var inStream = File.OpenRead(inFilename))
                using (var reader = new BinaryReader(inStream))
                {
                    while (reader.BaseStream.Position + byteLengths.Sum() - 1 < reader.BaseStream.Length)
                    {
                        for (var i = 0; i < writers.Length; i++)
                        {
                            var writer = writers[i];
                            var byteLength = byteLengths[i];
                            var bytes = reader.ReadBytes(byteLength);
                            writer.Write(bytes);
                        }
                    }

                    var finishedClean = reader.BaseStream.Position == reader.BaseStream.Length;
                    if (!finishedClean)
                    {
                        var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                        Console.WriteLine($"Did not reach the end of {inFilename}. {remaining} byte(s) remain.");
                    }

                    foreach (var writer in writers)
                        writer.Dispose();

                    return finishedClean ? 0 : 1;
                }
            }
        }

        static void Usage()
        {
            Console.WriteLine("HexZipper");
            Console.WriteLine("Zips (or unzips) the binary data of two or more files.");
            Console.WriteLine();
            Console.WriteLine("hexzipper <arguments>");
            Console.WriteLine("  Arguments:");
            Console.WriteLine("    --help, -h       Displays this message.");
            Console.WriteLine("    --unzip, -u      Unzip input files instead of zipping.");
            Console.WriteLine("    --input, -i      The input filename(s). Must be provided at least twice if zipping.");
            Console.WriteLine("    --output, -o     The output filename(s). Must be provided at least twice if unzipping.");
            Console.WriteLine("    --num-bytes, -n  The number of bytes. Must be provided at least twice.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  hexzipper -i input1.dat -n 32 -i input2.dat -n 16 output.dat");
            Console.WriteLine("    Zips 32 bytes from input1.dat with 16 bytes from input2.dat");
            Console.WriteLine("    and writes the result to output.dat.");
            Console.WriteLine();
            Console.WriteLine("  hexzipper -u -i input.dat -o output1.dat -n 32 output2.dat -n 16");
            Console.WriteLine("    Unzips input.dat storing 32 bytes into output1.dat");
            Console.WriteLine("    and 16 bytes into output2.dat.");
        }
    }
}
