using BenchmarkDotNet.Running;
using FloatingPointCompressor;
using System.Globalization;
using FloatingPointCompressor.Tests;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Uncomment to run benchmark.
        // var summary = BenchmarkRunner.Run<FloatCompressorBenchmark>();

        // Use a sample without extreme outliers for correct compression
        float[] scientificFloatValues = {
            5.54500008f,
            -7.55112505f,
            123456.789f,
            -98765.4297f,
            3.1415925f,
            -2.71828175f,
            1.61803389f,
            -0.577215672f,
            299792.469f
        };
        double[] scientificDoubleValues = {
            5.54500008,
            -7.55112505,
            123456.789,
            -98765.4297,
            3.1415925,
            -2.71828175,
            1.61803389,
            -0.577215672,
            299792.469
        };

        // Only include precisions down to millionths (1e-6)
        var precisions = PrecisionExtensions.AllPrecisions;
            //.Where(p => p.Value >= 1e-6)
            //.ToArray();

        // FLOAT TESTS
        Console.WriteLine("\n[float] Original values:");
        foreach (var v in scientificFloatValues)
            Console.WriteLine(v.ToString("G9", CultureInfo.InvariantCulture));

        foreach (var precision in precisions)
        {
            var name = $"{precision.Value:G}";
            var compressor = new FloatCompressor<float>(scientificFloatValues, precision);
            byte[] compressed = compressor.Compress();
            float[] decompressed = compressor.Decompress(compressed);

            float[] errors = new float[scientificFloatValues.Length];
            for (int i = 0; i < scientificFloatValues.Length; i++)
                errors[i] = decompressed[i] - scientificFloatValues[i];

            float meanError = errors.Average();
            float meanAbsError = errors.Select(Math.Abs).Average();
            float maxError = errors.Max(Math.Abs);
            int originalSize = scientificFloatValues.Length * sizeof(float);
            int compressedSize = compressed.Length;
            float ratio = compressedSize == 0 ? 0 : (float)originalSize / compressedSize;

            Console.WriteLine($"\n--- [float] Precision: {name} ---");
            Console.WriteLine($"Compression Ratio: {ratio:F2}");
            Console.WriteLine($"Mean Error: {meanError:E}");
            //Console.WriteLine($"Mean Absolute Error: {meanAbsError:E}");
            //Console.WriteLine($"Maximum Error: {maxError:E}");
            Console.WriteLine("Decompressed values:");
            for (int i = 0; i < scientificFloatValues.Length; i++)
            {
                Console.WriteLine($"Original: {scientificFloatValues[i],20:G9} | Decompressed: {decompressed[i],20:G9} | Error: {errors[i],12:E}");
            }
        }

        // DOUBLE TESTS
        Console.WriteLine("\n[double] Original values:");
        foreach (var v in scientificDoubleValues)
            Console.WriteLine(v.ToString("G17", CultureInfo.InvariantCulture));

        foreach (var precision in precisions)
        {
            var name = $"{precision.Value:G}";
            var compressor = new FloatCompressor<double>(scientificDoubleValues, precision);
            byte[] compressed = compressor.Compress();
            double[] decompressed = compressor.Decompress(compressed);

            double[] errors = new double[scientificDoubleValues.Length];
            for (int i = 0; i < scientificDoubleValues.Length; i++)
                errors[i] = decompressed[i] - scientificDoubleValues[i];

            double meanError = errors.Average();
            double meanAbsError = errors.Select(Math.Abs).Average();
            double maxError = errors.Max(Math.Abs);
            int originalSize = scientificDoubleValues.Length * sizeof(double);
            int compressedSize = compressed.Length;
            double ratio = compressedSize == 0 ? 0 : (double)originalSize / compressedSize;

            Console.WriteLine($"\n--- [double] Precision: {name} ---");
            Console.WriteLine($"Compression Ratio: {ratio:F2}");
            Console.WriteLine($"Mean Error: {meanError:E}");
            //Console.WriteLine($"Mean Absolute Error: {meanAbsError:E}");
            //Console.WriteLine($"Maximum Error: {maxError:E}");
            Console.WriteLine("Decompressed values:");
            for (int i = 0; i < scientificDoubleValues.Length; i++)
            {
                Console.WriteLine($"Original: {scientificDoubleValues[i],20:G17} | Decompressed: {decompressed[i],20:G17} | Error: {errors[i],22:E}");
            }
        }

    }
}
