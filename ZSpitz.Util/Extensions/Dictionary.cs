using System;
using System.Collections.Generic;
using System.Text;

namespace ZSpitz.Util {
    public static class DictionaryExtensions {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<(TKey, TValue)> src) => src.ForEach(t => dict.Add(t.Item1, t.Item2));
    }
}
