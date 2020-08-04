using System.Collections.Generic;
using Xunit;

namespace Tests {
    public static class Extensions {
        internal static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> src) {
            var ret = new TheoryData<T>();
            foreach (var item in src) {
                ret.Add(item);
            }
            return ret;
        }
    }
}
