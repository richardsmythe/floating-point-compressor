using Xunit;
namespace FloatingPointCompressor.Tests
{
    public class FloatCompressorTests
    {
        public static readonly TheoryData<float[], Precision> samples = new TheoryData<float[], Precision>
        {
            { new float[] { 3.14159265f }, Precision.Millionths },
            { new float[] {0f, 0.0f, 0.00f, 0.000f, 0.0000f,0.000000f}, Precision.Hundredths },
            { new float[] { -0.0001f, -6.100f, -9.9999f}, Precision.Thousandsth },
            { new float[] { 0.2345f, 0.1000f, 1.24145f}, Precision.Tenths },
            { new float[] { 4.45473145f, 2.45444125f, 8.45478115f, 5.35473615f, 7.25247615f, 1.41547365f }, Precision.Tenths },
            { new float[] { 2.4345f, 3.5331f, 8.2222f, 4.1234f }, Precision.Hundredths },
            { new float[] { 5.1431f, 2.2222f, 9.4444f, 5.1111f, 7.3244f, 5.5555f }, Precision.Thousandsth },
            { new float[] { 6.454731f, 4.454441f, 6.454781f, 5.354761f, 7.254761f, 2.454736f }, Precision.TenThousandths },
            { new float[] { 1.123f, 2.222f, 9.444f, 5.111f, 6.134f, 6.555f, 5.111f, 9.211f }, Precision.Millionths },
            { new float[] { 0.123f, 1.234f, 2.345f, 3.456f, 4.567f, 5.678f, 6.789f, 7.890f, 8.901f, 9.012f,
                            10.123f, 11.2364f, 12.345f, 13.456f, 14.567f, 115.678f, 16.789f, 17.890f, -18.901f, 19.012f,
                            20.123f, 21.2634f, 22.3445f, 23.456f, 24.567f, 625.678f, 26.789f, 27.890f, 28.901f, 29.012f,
                            230.123f, 31.2364f, 32.345f, 33.4456f, 34.567f, -35.678f, 36.789f, 37.890f, 38.901f, 39.012f,
                            40.123f, 41.234f, 42.345f, 43.456f, -44.567f, 45.678f, 46.789f, 47.890f, 48.901f, 49.012f,
                            50.123f, 51.234f, 52.345f, 53.456f, -54.567f, 55.678f, 56.789f, 57.890f, 58.901f, 59.012f,
                            60.123f, 61.234f, 62.345f, 63.456f, 64.567f, 65.678f, 66.789f, 67.890f, 68.901f, 69.012f,
                            -70.123f, 71.234f, 72.345f, 73.456f, 74.567f, 75.678f, 76.789f, 77.890f, 784.9501f, 79.012f,
                            80.123f, 81.234f, 82.345f, 83.456f, 84.567f, 85.678f, 86.789f, 87.890f, 88.901f, 89.012f,
                            90.123f, 91.234f, 92.345f, 93.456f, 94.567f, 95.678f, 96.789f, 97.890f, 98.901f,-99.012f
                        }, Precision.Thousandsth }
        };

        [Theory]
        [MemberData(nameof(samples))]
        public void Test_Precision_With_Different_Values(float[] values, Precision precision)
        {
            FloatCompressor fc = new FloatCompressor(values, precision);
            var compressedData = fc.Compress();
            float[] decompressed = fc.Decompress(compressedData);
            for (int i = 0; i < values.Length; i++)
            {
                float expected = values[i];
                float actual = decompressed[i];
                float tolerance = (float)precision.GetTolerance();
                Assert.InRange(actual, expected - tolerance, expected + tolerance);
            }
        }

        [Fact]
        public void Test_Large_Input_Size()
        {
            float[] values = new float[1000000];
            Random rand = new Random();
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (float)(rand.NextDouble() * 100);
            }
            FloatCompressor fc = new FloatCompressor(values, Precision.Thousandsth);
            var compressedData = fc.Compress();
            var decompressed = fc.Decompress(compressedData);

