using System;
using System.Collections.Generic;
using System.Linq;

namespace ZSpitz.Util {
    public static class IEnumerableKVPExtensions {
        public static void AddRangeTo<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> toAdd, Dictionary<TKey, TValue> dict) => toAdd.ForEach(kvp => dict.Add(kvp.Key, kvp.Value));
        public static IEnumerable<TValue> Values<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.Select(x => x.Value);
        public static IEnumerable<TResult> SelectKVP<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, TResult> selector) => dict.Select(kvp => selector(kvp.Key, kvp.Value));
        public static IEnumerable<TKey> Keys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.Select(x => x.Key);
        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
        public static IEnumerable<KeyValuePair<TKey, TValue>> WhereKVP<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src, Func<TKey, TValue, bool> predicate) => src.Where(kvp => predicate(kvp.Key, kvp.Value));
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.ToDictionary(x => x.Key, x => x.Value);
    }
}
