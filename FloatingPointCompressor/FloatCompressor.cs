using System.Runtime.CompilerServices;
using System.Numerics;

namespace FloatingPointCompressor
{
    public class FloatCompressor<T> where T: IFloatingPointIeee754<T>
    {
        private readonly T[] _values;
        private readonly long _scale; // changed from double to long
        private readonly int _bitsPerValue;

        public FloatCompressor(T[] values, Precision precision)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            _values = values;
            double scaleDouble = 1.0 / precision.Value;
            if (scaleDouble > long.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(precision), $"Precision value too small, scale overflows long: {scaleDouble}");
            _scale = (long)Math.Round(scaleDouble);
            _bitsPerValue = CalculateBitsPerValue();
        }

        public byte[] Compress()
        {
            if (_values.Length == 0) return Array.Empty<byte>();
            
            int totalBits = _values.Length * _bitsPerValue;
            int totalBytes = (totalBits + 7) / 8;
            var compressedData = new byte[totalBytes];
            int bitPosition = 0;

            var scaleT = T.CreateChecked(_scale);
            var scaleVector = Vector<T>.One * scaleT;
            for (int i = 0; i + Vector<T>.Count <= _values.Length; i += Vector<T>.Count)
            {
                var vector = new Vector<T>(_values, i);
                var scaledVector = vector * scaleVector;
                var scaledArray = new T[Vector<T>.Count];
                scaledVector.CopyTo(scaledArray);
                for (int j = 0; j < Vector<T>.Count; j++)
                {
                    double scaled = double.CreateChecked(T.Round(scaledArray[j]));
                    double clamped = Math.Max(Math.Min(scaled, long.MaxValue), long.MinValue);
                    long scaledValue = (long)clamped;
                    PackBits(scaledValue, ref bitPosition, compressedData);
                }
            }
            // makes sure all values fit into vector
            for (int i = _values.Length - (_values.Length % Vector<T>.Count); i < _values.Length; i++)
            {
                double scaled = double.CreateChecked(T.Round(_values[i] * scaleT));
                double clamped = Math.Max(Math.Min(scaled, long.MaxValue), long.MinValue);
                long scaledValue = (long)clamped;
                PackBits(scaledValue, ref bitPosition, compressedData);
            }

            return compressedData;
        }

        public T[] Decompress(byte[] compressedData)
        {
            if (compressedData == null) throw new ArgumentNullException(nameof(compressedData));
            if (compressedData.Length == 0) return Array.Empty<T>();
            var decompressedValues = new T[_values.Length];
            int bitPosition = 0;
            var scaleT = T.CreateChecked(_scale);
            for (int i = 0; i + Vector<T>.Count <= _values.Length; i += Vector<T>.Count)
            {
                var tmpArr = new T[Vector<T>.Count];
                for (int j = 0; j < Vector<T>.Count; j++)
                {
                    long scaledValue = UnpackBits(ref bitPosition, compressedData);
                    tmpArr[j] = T.CreateChecked(scaledValue) / scaleT;
                }
                for (int j = 0; j < Vector<T>.Count; j++)
                {
                    decompressedValues[i + j] = tmpArr[j];
                }
            }
            // remaing values that didnt fit in vector
            for (int i = _values.Length - (_values.Length % Vector<T>.Count); i < _values.Length; i++)
            {
                long scaledValue = UnpackBits(ref bitPosition, compressedData);
                decompressedValues[i] = T.CreateChecked(scaledValue) / scaleT;
            }

            return decompressedValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateBitsPerValue()
        {
            T maxAbsValue = _values.Length == 0 ? T.Zero : _values.Max(v => T.Abs(v));
            // Compute scaled value as double to check for overflow
            double scaled = double.CreateChecked(maxAbsValue) * _scale;
            double clamped = Math.Min(Math.Abs(scaled), long.MaxValue);
            long maxScaledValue = (long)Math.Ceiling(clamped);
            return (int)Math.Ceiling(Math.Log2(maxScaledValue + 1)) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PackBits(long value, ref int bitPosition, byte[] buffer)
        {
            bool isNegative = value < 0;
            long absoluteValue = isNegative ? ~value : value;
            for (int i = 0; i < _bitsPerValue - 1; i++)
            {
                WriteBit(buffer, bitPosition++, (absoluteValue & (1L << i)) != 0);
            }
            WriteBit(buffer, bitPosition++, isNegative);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long UnpackBits(ref int bitPosition, byte[] buffer)
        {
            long value = 0;

            for (int i = 0; i < _bitsPerValue - 1; i++)
            {
                if (ReadBit(buffer, bitPosition++))
                {
                    value |= 1L << i;
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
