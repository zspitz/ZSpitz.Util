using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZSpitz.Util {
    public static class MemberInfoExtensions {
        public static bool HasAttribute<TAttribute>(this MemberInfo mi, bool inherit = false) where TAttribute : Attribute =>
            mi.GetCustomAttributes(typeof(TAttribute), inherit).Any();

        //public static BindingFlags BindingFlags(this MemberInfo mi) {
        //    var (isPublic, isStatic) = mi switch {
        //        ConstructorInfo ci => (ci.IsPublic, ci.IsStatic),
        //        //EventInfo ei => (ei,
        //        FieldInfo fi => (fi.IsPublic, fi.IsStatic),
        //        MethodInfo mthdi => (mthdi.IsPublic, mthdi.IsStatic),
        //        PropertyInfo pi => (pi.,
        //        _ => throw new NotImplementedException()
        //    };
        //}
    }
}
