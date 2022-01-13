# HexZipper

Zips and unzips the binary data of two or more files.

## Usage

```
HexZipper <arguments>
  Arguments:
    --help, -h       Display this message.
    --unzip, -u      Unzip input files instead of zipping.
    --input, -i      The input filename(s). Must be provided at least twice if
                     zipping.
    --output, -o     The output filename(s). Must be provided at least twice if
                     unzipping.
    --num-bytes, -n  The number of bytes in the block. Must be provided at
                     least twice.

Examples:
  HexZipper -i input1.dat -n 32 -i input2.dat -n 16 output.dat
    Zips 32-byte blocks from input1.dat with 16-byte blocks from input2.dat and
    writes the result to output.dat.

  HexZipper -u -i input.dat -o output1.dat -n 32 output2.dat -n 16
    Unzips input.dat storing 32-byte blocks into output1.dat and 16-byte blocks
    into output2.dat.
```

## Requirements

- [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
  or later

## Download

See [Releases](https://github.com/jordanbtucker/HexZipper/releases).
