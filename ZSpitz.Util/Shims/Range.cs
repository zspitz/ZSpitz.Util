#if !NET && !NETCOREAPP3_0_OR_GREATER

// https://github.com/dotnet/runtime/blob/419e949d258ecee4c40a460fb09c66d974229623/src/libraries/System.Private.CoreLib/src/System/Index.cs
// https://github.com/dotnet/runtime/blob/419e949d258ecee4c40a460fb09c66d974229623/src/libraries/System.Private.CoreLib/src/System/Range.cs

using System.Runtime.CompilerServices;

namespace System {
    /// <summary>Represent a type can be used to index a collection either from the start or the end.</summary>
    /// <remarks>
    /// Index is used by the C# compiler to support the new index syntax
    /// <code>
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 } ;
    /// int lastElement = someArray[^1]; // lastElement = 5
    /// </code>
    /// </remarks>
    public readonly struct Index : IEquatable<Index> {
        private readonly int val;

        /// <summary>Construct an Index using a value and indicating if the index is from the start or from the end.</summary>
        /// <param name="value">The index value. it has to be zero or positive number.</param>
        /// <param name="fromEnd">Indicating if the index is from the start or from the end.</param>
        /// <remarks>
        /// If the Index constructed from the end, index value 1 means pointing at the last element and index value 0 means pointing at beyond last element.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int value, bool fromEnd = false) {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            }

            val = fromEnd ? ~value : value;
        }

        // The following private constructors mainly created for perf reason to avoid the checks
        private Index(int value) => val = value;

        /// <summary>Create an Index pointing at first element.</summary>
        public static Index Start => new(0);

        /// <summary>Create an Index pointing at beyond last element.</summary>
        public static Index End => new(~0);

        /// <summary>Create an Index from the start at the position indicated by the value.</summary>
        /// <param name="value">The index value from the start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value) => 
            value < 0 ? 
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative") : 
                new(value);

        /// <summary>Create an Index from the end at the position indicated by the value.</summary>
        /// <param name="value">The index value from the end.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value) => 
            value < 0 ? 
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative") : 
                new(~value);

        /// <summary>Returns the index value.</summary>
        public int Value => 
            val < 0 ? 
                ~val : 
                val;

        /// <summary>Indicates whether the index is from the start or the end.</summary>
        public bool IsFromEnd => val < 0;

        /// <summary>Calculate the offset from the start using the giving collection length.</summary>
        /// <param name="length">The length of the collection that the Index will be used with. length has to be a positive value</param>
        /// <remarks>
        /// For performance reason, we don't validate the input length parameter and the returned offset value against negative values.
        /// we don't validate either the returned offset is greater than the input length.
        /// It is expected Index will be used with collections which always have non negative length/count. If the returned offset is negative and
        /// then used to index a collection will get out of range exception which will be same affect as the validation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int length) {
            var offset = val;
            if (IsFromEnd) {
                // offset = length - (~value)
                // offset = length + (~(~value) + 1)
                // offset = length + value + 1

                offset += length + 1;
            }
            return offset;
        }

        /// <summary>Indicates whether the current Index object is equal to another object of the same type.</summary>
        /// <param name="value">An object to compare with this object</param>
        public override bool Equals(object? value) => value is Index idx && val == idx.val;

        /// <summary>Indicates whether the current Index object is equal to another Index object.</summary>
        /// <param name="other">An object to compare with this object</param>
        public bool Equals(Index other) => val == other.val;

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode() => val;

        /// <summary>Converts integer number to an Index.</summary>
        public static implicit operator Index(int value) => FromStart(value);

        /// <summary>Converts the value of the current Index object to its equivalent string representation.</summary>
        public override string ToString() => 
            IsFromEnd ? 
                "^" + ((uint)Value).ToString() : 
                ((uint)Value).ToString();
    }

    /// <summary>Represent a range has start and end indexes.</summary>
    /// <remarks>
    /// Range is used by the C# compiler to support the range syntax.
    /// <code>
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
    /// int[] subArray1 = someArray[0..2]; // { 1, 2 }
    /// int[] subArray2 = someArray[1..^0]; // { 2, 3, 4, 5 }
    /// </code>
    /// </remarks>
    public readonly struct Range : IEquatable<Range> {
        /// <summary>Represent the inclusive start index of the Range.</summary>
        public Index Start { get; }

        /// <summary>Represent the exclusive end index of the Range.</summary>
        public Index End { get; }

        /// <summary>Construct a Range object using the start and end indexes.</summary>
        /// <param name="start">Represent the inclusive start index of the range.</param>
        /// <param name="end">Represent the exclusive end index of the range.</param>
        public Range(Index start, Index end) {
            Start = start;
            End = end;
        }

        /// <summary>Indicates whether the current Range object is equal to another object of the same type.</summary>
        /// <param name="value">An object to compare with this object</param>
        public override bool Equals(object? value) =>
            value is Range r &&
            r.Start.Equals(Start) &&
            r.End.Equals(End);

        /// <summary>Indicates whether the current Range object is equal to another Range object.</summary>
        /// <param name="other">An object to compare with this object</param>
        public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();

        /// <summary>Converts the value of the current Range object to its equivalent string representation.</summary>
        public override string ToString() => Start + ".." + End;

        /// <summary>Create a Range object starting from start index to the end of the collection.</summary>
        public static Range StartAt(Index start) => new(start, Index.End);

        /// <summary>Create a Range object starting from first element in the collection to the end Index.</summary>
        public static Range EndAt(Index end) => new(Index.Start, end);

        /// <summary>Create a Range object starting from first element to the end.</summary>
        public static Range All => new(Index.Start, Index.End);

        /// <summary>Calculate the start offset and length of range object using a collection length.</summary>
        /// <param name="length">The length of the collection that the range will be used with. length has to be a positive value.</param>
        /// <remarks>
        /// For performance reason, we don't validate the input length parameter against negative values.
        /// It is expected Range will be used with collections which always have non negative length/count.
        /// We validate the range is inside the length scope though.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int Offset, int Length) GetOffsetAndLength(int length) {
            int start;
            var startIndex = Start;
            start = 
                startIndex.IsFromEnd ? 
                    length - startIndex.Value : 
                    startIndex.Value;

            int end;
            var endIndex = End;
            end = 
                endIndex.IsFromEnd ? 
                    length - endIndex.Value : 
                    endIndex.Value;

            return (uint)end > (uint)length || (uint)start > (uint)end ?
                    throw new ArgumentOutOfRangeException(nameof(length)) :
                    (start, end - start);
        }
    }
}

namespace System.Runtime.CompilerServices {
    internal static class RuntimeHelpers {
        /// <summary>
        /// Slices the specified array using the specified range.
        /// </summary>
        public static T[] GetSubArray<T>(T[] array, Range range) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            var (offset, length) = range.GetOffsetAndLength(array.Length);

            if (default(T) != null || typeof(T[]) == array.GetType()) {
                // We know the type of the array to be exactly T[].

                if (length == 0) {
#if NET452
                    return ZSpitz.Util.Functions.EmptyArray<T>();
#else
                    return Array.Empty<T>();
#endif
                }

                var dest = new T[length];
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            } else {
                // The array is actually a U[] where U:T.
                var dest = (T[])Array.CreateInstance(array.GetType().GetElementType(), length);
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
        }
    }
}

#endif
