using System.Collections.Generic;
using Xunit;

namespace Tests {
    internal static class Extensions {
        internal static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> src) {
            var ret = new TheoryData<T>();
            foreach (var item in src) {
                ret.Add(item);
            }
            return ret;
        }
        internal static TheoryData<T1, T2, T3> ToTheoryData<T1, T2, T3>(this IEnumerable<(T1, T2, T3)> src) {
            var ret = new TheoryData<T1, T2, T3>();
            foreach (var (a, b, c) in src) {
                ret.Add(a, b, c);
            }
            return ret;
        }

    }
}