            Assert.True(compressedData.Length < values.Length * sizeof(float));
            Assert.Equal(values.Length, decompressed.Length);
        }

        [Fact]
        public void Test_Compression_Reduction()
        {
            float[] values = { 1.23f, 4.56f, 7.89f, 10.12f, 15.34f };
            FloatCompressor fc = new FloatCompressor(values, Precision.Thousandsth);
            var compressedData = fc.Compress();
            var originalSize = values.Length * sizeof(float);
            float compressionRatio = (float)originalSize / compressedData.Length;
            Assert.True(compressionRatio > 1, $"Compression ratio is too low: {compressionRatio}");
        }

        [Fact]
        public void Test_Compression_Ratio_Varying_Precisions()
        {
            float[] values = { 1.23f, 4.56f, 7.89f, 10.12f, 15.34f };  
            foreach (var precision in Enum.GetValues(typeof(Precision)))
            {
                FloatCompressor fc = new FloatCompressor(values, (Precision)precision);
                var compressedData = fc.Compress();
                var originalSize = values.Length * sizeof(float);
                var compressedSize = compressedData.Length;
                float compressionRatio = (float)originalSize / compressedSize;
                Assert.True(compressionRatio > 1, $"Compression ratio for {precision} precision is too low: {compressionRatio}");
            }
        }    

        [Fact]
        public void Test_Error_Distributions_For_Different_Precisions()
        {
            float[] values = { 1.123456f, 2.654321f, 3.987654f, 4.000001f };
            FloatCompressor fcHigh = new FloatCompressor(values, Precision.Millionths);
            var compressedHigh = fcHigh.Compress();
            float[] decompressedHigh = fcHigh.Decompress(compressedHigh);
            FloatCompressor fcLow = new FloatCompressor(values, Precision.Tenths);
            var compressedLow = fcLow.Compress();
            float[] decompressedLow = fcLow.Decompress(compressedLow);
            for (int i = 0; i < values.Length; i++)
            {
                float original = values[i];
                float highPrecisionError = Math.Abs(original - decompressedHigh[i]);
                float lowPrecisionError = Math.Abs(original - decompressedLow[i]);

                Assert.True(highPrecisionError < lowPrecisionError,
                    $"Error with higher precision should be less than error with lower precision for index {i}. " +
                    $"High precision error: {highPrecisionError}, Low precision error: {lowPrecisionError}");
            }
            Assert.True(compressedLow.Length < compressedHigh.Length,
                "Compressed data with lower precision should be smaller in size compared to higher precision.");
        }

        [Fact]
        public void Test_Varying_Precision_Compresses()
        {
            var values = new float[] { 123.456f, 1.9f };
            FloatCompressor fc10s = new FloatCompressor(values, Precision.Tenths);
            FloatCompressor fc100s = new FloatCompressor(values, Precision.Hundredths);
            var compressedData10s = fc10s.Compress();
            var compressedData100s = fc100s.Compress();
            Assert.NotEmpty(compressedData10s);
            Assert.NotEmpty(compressedData100s);
            Assert.NotEqual(compressedData10s.Length, compressedData100s.Length);
        }

        [Fact]
        public void Test_Edge_Case_Precision()
        {
            float[] values = { 1.00000049f, 1.00000051f };
            FloatCompressor fc = new FloatCompressor(values, Precision.Millionths);
            var compressedData = fc.Compress();
            var decompressed = fc.Decompress(compressedData);
            Assert.Equal(1.0000005f, decompressed[0], precision: 6);
            Assert.Equal(1.0000005f, decompressed[1], precision: 6);
        }


    }

    public static class PrecisionExtensions
    {
        public static double GetTolerance(this Precision precision)
        {
            return precision switch
            {
                Precision.Millionths => 0.000001,
                Precision.HundredThousandths => 0.00001,
                Precision.TenThousandths => 0.0001,
                Precision.Thousandsth => 0.001,
                Precision.Hundredths => 0.01,
                Precision.Tenths => 0.1,
                _ => 0.0001,
            };
        }
    }
}
