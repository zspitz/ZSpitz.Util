using System;
using System.Collections.Generic;
using System.Linq;

namespace ZSpitz.Util.Extensions {
    // https://stackoverflow.com/a/5543006/111794
    internal static class EnumExtensions {
        public static IEnumerable<Enum> GetFlags(this Enum value) =>
            GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value) => 
            GetFlags(value, GetFlagValues(value.GetType()).ToArray());

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values) {
            long bits = Convert.ToInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--) {
                long mask = Convert.ToInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask) {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToInt64(value) && values.Length > 0 && Convert.ToInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType) {
            long flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>()) {
                long bits = Convert.ToInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }
}
