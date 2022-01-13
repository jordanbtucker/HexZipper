using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HexZipper
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Header();
                Usage();
                return 1;
            }

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
                        Header();
                        Usage();
                        return 0;
                    case "--num-bytes":
                    case "-n":
                        if (i == args.Length - 1) return Error($"Missing value for argument {arg}.", true);
                        byteLengths.Add(int.Parse(args[++i]));
                        break;
                    case "--input":
                    case "-i":
                        if (i == args.Length - 1) return Error($"Missing value for argument {arg}.", true);
                        inFilenames.Add(args[++i]);
                        break;
                    case "--output":
                    case "-o":
                        if (i == args.Length - 1) return Error($"Missing value for argument {arg}.", true);
                        outFilenames.Add(args[++i]);
                        break;
                    case "--unzip":
                    case "-u":
                        isZip = false;
                        break;
                    default:
                        return Error($"Unknown argument '{arg}'.", true);
                }
            }

            string errorMessage = null;
            if (isZip)
            {
                if (inFilenames.Count < 2) errorMessage = "--input must be provided at least twice when zipping.";
                else if (inFilenames.Count != byteLengths.Count) errorMessage = "--num-bytes must be provided once for each --input when zipping.";
                else if (outFilenames.Count != 1) errorMessage = "--output must be provided once when zipping.";
            }
            else
            {
                if (inFilenames.Count != 1) errorMessage = "--input must be provided once when unzipping.";
                else if (outFilenames.Count < 2) errorMessage = "--output must be provided at least twice when unzipping.";
                else if (outFilenames.Count != byteLengths.Count) errorMessage = "--num-bytes must be provided once for each --output when unzipping.";
            }

            if (errorMessage != null)
                return Error(errorMessage, true);

            try
            {
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
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        static void Header()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"HexZipper v{version.Major}.{version.Minor}.{version.Revision}");
            Console.WriteLine("Zips and unzips the binary data of two or more files.");
            Console.WriteLine("https://github.com/jordanbtucker/HexZipper");
            Console.WriteLine();
        }

        static void Usage()
        {
            Console.WriteLine("HexZipper <arguments>");
            Console.WriteLine("  Arguments:");
            Console.WriteLine("    --help, -h       Display this message.");
            Console.WriteLine("    --unzip, -u      Unzip input files instead of zipping.");
            Console.WriteLine("    --input, -i      The input filename(s). Must be provided at least twice if");
            Console.WriteLine("                     zipping.");
            Console.WriteLine("    --output, -o     The output filename(s). Must be provided at least twice if");
            Console.WriteLine("                     unzipping.");
            Console.WriteLine("    --num-bytes, -n  The number of bytes in the block. Must be provided at");
            Console.WriteLine("                     least twice.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  HexZipper -i input1.dat -n 32 -i input2.dat -n 16 output.dat");
            Console.WriteLine("    Zips 32-byte blocks from input1.dat with 16-byte blocks from input2.dat and");
            Console.WriteLine("    writes the result to output.dat.");
            Console.WriteLine();
            Console.WriteLine("  HexZipper -u -i input.dat -o output1.dat -n 32 output2.dat -n 16");
            Console.WriteLine("    Unzips input.dat storing 32-byte blocks into output1.dat and 16-byte blocks");
            Console.WriteLine("    into output2.dat.");
        }

        static int Error(string message, bool displayUsage = false)
        {
            if (displayUsage)
            {
                Usage();
                Console.WriteLine();
            }

            Console.WriteLine(message);
            return 1;
        }
    }
}
