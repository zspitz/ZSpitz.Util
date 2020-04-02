﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZSpitz.Util {
    public static class IEnumerableKVPExtensions {
        public static void AddRangeTo<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> toAdd, Dictionary<TKey, TValue> dict) => toAdd.ForEach(kvp => dict.Add(kvp.Key, kvp.Value));
        public static IEnumerable<TValue> Values<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.Select(x => x.Value);
        public static IEnumerable<TResult> SelectKVP<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, TResult> selector) => dict.Select(kvp => selector(kvp.Key, kvp.Value));
        public static IEnumerable<TKey> Keys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src) => src.Select(x => x.Key);
    }
}
