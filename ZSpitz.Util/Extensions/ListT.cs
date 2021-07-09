using System.Collections.Generic;

namespace ZSpitz.Util {
    public static class ListTExtensions {
        public static void RemoveLast<T>(this List<T> lst, int count = 1) => lst.RemoveRange(lst.Count - count, count);
    }
}
