using FloatingPointCompressor;
internal class Program
{
    private static async Task Main(string[] args)
    {       

        // Uncomment to run benchmark.
        //var summary = BenchmarkRunner.Run<FloatCompressorBenchmark>();

        float[] valuesToCompress = {
                10.123f, 11.2364f, 12.345f, -13.456f, 14.567f, 115.678f, 16.789f, 17.890f, -18.901f, 19.012f
        };

        Precision precision = Precision.TenThousandths;
        Console.WriteLine($"Compressing to {precision} precision.");

        FloatCompressor<float> fc = new FloatCompressor<float>(valuesToCompress, precision);
        byte[] compressedData = fc.Compress();
        float[] decompressedValues = fc.Decompress(compressedData);

        Console.WriteLine("\nOriginal values: " + string.Join(", ", valuesToCompress));
        Console.WriteLine("Compressed values: " + string.Join(", ", compressedData));
        Console.WriteLine("Decompressed values: " + string.Join(", ", decompressedValues));

        float[] errors = new float[valuesToCompress.Length];
        for (int i = 0; i < valuesToCompress.Length; i++)
        {
            errors[i] = decompressedValues[i] - valuesToCompress[i];
        }

        float meanError = errors.Average();
        float meanAbsoluteError = errors.Select(Math.Abs).Average();
        float maxError = errors.Max(Math.Abs);

        Console.WriteLine("\nError Metrics:");
        Console.WriteLine($"Mean Error (negative is underestimation): {meanError}");
        Console.WriteLine($"Mean Absolute Error (deviation): {meanAbsoluteError}");
        Console.WriteLine($"Maximum Error (worst case): {maxError}");
        Console.WriteLine("\nError Distribution:");
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error}");
        }

        int originalSize = valuesToCompress.Length * sizeof(float);
        int compressedSize = compressedData.Length;
        Console.WriteLine($"\nCompression Ratio: {(float)originalSize / compressedSize:F2}");

        Console.WriteLine("\nPacked Compressed Data (byte-level representation):");
        foreach (byte b in compressedData)
        {
            Console.WriteLine($"Byte: {b} (Binary: {ConvertByteToBits(b)})");
        }

        Console.WriteLine("\nPacked Values as Raw Bytes (Before Compression):");
        foreach (float value in valuesToCompress)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            Console.WriteLine($"{value} -> {BitConverter.ToString(valueBytes)} (Binary: {ConvertByteArrayToBits(valueBytes)})");
        }
    }

    private static string ConvertByteToBits(byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    private static string ConvertByteArrayToBits(byte[] byteArray)
    {
        return string.Join(" ", byteArray.Select(ConvertByteToBits));
    }

}
