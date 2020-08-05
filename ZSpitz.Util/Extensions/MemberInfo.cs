using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZSpitz.Util {
    public static class MemberInfoExtensions {
        public static bool HasAttribute<TAttribute>(this MemberInfo mi, bool inherit = false) where TAttribute : Attribute =>
            mi.GetCustomAttributes(typeof(TAttribute), inherit).Any();

        private static (bool isPublic, bool isStatic) bindingFlagsInfo(params MethodBase[] methods) {
            if (methods.None()) { throw new ArgumentException($"'{nameof(methods)} cannot be empty."); }
            bool isStatic = false;
            bool isPublic = false;
            methods.Where(x => x is { }).ForEach((x, index) => {
                if (index ==0) { isStatic = x.IsStatic; }
                if (x.IsPublic) {
                    isPublic = true;
                    return;
                }
            });
            return (isPublic, isStatic);
        }

        private static BindingFlags getBindingFlags(this MemberInfo mi) {
            var (isPublic, isStatic) = mi switch {
                ConstructorInfo ci => (ci.IsPublic, ci.IsStatic),
                EventInfo ei => bindingFlagsInfo(ei.AddMethod, ei.RemoveMethod, ei.RaiseMethod),
                FieldInfo fi => (fi.IsPublic, fi.IsStatic),
                MethodInfo mthdi => (mthdi.IsPublic, mthdi.IsStatic),
                PropertyInfo pi => bindingFlagsInfo(pi.GetMethod, pi.SetMethod),
                //Type t => (t.IsPublic, t.I)
                _ => throw new NotImplementedException()
            };
            return
                (isPublic ? BindingFlags.Public : BindingFlags.NonPublic) |
                (isStatic ? BindingFlags.Static : BindingFlags.Instance);
        }

        private static HashSet<BindingFlags> defaultLookups = new HashSet<BindingFlags> {
            BindingFlags.Public | BindingFlags.Instance,
            BindingFlags.Public | BindingFlags.Static
        };

        public static (MethodInfo method, object?[] args) GetInputs(this MemberInfo mi) {
            var reflectedType = mi.ReflectedType;
            var parameters = new List<(Type type, object? value)>();
            if (!(mi is ConstructorInfo)) {
                parameters.Add(typeof(string), mi.Name);
            }

            var flags = mi.getBindingFlags();

            if (mi is FieldInfo || mi is EventInfo) {
                // EventInfo and FieldInfo have two overloads: (string), and (string, bindingFlags)
                // If we need the BinidingFlags parameter, we'll use that overload
                if (flags.NotIn(defaultLookups)) { parameters.Add(typeof(BindingFlags), flags); }
            } else { // ConstrucctorInfo, MethodInfo, PropertyInfo
                var typesParameter = (mi switch {
                    ConstructorInfo ci => ci.GetParameters(),
                    MethodInfo mthdi => mthdi.GetParameters(),
                    PropertyInfo pi => pi.GetIndexParameters(),
                    _ => throw new NotImplementedException()
                }).Select(x => x.ParameterType).ToArray();

                var otherMemberCount = (mi switch
                {
                    ConstructorInfo _ => reflectedType.GetConstructors(flags),
                    MethodInfo mthdi => reflectedType.GetMethods(flags).Where(x => x.Name == mi.Name),
                    PropertyInfo _ => reflectedType.GetIndexers(true, flags),
                    _ => Enumerable.Empty<MemberInfo>()
                }).Count() - 1;

                if (mi is PropertyInfo) {
                    if (otherMemberCount > 0 && flags.NotIn(defaultLookups)) {
                        // string, BindingFlags, Binder, Type, Type[], ParameterModifier[]
                        parameters.Add(typeof(BindingFlags), flags);
                        parameters.Add(typeof(Binder), null);
                        parameters.Add(typeof(Type), null);
                        parameters.Add(typeof(Type[]), typesParameter);
                        parameters.Add(typeof(ParameterModifier[]), null);
                    } else {
                        // string
                        // string, Type[],
                        // string, BindingFlags
                        if (otherMemberCount > 0) { parameters.Add(typeof(Type[]), typesParameter); }
                        if (flags.NotIn(defaultLookups)) { parameters.Add(typeof(BindingFlags), flags); }
                    }
                } else if (mi is ConstructorInfo) {
                    if (flags.NotIn(defaultLookups)) {
                        // BindingFlags, Binder, Type[], ParameterModifier[]
                        parameters.Add(typeof(BindingFlags), flags);
                        parameters.Add(typeof(Binder), null);
                        parameters.Add(typeof(Type[]), typesParameter);
                        parameters.Add(typeof(ParameterModifier[]), null);
                    } else {
                        // Type[]
                        parameters.Add(typeof(Type[]), typesParameter);
                    }
                } else if (mi is MethodInfo mthdi) {
                    if (otherMemberCount > 0 && flags.NotIn(defaultLookups)) {
                        // string, BindingFlags, Binder, Type[], ParameterModifier[]
                        parameters.AddRange(
                            (typeof(BindingFlags), flags),
                            (typeof(Binder), null),
                            (typeof(Type[]), typesParameter),
                            (typeof(ParameterModifier[]), null)
                        );
                    } else {
                        // string
                        // string, Type[]
                        // string, BindingFlags
                        if (otherMemberCount > 0) { parameters.Add(typeof(Type[]), typesParameter); }
                        if (flags.NotIn(defaultLookups)) { parameters.Add(typeof(BindingFlags), flags); }
                    }
                } else {
                    throw new NotImplementedException();
                }
            }

            var methodName = mi switch
            {
                FieldInfo _ => "GetField",
                EventInfo _ => "GetEvent",
                PropertyInfo _ => "GetProperty",
                ConstructorInfo _ => "GetConstructor",
                MethodInfo _ => "GetMethod",
                _ => throw new NotImplementedException()
            };

            var method = reflectedType.GetType().GetMethod(methodName, parameters.Select(x => x.type).ToArray());
            return (method, parameters.Select(x => x.value).ToArray());
        }

        public static (MethodInfo method, object?[] args) GetInputs(this MethodInfo mi) {
            var reflectedType = mi.ReflectedType;
            var parameters = new List<(Type type, object? value)> {
                (typeof(string), mi.Name)
            };
            var flags = mi.getBindingFlags();
            var typesParameter = mi.GetParameters().Select(x => x.ParameterType).ToArray();
            var otherMemberCount = reflectedType.GetMethods(flags).Where(x => x.Name == mi.Name).Count() - 1;

            if (otherMemberCount > 0 && flags.NotIn(defaultLookups)) {
                // string, BindingFlags, Binder, Type[], ParameterModifier[]
                parameters.AddRange(
                    (typeof(BindingFlags), flags),
                    (typeof(Binder), null),
                    (typeof(Type[]), typesParameter),
                    (typeof(ParameterModifier[]), null)
                );
            } else {
                // string
                // string, Type[]
                // string, BindingFlags
                if (otherMemberCount > 0) { parameters.Add(typeof(Type[]), typesParameter); }
                if (flags.NotIn(defaultLookups)) { parameters.Add(typeof(BindingFlags), flags); }
            }

            var getMethod = reflectedType.GetType().GetMethod("GetMethod", parameters.Select(x => x.type).ToArray());
            return (getMethod, parameters.Select(x => x.value).ToArray());
        }
    }
}
