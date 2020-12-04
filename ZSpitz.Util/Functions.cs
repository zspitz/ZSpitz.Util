using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using static ZSpitz.Util.Language;
using static System.Globalization.CultureInfo;

namespace ZSpitz.Util {
    public static class Functions {
        public static (bool isLiteral, string repr) TryRenderLiteral(object? o, OneOf<string, Language?> languageArg) {
            var language = languageArg.ResolveLanguage();

            bool rendered = true;
            string? ret = null;
            if (o is null) {
                ret =
                    language == CSharp ? "null" :
                    language == VisualBasic ? "Nothing" :
                    "␀";
                return (rendered, ret);
            }

            var type = o.GetType().UnderlyingIfNullable();
            if (o is bool b) {
                if (language == CSharp) {
                    ret = b ? "true" : "false";
                } else { // Visual Basic and Boolean.ToString are the same
                    ret = b ? "True" : "False";
                }
            } else if (o is char c) {
                if (language == CSharp) {
                    ret = $"'{c}'";
                } else if (language == VisualBasic) {
                    ret = $"\"{c}\"C";
                }
            } else if ((o is DateTime) && language == VisualBasic) {
                ret = $"#{o:yyyy-MM-dd HH:mm:ss}#";
            } else if (o is string s) {
                if (language.In(CSharp, VisualBasic)) {
                    ret = s.ToVerbatimString(language);
                } else if (!s.HasSpecialCharacters()) {
                    ret = $"\"{s}\"";
                }
            } else if (o is Enum e) {
                if (type.HasAttribute<FlagsAttribute>()) {
                    var flagValues = e.GetIndividualFlags().ToArray();
                    var or = language switch
                    {
                        CSharp => " | ",
                        VisualBasic => " Or ",
                        _ => ", "
                    };
                    ret = flagValues.Joined(or, flagValue => $"{type.Name}.{flagValue}");
                } else {
                    // If GetIndividualFlags is used with a non-Flags enum, it returns multiple values.
                    // TODO this could probably be avoided by sorting the values in descending numeric order, then checking for the highest values first.
                    // At the very least, GetIndividualFlags should throw an Exception if used with a non-flags enum
                    ret = $"{type.Name}.{e}";
                }
            } else if (o is Type t && language.In(CSharp, VisualBasic)) {
                var typeOp = language == CSharp ? "typeof" : "GetType";

                // I'm not entirely sure a generic type definition type can have IsByRef == true
                bool isByRef = false;
                if (t.IsByRef) {
                    isByRef = true;
                    t = t.GetElementType()!;
                }

                if (t.IsGenericParameter) {
                    ret = $"Type.MakeGenericMethodParameter({t.GenericParameterPosition})";
                } else if (!t.ContainsGenericParameters) {
                    if (t.IsAnonymous()) {
                        ret = $"{typeOp}(<{(language == CSharp ? "a" : "A")}nonymous({t.FriendlyName(language)})>)";
                    } else {
                        ret = $"{typeOp}({t.FriendlyName(language)})";
                    }
                } else if (t.IsGenericTypeDefinition) {
                    if (language == CSharp) {
                        ret = $"{typeOp}({t.NonGenericName()}<{t.GetGenericArguments().Joined(",", _ => "")}>)";
                    } else {
                        ret = $"{typeOp}({t.NonGenericName()}(Of {t.GetGenericArguments().Joined(",", _ => "")}))";
                    }
                } else {
                    ret = $"{RenderLiteral(t.GetGenericTypeDefinition(), language)}.MakeGenericType({t.GenericTypeArguments.Joined(", ", x => RenderLiteral(x, language))})";
                }

                if (isByRef) { ret += ".MakeByRefType()"; }
            } else if (o is MemberInfo mi && language.In(CSharp, VisualBasic)) {
                var (method, args) = mi.GetInputs();
                var name = method.Match(mi => mi.Name, s => s);
                ret = $"{RenderLiteral(mi.ReflectedType, language)}.{name}({args.Joined(", ", x => RenderLiteral(x, language))})";
            } else if (type.IsArray && !type.GetElementType()!.IsArray && type.GetArrayRank() == 1 && language.In(CSharp, VisualBasic)) {
                var values = ((Array)o).Cast<object>().Joined(", ", x => RenderLiteral(x, language));
                values =
                    values.IsNullOrWhitespace() ?
                        " " :
                        $" {values} ";
                if (language == CSharp) {
                    var typename = values.IsNullOrWhitespace() ? " " + type.GetElementType()!.FriendlyName(language) : "";
                    ret = $"new{typename}[] {{{values}}}";
                } else {
                    ret = $"{{{values}}}";
                }
            } else if (type.IsTupleType(out var isTupleType)) {
                ret =
                    (!isTupleType ? "Tuple.Create" : "")
                    + "(" + TupleValues(o).Select(x => RenderLiteral(x, language)).Joined(", ") + ")";
            } else if (type.IsNumeric()) {
#if NET452
                ret = o.ToString();
#else
                ret = FormattableString.Invariant($"{o}");
#endif
            } else if (o is Exception ex) {
                ret = $"#{ex.GetType().FriendlyName(language)}:{RenderLiteral(ex.Message, language)}";
            }

            if (ret is null) {
                rendered = false;
                ret = $"#{type.FriendlyName(language)}";
            }
            return (rendered, ret);
        }

