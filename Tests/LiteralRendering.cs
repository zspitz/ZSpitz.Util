﻿using System;
using System.Collections.Generic;
using Xunit;
using static ZSpitz.Util.Functions;
using ZSpitz.Util;
using static ZSpitz.Util.LanguageNames;
using System.Reflection;

namespace Tests {
    [Trait("Type", "Literal rendering")]
    public class LiteralRendering {
        [Theory]
        [MemberData(nameof(TestData))]
        public static void TestLiteral(object o, string language, string expected) {
            var (actualHasLiteral, actual) = TryRenderLiteral(o, language);
            var expectedHasLiteral = !expected.StartsWith("#") || expected.EndsWith("#") || expected.Contains(':'); // because exceptions are rendered as #Exception:Message
            Assert.Equal(expectedHasLiteral, actualHasLiteral);
            Assert.Equal(expected, actual);
        }

        public static TheoryData<object?, string, string> TestData = IIFE(() => {
            var testData = new List<(object?, (string neutral, string csharp, string vb))>() {
                { null, ("␀", "null", "Nothing") },
                {5, ("5","5","5") },
                {17.2, ("17.2","17.2","17.2") },
                {true, ("True","true","True") },
                {false, ("False","false","False") },
                {'a', ("#Char","'a'","\"a\"C") },
                {"abcd", ("\"abcd\"", "\"abcd\"", "\"abcd\"") },
                {"ab\rcd", ("#String", "\"ab\\rcd\"", "\"ab\rcd\"") },
                {DayOfWeek.Thursday, ("DayOfWeek.Thursday","DayOfWeek.Thursday","DayOfWeek.Thursday") },
                {BindingFlags.Public | BindingFlags.Static, ("BindingFlags.Static, BindingFlags.Public", "BindingFlags.Static | BindingFlags.Public", "BindingFlags.Static Or BindingFlags.Public") },
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
                {typeof(string).MakeByRefType(), ("typeof(string).MakeByRef()", "GetType(String).MakeByRef()") },
                {timerType.GetConstructor(new Type[] { })!, ("typeof(Timer).GetConstructor(new Type[] { })", "GetType(Timer).GetConstructor({ })") },
                {timerType.GetEvent("Elapsed")!, ("typeof(Timer).GetEvent(\"Elapsed\")", "GetType(Timer).GetEvent(\"Elapsed\")")},
                {typeof(string).GetField("Empty")!, ("typeof(string).GetField(\"Empty\")", "GetType(String).GetField(\"Empty\")") },
                { GetMethod(() => Console.WriteLine()), ("typeof(Console).GetMethod(\"WriteLine\", new Type[] { })", "GetType(Console).GetMethod(\"WriteLine\", { })") },
                {GetMember(() => "".Length), ("typeof(string).GetProperty(\"Length\")", "GetType(String).GetProperty(\"Length\")") }
            }.SelectT((o, x) => {
                var (csharp, vb) = x;
                return (o, ($"#{o!.GetType().Name}", csharp, vb));
            }).AddRangeTo(testData);

            var dte = new DateTime(1981, 1, 1);
            testData.Add(dte, ("#DateTime", "#DateTime", $"#1981-01-01 00:00:00#"));

            var ret = new TheoryData<object?, string, string>();
            foreach (var (o, expected) in testData) {
                var (neutral, csharp, vb) = expected;
                ret.Add(o, "", neutral);
                ret.Add(o, CSharp, csharp);
                ret.Add(o, VisualBasic, vb);
            }

            // C# escaped-string tests; not relevant for Visual Basic
            ret.Add("\'\"\\\0\a\b\f\n\r\t\v", CSharp, "\"\\'\\\"\\\\\\0\\a\\b\\f\\n\\r\\t\\v\"");

            return ret;
        });
    }
}
