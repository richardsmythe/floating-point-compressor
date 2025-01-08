# Floating Point Compressor

This is a PoC utility for compressing and decompressing arrays of floating-point numbers. The main goal is to reduce the size of floating-point data arrays by applying compression based on a specified precision level. This is useful for optimizing storage and transmission of numerical data where full precision is not always required. Additionally, the compressor utilizes low-level optimizations using **SIMD (Single Instruction, Multiple Data)** for high-speed processing, making it suitable for performance-sensitive applications.

## What It Does

The Floating Point Compressor allows you to:

- **Compress an array of floating-point numbers** into a byte array, reducing storage requirements.
- **Decompress a byte array** back into an array of floating-point numbers.
- **Adjust the precision** of the compressed values based on the desired accuracy (e.g., to the nearest hundredths, thousandths, or millionths).

### Key Features:
- **Precision Control**: You can choose from different precision levels such as Hundredths, Thousandths, and Millionths to control the accuracy of the compressed values.
- **Efficient Storage**: Reduces the size of floating-point data while preserving the required level of precision.
- **Lossless Decompression**: The decompression process restores the values.
- **High-Speed Compression**: The compressor uses SIMD to optimize speed, making it ideal for high-performance applications that require fast floating-point data handling.

## Precision Levels

The compressor supports the following precision levels, which define how many decimal places are retained in the floating-point values:

- **Tenths**: Values are compressed to 1 decimal place (e.g., `1.2`).
- **Hundredths**: Values are compressed to 2 decimal places (e.g., `1.23`).
- **Thousandths**: Values are compressed to 3 decimal places (e.g., `1.234`).
- **Ten Thousandths**: Values are compressed to 4 decimal places (e.g., `1.2345`).
- **Millionths**: Values are compressed to 6 decimal places (e.g., `1.234567`).

## Benchmark

The benchmark involved compressing and decompressing an array of **1,000,000** floating-point numbers with a precision of "tenths" (1 decimal place). The results are as follows:

### Benchmark Results

| Method              | Mean      | Error    | StdDev    |
|-------------------- |----------:|---------:|----------:|
| CompressBenchmark   |  69.30 ms | 1.262 ms |  1.964 ms |
| DecompressBenchmark | 146.32 ms | 4.639 ms | 13.236 ms |

## TODOs

- Switching to doubles would allow 15-16 decimal digits of extra precision, rather than 7.

## Contribution

Please feel free to contribute.

