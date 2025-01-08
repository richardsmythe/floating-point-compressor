using BenchmarkDotNet.Attributes;
using System.Numerics;

namespace FloatingPointCompressor
{
    public class FloatCompressorBenchmark
    {
        private Precision _precision = Precision.Thousandsth;
        private float[] _values;


        [GlobalSetup]
        public void Setup()
        {
            int numValues = 1000000;
            _values = new float[numValues];
            var r = new Random();

            for (int i = 0; i < _values.Length; i++)
            {  
                _values[i] = (float)(r.NextDouble() * 10);
                _values[i] = (float)Math.Round(_values[i], 6);
            }
        }

        [Benchmark]
        public byte[] CompressBenchmark()
        {
            FloatCompressor fc = new FloatCompressor(_values, _precision);
            return fc.Compress();
        }

        [Benchmark]
        public float[] DecompressBenchmark()
        {
            FloatCompressor fc = new FloatCompressor(_values, _precision);
            var compressedData = fc.Compress();
            return fc.Decompress(compressedData);
        }
    }
}
