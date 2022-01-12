# HexZipper

Zips the binary data of two or more files.

## Usage

```
hexzipper <options> <out-filename>
  Options:
    --help, -h       Displays this message.
    --input, -i      The input filename. Must be provided at least twice.
    --num-bytes, -n  The number of bytes. Must be provided at least twice.
  <out-filename>     The filename of the resulting zipped data.
Examples:
  hexzipper -i file1.dat -n 32 -i file2.dat -n 16 outfile.dat
    Zips 32 bytes from file1.dat with 16 bytes from file2.dat
    and writes the result to outfile.dat
```

## Requirements

- .NET Framework 4.7.2 or later

## Download

See [Releases](https://github.com/jordanbtucker/HexZipper/releases).
