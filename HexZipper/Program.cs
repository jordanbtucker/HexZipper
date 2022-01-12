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
            List<int> byteLengths = new List<int>();
            string outFilename = null;

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
                    default:
                        outFilename = arg;
                        break;
                }
            }

            if (inFilenames.Count < 2 || byteLengths.Count < 2 || inFilenames.Count != byteLengths.Count || outFilename == null)
            {
                Usage();
                return 1;
            }

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
                var reader = readers[i];
                var inFilename = inFilenames[i];
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                    finishedClean = false;
                    Console.WriteLine($"Did not reach the end of {inFilename}. {remaining} byte(s) remain.");
                }
            }

            return finishedClean ? 0 : 1;
        }

        static void Usage()
        {
            Console.WriteLine("HexZipper");
            Console.WriteLine("Zips the binary data of two or more files.");
            Console.WriteLine();
            Console.WriteLine("hexzipper <options> <out-filename>");
            Console.WriteLine("  Options:");
            Console.WriteLine("    --help, -h       Displays this message.");
            Console.WriteLine("    --input, -i      The input filename. Must be provided at least twice.");
            Console.WriteLine("    --num-bytes, -n  The number of bytes. Must be provided at least twice.");
            Console.WriteLine("  <out-filename>     The filename of the resulting zipped data.");
            Console.WriteLine("Examples:");
            Console.WriteLine("  hexzipper -i file1.dat -n 32 -i file2.dat -n 16 outfile.dat");
            Console.WriteLine("    Zips 32 bytes from file1.dat with 16 bytes from file2.dat");
            Console.WriteLine("    and writes the result to outfile.dat");
        }
    }
}
