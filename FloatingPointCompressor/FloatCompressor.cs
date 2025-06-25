using System.Runtime.CompilerServices;
using System.Numerics;

namespace FloatingPointCompressor
{
    public class FloatCompressor<T> where T: IFloatingPointIeee754<T>
    {
        private readonly T[] _values;
        private readonly int _scale;
        private readonly int _bitsPerValue;

        public FloatCompressor(T[] values, Precision precision)
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
                    int scaledValue = int.CreateChecked(T.Round(scaledArray[j]));
                    PackBits(scaledValue, ref bitPosition, compressedData);
                }
            }
            // makes sure all values fit into vector
            for (int i = _values.Length - (_values.Length % Vector<T>.Count); i < _values.Length; i++)
            {
                var scaled = _values[i] * scaleT;
                int scaledValue = int.CreateChecked(T.Round(scaled));
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
                    int scaledValue = UnpackBits(ref bitPosition, compressedData);
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
                int scaledValue = UnpackBits(ref bitPosition, compressedData);
                decompressedValues[i] = T.CreateChecked(scaledValue) / scaleT;
            }

            return decompressedValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateBitsPerValue()
        {
            T maxAbsValue = _values.Length == 0 ? T.Zero : _values.Max(v => T.Abs(v));
            int maxScaledValue = int.CreateChecked(T.Ceiling(maxAbsValue * T.CreateChecked(_scale)));
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
