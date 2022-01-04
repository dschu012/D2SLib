// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Adapted from here, to allow for span-based constructor. Removed unused/unsafe code.
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Collections/src/System/Collections/BitArray.cs

using Microsoft.Toolkit.HighPerformance;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace D2SLib.IO;

// A vector of bits.  Use this to store bits efficiently
[Serializable]
internal sealed class InternalBitArray : IList<bool>, ICloneable, IDisposable
{
    private int[] m_array; // Do not rename (binary serialization)
    private int m_length; // Do not rename (binary serialization)
    private int _version; // Do not rename (binary serialization)

    private const int _ShrinkThreshold = 256;

    /*=========================================================================
    ** Allocates space to hold length bit values. All of the values in the bit
    ** array are set to false.
    **
    ** Exceptions: ArgumentException if length < 0.
    =========================================================================*/
    public InternalBitArray(int length)
        : this(length, false)
    {
    }

    /*=========================================================================
    ** Allocates space to hold length bit values. All of the values in the bit
    ** array are set to defaultValue.
    **
    ** Exceptions: ArgumentOutOfRangeException if length < 0.
    =========================================================================*/
    public InternalBitArray(int length, bool defaultValue)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be non-negative");
        }

        int arrayLength = GetInt32ArrayLengthFromBitLength(length);
        m_array = ArrayPool<int>.Shared.Rent(arrayLength);
        m_length = length;

        if (defaultValue)
        {
            var span = m_array.AsSpan(0, arrayLength);
            span.Fill(-1);

            // clear high bit values in the last int
            Div32Rem(length, out int extraBits);
            if (extraBits > 0)
            {
                span[^1] = (1 << extraBits) - 1;
            }
        }

        _version = 0;
    }

    /*=========================================================================
    ** Allocates space to hold the bit values in bytes. bytes[0] represents
    ** bits 0 - 7, bytes[1] represents bits 8 - 15, etc. The LSB of each byte
    ** represents the lowest index value; bytes[0] & 1 represents bit 0,
    ** bytes[0] & 2 represents bit 1, bytes[0] & 4 represents bit 2, etc.
    **
    ** Exceptions: ArgumentException if bytes == null.
    =========================================================================*/
    public InternalBitArray(ReadOnlySpan<byte> bytes)
    {
        // this value is chosen to prevent overflow when computing m_length.
        // m_length is of type int32 and is exposed as a property, so
        // type of m_length can't be changed to accommodate.
        if (bytes.Length > int.MaxValue / BitsPerByte)
        {
            throw new ArgumentException("Too many bytes!", nameof(bytes));
        }

        m_array = ArrayPool<int>.Shared.Rent(GetInt32ArrayLengthFromByteLength(bytes.Length));
        m_length = bytes.Length * BitsPerByte;

        uint totalCount = (uint)bytes.Length / 4;

        for (int i = 0; i < totalCount; i++)
        {
            m_array[i] = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            bytes = bytes[4..];
        }

        Debug.Assert(bytes.Length is >= 0 and < 4);

        int last = 0;
        switch (bytes.Length)
        {
            case 3:
                last = bytes[2] << 16;
                goto case 2;
            // fall through
            case 2:
                last |= bytes[1] << 8;
                goto case 1;
            // fall through
            case 1:
                m_array[totalCount] = last | bytes[0];
                break;
        }

        _version = 0;
    }

    /*=========================================================================
    ** Allocates a new BitArray with the same length and bit values as bits.
    **
    ** Exceptions: ArgumentException if bits == null.
    =========================================================================*/
    public InternalBitArray(InternalBitArray bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        int arrayLength = GetInt32ArrayLengthFromBitLength(bits.m_length);

        m_array = ArrayPool<int>.Shared.Rent(arrayLength);

        Debug.Assert(bits.m_array.Length <= arrayLength);

        Array.Copy(bits.m_array, m_array, arrayLength);
        m_length = bits.m_length;

        _version = bits._version;
    }

    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Get(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Set(index, value);
    }

    /*=========================================================================
    ** Returns the bit value at position index.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Get(int index)
    {
        if ((uint)index >= (uint)m_length)
        {
            ThrowArgumentOutOfRangeException(index);
        }

        return (m_array[index >> 5] & (1 << index)) != 0;
    }

    /*=========================================================================
    ** Sets the bit value at position index to value.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int index, bool value)
    {
        if ((uint)index >= (uint)m_length)
        {
            ThrowArgumentOutOfRangeException(index);
        }

        int bitMask = 1 << index;
        ref int segment = ref m_array[index >> 5];

        if (value)
        {
            segment |= bitMask;
        }
        else
        {
            segment &= ~bitMask;
        }

        _version++;
    }

    public void Add(bool value)
    {
        int idx = Length++;
        Set(idx, value);
    }

    /*=========================================================================
    ** Sets all the bit values to value.
    =========================================================================*/
    public void SetAll(bool value)
    {
        int arrayLength = GetInt32ArrayLengthFromBitLength(Length);
        var span = m_array.AsSpan(0, arrayLength);
        if (value)
        {
            span.Fill(-1);

            // clear high bit values in the last int
            Div32Rem(m_length, out int extraBits);
            if (extraBits > 0)
            {
                span[^1] &= (1 << extraBits) - 1;
            }
        }
        else
        {
            span.Clear();
        }

        _version++;
    }

    public int Length
    {
        get => m_length;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative.");
            }

            int newints = GetInt32ArrayLengthFromBitLength(value);
            if (newints > m_array.Length || newints + _ShrinkThreshold < m_array.Length)
            {
                // grow or shrink (if wasting more than _ShrinkThreshold ints)
                ArrayPool<int>.Shared.Resize(ref m_array!, newints);
            }

            if (value > m_length)
            {
                // clear high bit values in the last int
                int last = (m_length - 1) >> BitShiftPerInt32;
                Div32Rem(m_length, out int bits);
                if (bits > 0)
                {
                    m_array[last] &= (1 << bits) - 1;
                }

                // clear remaining int values
                m_array.AsSpan(last + 1, newints - last - 1).Clear();
            }

            m_length = value;
            _version++;
        }
    }

    int ICollection<bool>.Count => m_length;

    bool ICollection<bool>.IsReadOnly => false;

    public object Clone() => new InternalBitArray(this);

    public IEnumerator<bool> GetEnumerator() => new BitArrayEnumeratorSimple(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // XPerY=n means that n Xs can be stored in 1 Y.
    private const int BitsPerInt32 = 32;
    private const int BitsPerByte = 8;

    private const int BitShiftPerInt32 = 5;
    private const int BitShiftPerByte = 3;
    private const int BitShiftForBytesPerInt32 = 2;

    /// <summary>
    /// Used for conversion between different representations of bit array.
    /// Returns (n + (32 - 1)) / 32, rearranged to avoid arithmetic overflow.
    /// For example, in the bit to int case, the straightforward calc would
    /// be (n + 31) / 32, but that would cause overflow. So instead it's
    /// rearranged to ((n - 1) / 32) + 1.
    /// Due to sign extension, we don't need to special case for n == 0, if we use
    /// bitwise operations (since ((n - 1) >> 5) + 1 = 0).
    /// This doesn't hold true for ((n - 1) / 32) + 1, which equals 1.
    ///
    /// Usage:
    /// GetArrayLength(77): returns how many ints must be
    /// allocated to store 77 bits.
    /// </summary>
    /// <param name="n"></param>
    /// <returns>how many ints are required to store n bytes</returns>
    private static int GetInt32ArrayLengthFromBitLength(int n)
    {
        Debug.Assert(n >= 0);
        return (int)((uint)(n - 1 + (1 << BitShiftPerInt32)) >> BitShiftPerInt32);
    }

    private static int GetInt32ArrayLengthFromByteLength(int n)
    {
        Debug.Assert(n >= 0);
        // Due to sign extension, we don't need to special case for n == 0, since ((n - 1) >> 2) + 1 = 0
        // This doesn't hold true for ((n - 1) / 4) + 1, which equals 1.
        return (int)((uint)(n - 1 + (1 << BitShiftForBytesPerInt32)) >> BitShiftForBytesPerInt32);
    }

    internal static int GetByteArrayLengthFromBitLength(int n)
    {
        Debug.Assert(n >= 0);
        // Due to sign extension, we don't need to special case for n == 0, since ((n - 1) >> 3) + 1 = 0
        // This doesn't hold true for ((n - 1) / 8) + 1, which equals 1.
        return (int)((uint)(n - 1 + (1 << BitShiftPerByte)) >> BitShiftPerByte);
    }

    private static int Div32Rem(int number, out int remainder)
    {
        uint quotient = (uint)number / 32;
        remainder = number & (32 - 1);    // equivalent to number % 32, since 32 is a power of 2
        return (int)quotient;
    }

    private static void ThrowArgumentOutOfRangeException(int index) => throw new ArgumentOutOfRangeException(nameof(index), index, "Index was out of range.");
    
    int IList<bool>.IndexOf(bool item) => throw new NotImplementedException();
    void IList<bool>.Insert(int index, bool item) => throw new NotImplementedException();
    void IList<bool>.RemoveAt(int index) => throw new NotImplementedException();
    void ICollection<bool>.Add(bool item) => throw new NotImplementedException();
    public void Clear() => SetAll(false);
    bool ICollection<bool>.Contains(bool item) => throw new NotImplementedException();
    void ICollection<bool>.CopyTo(bool[] array, int arrayIndex) => throw new NotImplementedException();
    bool ICollection<bool>.Remove(bool item) => throw new NotImplementedException();

    public void Dispose()
    {
        if (m_array.Length > 0)
        {
            ArrayPool<int>.Shared.Return(m_array);
            m_array = Array.Empty<int>();
            m_length = 0;
        }
    }

    private sealed class BitArrayEnumeratorSimple : IEnumerator<bool>, ICloneable
    {
        private readonly InternalBitArray _bitArray;
        private int _index;
        private readonly int _version;
        private bool _currentElement;

        internal BitArrayEnumeratorSimple(InternalBitArray bitArray)
        {
            _bitArray = bitArray;
            _index = -1;
            _version = bitArray._version;
        }

        public object Clone() => MemberwiseClone();

        public bool MoveNext()
        {
            if (_version != _bitArray._version)
            {
                throw new InvalidOperationException("Enumeration failed: collection was modified.");
            }

            if (_index < (_bitArray.m_length - 1))
            {
                _index++;
                _currentElement = _bitArray.Get(_index);
                return true;
            }
            else
            {
                _index = _bitArray.m_length;
            }

            return false;
        }

        public bool Current
        {
            get
            {
                if ((uint)_index >= (uint)_bitArray.m_length)
                {
                    throw GetInvalidOperationException(_index);
                }

                return _currentElement;
            }
        }

        object IEnumerator.Current => Current;

        public void Reset()
        {
            if (_version != _bitArray._version)
            {
                throw new InvalidOperationException("Enumeration failed: collection was modified.");
            }

            _index = -1;
        }

        private InvalidOperationException GetInvalidOperationException(int index)
        {
            if (index == -1)
            {
                return new InvalidOperationException("Enumeration not started.");
            }
            else
            {
                Debug.Assert(index >= _bitArray.m_length);
                return new InvalidOperationException("Enumeration ended.");
            }
        }

        void IDisposable.Dispose() { }
    }
}
