﻿#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1
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
