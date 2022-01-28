# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- The `--help, -h` argument displays the correct usage for the `--output, -o`
  argument.

## [0.2.0] - 2021-01-12

### Breaking Changes

- The `--output, -o` argument has been added, and it must be provided.

  ```bash
  # Correct
  HexZipper -i in1.dat -n 32 -i in2.dat -n 16 -o out.dat

  # Incorrect
  HexZipper -i in1.dat -n 32 -i in2.dat -n 16 out.dat
  ```

### Added

- Files can be unzipped with the `--unzip, -u` argument now.

  ```bash
  # Example
  HexZipper -u -i in.dat -n 32 -o out1.dat -n 16 -o out2.dat
  ```

## [0.1.0] - 2021-01-12

### Added

- Initial implementation

[unreleased]: https://github.com/jordanbtucker/HexZipper/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/jordanbtucker/HexZipper/releases/tag/v0.2.0
[0.1.0]: https://github.com/jordanbtucker/HexZipper/releases/tag/v0.1.0
