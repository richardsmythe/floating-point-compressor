using System.Runtime.CompilerServices;
using System.Numerics;

namespace FloatingPointCompressor
{
    public class FloatCompressor
    {
        private readonly float[] _values;
        private readonly int _scale;
        private readonly int _bitsPerValue;

        public FloatCompressor(float[] values, Precision precision)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            _values = values;
            _scale = (int)precision;
            _bitsPerValue = CalculateBitsPerValue();
        }

        public byte[] Compress()
        {
            if (_values.Length == 0) return Array.Empty<byte>();
            
            int totalBits = _values.Length * _bitsPerValue;
            int totalBytes = (totalBits + 7) / 8;
            var compressedData = new byte[totalBytes];
            int bitPosition = 0;

            for (int i = 0; i + Vector<float>.Count <= _values.Length; i += Vector<float>.Count)
            {
                var vector = new Vector<float>(_values, i);
                var scaledVector = vector * new Vector<float>(_scale);
                var scaledArray = new float[Vector<float>.Count];
                scaledVector.CopyTo(scaledArray);
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    int scaledValue = (int)Math.Round(scaledArray[j]);
                    PackBits(scaledValue, ref bitPosition, compressedData);
                }
            }
            // makes sure all values fit into vector
            for (int i = _values.Length - (_values.Length % Vector<float>.Count); i < _values.Length; i++)
            {
                int scaledValue = (int)Math.Round(_values[i] * _scale);
                PackBits(scaledValue, ref bitPosition, compressedData);
            }

            return compressedData;
        }

        public float[] Decompress(byte[] compressedData)
        {
            if (compressedData == null) throw new ArgumentNullException(nameof(compressedData));
            if (compressedData.Length == 0) return Array.Empty<float>();
            var decompressedValues = new float[_values.Length];
            int bitPosition = 0;
            for (int i = 0; i + Vector<float>.Count <= _values.Length; i += Vector<float>.Count)
            {
                var tmpArr = new float[Vector<float>.Count];
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    int scaledValue = UnpackBits(ref bitPosition, compressedData);
                    tmpArr[j] = scaledValue / (float)_scale;
                }
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    decompressedValues[i + j] = tmpArr[j];
                }
            }
            // remaing values that didnt fit in vector
            for (int i = _values.Length - (_values.Length % Vector<float>.Count); i < _values.Length; i++)
            {
                int scaledValue = UnpackBits(ref bitPosition, compressedData);
                decompressedValues[i] = scaledValue / (float)_scale;
            }

            return decompressedValues;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateBitsPerValue()
        {
            float maxAbsValue = _values.Length == 0 ? 0 : _values.Max(v => Math.Abs(v));
            int maxScaledValue = (int)Math.Ceiling(maxAbsValue * _scale);
            return (int)Math.Ceiling(Math.Log2(maxScaledValue + 1)) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PackBits(int value, ref int bitPosition, byte[] buffer)
        {
            bool isNegative = value < 0;
            int absoluteValue = isNegative ? ~value : value;
            for (int i = 0; i < _bitsPerValue - 1; i++)
            {
                WriteBit(buffer, bitPosition++, (absoluteValue & (1 << i)) != 0);
            }
            WriteBit(buffer, bitPosition++, isNegative);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int UnpackBits(ref int bitPosition, byte[] buffer)
        {
            int value = 0;

            for (int i = 0; i < _bitsPerValue - 1; i++)
            {
                if (ReadBit(buffer, bitPosition++))
                {
                    value |= 1 << i;
                }
            }
            if (ReadBit(buffer, bitPosition++))
            {
                value = ~value;
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBit(byte[] buffer, int bitIndex, bool bitValue)
        {
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;

            if (bitValue)
            {
                buffer[byteIndex] |= (byte)(1 << bitOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReadBit(byte[] buffer, int bitIndex)
        {
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;
            return (buffer[byteIndex] & (1 << bitOffset)) != 0;
        }
    }
}