        public static string RenderLiteral(object? o, OneOf<string, Language?> languageArg) => TryRenderLiteral(o, languageArg).repr;

        /// <summary>Returns a string representation of the value, which may or may not be a valid literal in the language</summary>
        public static string StringValue(object? o, OneOf<string, Language?> languageArg) {
            var language = languageArg.ResolveLanguage();
            var (isLiteral, repr) = TryRenderLiteral(o, language);
            if (!isLiteral) {
                var hasDeclaredToString = o!.GetType().GetMethods().Any(x => {
                    if (x.Name != "ToString") { return false; }
                    if (x.GetParameters().Any()) { return false; }
                    if (x.DeclaringType == typeof(object)) { return false; }
                    if (x.DeclaringType!.InheritsFromOrImplements<EnumerableQuery>()) { return false; } // EnumerableQuery implements its own ToString which we don't want to use
                    return true;
                });
                if (hasDeclaredToString) { return o.ToString()!; }
            }
            return repr;
        }

        public static MethodInfo GetMethod(Expression<Action> expr, params Type[] typeargs) {
            var ret = ((MethodCallExpression)expr.Body).Method;
            // TODO handle partially open generic methods
            if (typeargs.Any() && ret.IsGenericMethod) {
                ret = ret.GetGenericMethodDefinition().MakeGenericMethod(typeargs);
            }
            return ret;
        }

        public static MemberInfo GetMember<T>(Expression<Func<T>> expr) => ((MemberExpression)expr.Body).Member;

        // TODO handle more than 8 values
        public static object?[] TupleValues(object tuple) {
            var type = tuple.GetType();
            if (!type.IsTupleType()) { throw new InvalidOperationException(); }
            var fields = type.GetFields();
            if (fields.Any()) { return type.GetFields().Select(x => x.GetValue(tuple)).ToArray(); }
            return type.GetProperties().Select(x => x.GetValue(tuple)).ToArray();
        }

        public static bool TryTupleValues(object tuple, [NotNullWhen(true)] out object?[]? values) {
            // this code cannot be simplified -- https://github.com/dotnet/roslyn/issues/44494
            if (tuple.GetType().IsTupleType()) {
                values = TupleValues(tuple);
                return true;
            }
            values = null;
            return false;
        }

