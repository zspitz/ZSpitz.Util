using System;
using System.Collections.Generic;
using Xunit;
using static ZSpitz.Util.Functions;
using ZSpitz.Util;
using static ZSpitz.Util.Language;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

namespace Tests {
    [Trait("Type", "Literal rendering")]
    public class LiteralRendering {
        [Theory]
        [MemberData(nameof(TestData))]
        public static void TestLiteral(object o, Language? language, string expected) {
            var (actualHasLiteral, actual) = TryRenderLiteral(o, language);
            var expectedHasLiteral = !expected.StartsWith("#") || expected.EndsWith("#") || expected.Contains(':'); // because exceptions are rendered as #Exception:Message
            Assert.Equal(expectedHasLiteral, actualHasLiteral);
            Assert.Equal(expected, actual);
        }

        public static TheoryData<object?, Language?, string> TestData = IIFE(() => {
            var testData = new List<(object?, (string neutral, string csharp, string vb))>() {
                { null, ("␀", "null", "Nothing") },
                {5, ("5","5","5") },
                {17.2, ("17.2","17.2","17.2") },
                {-5, ("-5","-5","-5") },
                {-17.2, ("-17.2","-17.2","-17.2") },
                {0.2, ("0.2","0.2","0.2") },
                {true, ("True","true","True") },
                {false, ("False","false","False") },
                {'a', ("#Char","'a'","\"a\"C") },
                {"abcd", ("\"abcd\"", "\"abcd\"", "\"abcd\"") },
                {"ab\rcd", ("#String", "\"ab\\rcd\"", "\"ab\rcd\"") },
                {DayOfWeek.Thursday, ("DayOfWeek.Thursday","DayOfWeek.Thursday","DayOfWeek.Thursday") },
                {BindingFlags.Public | BindingFlags.Static, ("BindingFlags.Static, BindingFlags.Public", "BindingFlags.Static | BindingFlags.Public", "BindingFlags.Static Or BindingFlags.Public") },
                {ExpressionType.Throw, ("ExpressionType.Throw", "ExpressionType.Throw", "ExpressionType.Throw")},
                {new object[] {1}, ("#Object[]", "new[] { 1 }", "{ 1 }")},
                {Tuple.Create(1,"2"), ("Tuple.Create(1, \"2\")", "Tuple.Create(1, \"2\")", "Tuple.Create(1, \"2\")") },
                {(1,"2"), ("(1, \"2\")", "(1, \"2\")", "(1, \"2\")") },
                {"\"", ("#String", "\"\\\"\"", "\"\"\"\"") },
                {new InvalidOperationException("This is a message."), ("#InvalidOperationException:\"This is a message.\"", "#InvalidOperationException:\"This is a message.\"", "#InvalidOperationException:\"This is a message.\"") },
                {new InvalidOperationException("This is a message\non two lines."), ("#InvalidOperationException:#String", "#InvalidOperationException:\"This is a message\\non two lines.\"", "#InvalidOperationException:\"This is a message\non two lines.\"") }
            };

            var timerType = typeof(System.Timers.Timer);

            // populate with reflection test data
            new List<(object?, (string csharp, string vb))>() {
                {typeof(string), ("typeof(string)", "GetType(String)") },
                {typeof(string).MakeByRefType(), ("typeof(string).MakeByRefType()", "GetType(String).MakeByRefType()") },
                {timerType.GetConstructor(new Type[] { })!, ("typeof(Timer).GetConstructor(new Type[] { })", "GetType(Timer).GetConstructor({ })") },
                {timerType.GetEvent("Elapsed"), ("typeof(Timer).GetEvent(\"Elapsed\")", "GetType(Timer).GetEvent(\"Elapsed\")")},
                {typeof(string).GetField("Empty"), ("typeof(string).GetField(\"Empty\")", "GetType(String).GetField(\"Empty\")") },
                { GetMethod(() => Console.WriteLine()), ("typeof(Console).GetMethod(\"WriteLine\", new Type[] { })", "GetType(Console).GetMethod(\"WriteLine\", { })") },
                {GetMember(() => "".Length), ("typeof(string).GetProperty(\"Length\")", "GetType(String).GetProperty(\"Length\")") },

                // generic type definition
                {
                    typeof(Dictionary<,>), ("typeof(Dictionary<,>)", "GetType(Dictionary(Of ,))")
                },

                // constructed generic type
                {
                    typeof(Dictionary<string, int>), ("typeof(Dictionary<string, int>)", "GetType(Dictionary(Of String, Integer))")
                },

                // nested constructed generic type
                {
                    typeof(List<Dictionary<string, int>>), ("typeof(List<Dictionary<string, int>>)", "GetType(List(Of Dictionary(Of String, Integer)))")
                }

#if NETCOREAPP3_1                
                ,

                // generic type parameter
                {
                    Type.MakeGenericMethodParameter(5), ("Type.MakeGenericMethodParameter(5)","Type.MakeGenericMethodParameter(5)")
                },

                // partial generic type
                {
                    typeof(Dictionary<,>).MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(1)),
                    (
                        "typeof(Dictionary<,>).MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(1))",
                        "GetType(Dictionary(Of ,)).MakeGenericType(GetType(String), Type.MakeGenericMethodParameter(1))"
                    )
                },

                // nested partial generic type
                {
                    typeof(List<>).MakeGenericType(
                        typeof(Dictionary<,>).MakeGenericType(                        
                            typeof(string), Type.MakeGenericMethodParameter(1)
                        )
                    ),
                    (
                        "typeof(List<>).MakeGenericType(typeof(Dictionary<,>).MakeGenericType(typeof(string), Type.MakeGenericMethodParameter(1)))",
                        "GetType(List(Of )).MakeGenericType(GetType(Dictionary(Of ,)).MakeGenericType(GetType(String), Type.MakeGenericMethodParameter(1)))"
                    )
                },

                // MethodInfo with generic parameter
                {
                    new Func<MethodInfo>(() => {
                        var p0 = Type.MakeGenericMethodParameter(0);
                        return typeof(Enumerable).GetMethod("Contains", new[] {
                            typeof(IEnumerable<>).MakeGenericType(p0),
                            p0
                        })!;
                    })(),
                    (
                        @"typeof(Enumerable).GetMethod(""Contains"", new[] {
typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)),
Type.MakeGenericMethodParameter(0)
})".Replace(Environment.NewLine," "),
                        @"GetType(Enumerable).GetMethod(""Contains"", {
GetType(IEnumerable(Of )).MakeGenericType(Type.MakeGenericMethodParameter(0)),
Type.MakeGenericMethodParameter(0)
})".Replace(Environment.NewLine," ")
                    )
                }
#endif
            }.SelectT((o, x) => {
                var (csharp, vb) = x;
                return (o, ($"#{o!.GetType().Name}", csharp, vb));
            }).AddRangeTo(testData);

            var dte = new DateTime(1981, 1, 1);
            testData.Add(dte, ("#DateTime", "#DateTime", $"#1981-01-01 00:00:00#"));

            var ret = new TheoryData<object?, Language?, string>();
            foreach (var (o, expected) in testData) {
                var (neutral, csharp, vb) = expected;
                ret.Add(o, null, neutral);
                ret.Add(o, CSharp, csharp);
                ret.Add(o, VisualBasic, vb);
            }

            // C# escaped-string tests; not relevant for Visual Basic
            ret.Add("\'\"\\\0\a\b\f\n\r\t\v", CSharp, "\"\\'\\\"\\\\\\0\\a\\b\\f\\n\\r\\t\\v\"");

            return ret;
        });
    }
}
