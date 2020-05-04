﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZSpitz.Util {
    public static class AssemblyExtensions {
        public static IEnumerable<T> GetAttributes<T>(this Assembly asm, bool inherit) where T : Attribute =>
            asm.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }
}