        // based on https://github.com/dotnet/corefx/blob/7cf002ec36109943c048ec8da8ef80621190b4be/src/Common/src/CoreLib/System/Text/StringBuilder.cs#L1514
        public static (string literal, int? index, int? alignment, string? itemFormat)[] ParseFormatString(string format) {
            if (format == null) { throw new ArgumentNullException("format"); }

            const int indexLimit = 1000000;
            const int alignmentLimit = 100000;

            int pos = -1;
            char ch = '\x0';
            int lastPos = format.Length - 1;

            var parts = new List<(string literal, int? index, int? alignment, string? itemFormat)>();

            while (pos <= lastPos) {

                // Parse literal until argument placeholder
                string literal = "";
                while (pos < lastPos) {
                    advanceChar();

                    if (ch == '}') {
                        advanceChar();
                        if (ch == '}') {
                            literal += '}';
                        } else {
                            throw new FormatException("Missing start brace");
                        }
                    } else if (ch == '{') {
                        advanceChar();
                        if (ch == '{') {
                            literal += '{';
                        } else {
                            break;
                        }
                    } else {
                        literal += ch;
                    }
                }

                if (pos == lastPos) {
                    if (literal != "") {
                        parts.Add((literal, (int?)null, (int?)null, (string?)null));
                    }
                    break;
                }

                // Parse index section; required
                int index = getNumber(indexLimit);

                // Parse alignment; optional
                int? alignment = null;
                if (ch == ',') {
                    advanceChar();
                    alignment = getNumber(alignmentLimit, true);
                }

                // Parse item format; optional
                string? itemFormat = null;
                if (ch == ':') {
                    itemFormat = "";
                    while (true) {
                        advanceChar();
                        if (ch == '{') {
                            advanceChar();
                            if (ch == '{') {
                                itemFormat += '{';
                            } else {
                                throw new FormatException("Nested placeholders not allowed");
                            }
                        } else if (ch == '}') {
                            advanceChar(true);
                            if (ch == '}') {
                                itemFormat += '}';
                            } else {
                                break;
                            }
                        } else {
                            itemFormat += ch;
                        }
                    }
                }

                parts.Add((literal, index, alignment, itemFormat));
            }

            return parts.ToArray();

            void advanceChar(bool ignoreEnd = false) {
                pos += 1;
                if (pos <= lastPos) {
                    ch = format[pos];
                } else if (ignoreEnd) {
                    ch = '\x0';
                } else {
                    throw new FormatException("Unexpected end of text");
                }
            }

            void skipWhitespace() {
                while (ch == ' ') {
                    advanceChar(true);
                }
            }

            int getNumber(int limit, bool allowNegative = false) {
                skipWhitespace();

                bool isNegative = false;
                if (ch == '-') {
                    if (!allowNegative) { throw new FormatException("Negative number not allowed"); }
                    isNegative = true;
                    advanceChar();
                }
                if (ch < '0' || ch > '9') { throw new FormatException("Expected digit"); }
                int ret = 0;
                do {
                    ret = ret * 10 + ch - '0';
                    advanceChar();
                } while (ch >= '0' && ch <= '9' && ret < limit);

                skipWhitespace();

                return ret * (isNegative ? -1 : 1);
            }
        }

        public static KeyValuePair<TKey, TValue> KVP<TKey, TValue>(TKey key, TValue value) => new KeyValuePair<TKey, TValue>(key, value);

        public static bool IsReferenceComparison(ExpressionType nodeType, Expression left, Expression right, bool hasMethod) =>
            (nodeType == ExpressionType.Equal || nodeType == ExpressionType.NotEqual) &&
            !hasMethod &&
            !left.Type.IsValueType &&
            !right.Type.IsValueType;

        // TODO consider using Pather for this
        static readonly Regex re = new Regex(@"(?:^|\.)(\w+)(?:\[(\d+)\])?");
        public static object? ResolvePath(object? o, string path) {
            // note that we want to have a NullReferenceException if any parts of the path (except the last one) resolve to null
            foreach (var (propertyName, index) in re.Matches(path).Cast<Match>()) {
                o = o!.GetType().GetProperty(propertyName)!.GetValue(o);
                if (!index.IsNullOrWhitespace()) {
                    o = o!.GetType().GetIndexers(true).Single(x => x.GetIndexParameters().Single().ParameterType == typeof(int)).GetValue(o, new object[] { int.Parse(index) });
                }
            }
            return o;
        }

        public static string NewLines(int count = 2) => Enumerable.Repeat(Environment.NewLine, count).Joined("");

        public static T IIFE<T>(Func<T> fn) => fn();

        public static bool IsEqual<T>(T current, T newValue) => EqualityComparer<T>.Default.Equals(current, newValue);

        private static object makeTuple(bool valueTuple, object?[] elements, Type[]? types = null) {
            types ??= elements.Select(x =>
                x is null ?
                    typeof(object) :
                    x.GetType()
            ).ToArray();

            var tupleFactoryType =
                valueTuple ?
                    typeof(ValueTuple) :
                    typeof(Tuple);

            return tupleFactoryType
                .GetMethods()
                .First(x => x.Name == "Create" && x.GetGenericArguments().Length == types.Length)
                .MakeGenericMethod(types)
                .Invoke(null, elements)!;
        }

        public static object MakeTuple(params object[] elements) => makeTuple(true, elements);
        public static object MakeOldTuple(params object[] elements) => makeTuple(false, elements);
    }
}
