using FloatingPointCompressor;
using System.Globalization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Scientific-like float values: small, large, positive, negative, high precision
        float[] scientificValues = {
            0.00012345f, -0.00098765f, 123456.789f, -98765.4321f, 3.1415926f, -2.7182818f, 1.6180339f, -0.5772157f, 299792.458f, -6.02214076e23f
        };

        var precisions = new[]
        {
            ("Thousandths", Precision.Thousandsth),
            ("TenThousandths", Precision.TenThousandths),
            ("Millionths", Precision.Millionths)
        };

        Console.WriteLine("Showcasing the power and utility of the FloatingPointCompressor for scientific data:");
        Console.WriteLine("\nOriginal values:");
        foreach (var v in scientificValues)
            Console.WriteLine(v.ToString("G9", CultureInfo.InvariantCulture));

        foreach (var (name, precision) in precisions)
        {
            FloatCompressor<float> compressor = new FloatCompressor<float>(scientificValues, precision);
            byte[] compressed = compressor.Compress();
            float[] decompressed = compressor.Decompress(compressed);

            float[] errors = new float[scientificValues.Length];
            for (int i = 0; i < scientificValues.Length; i++)
                errors[i] = decompressed[i] - scientificValues[i];

            float meanError = errors.Average();
            float meanAbsError = errors.Select(Math.Abs).Average();
            float maxError = errors.Max(Math.Abs);
            int originalSize = scientificValues.Length * sizeof(float);
            int compressedSize = compressed.Length;
            float ratio = (float)originalSize / compressedSize;

            Console.WriteLine($"\n--- Precision: {name} ({precision.Value:G}) ---");
            Console.WriteLine($"Compression Ratio: {ratio:F2}");
            Console.WriteLine($"Mean Error: {meanError:E}");
            Console.WriteLine($"Mean Absolute Error: {meanAbsError:E}");
            Console.WriteLine($"Maximum Error: {maxError:E}");
            Console.WriteLine("Decompressed values:");
            for (int i = 0; i < scientificValues.Length; i++)
            {
                Console.WriteLine($"Original: {scientificValues[i],20:G9} | Decompressed: {decompressed[i],20:G9} | Error: {errors[i],12:E}");
            }
        }

        Console.WriteLine("\nThis demonstrates how the compressor can be tuned for scientific applications, balancing storage efficiency and precision for a wide range of values.");
    }
}
