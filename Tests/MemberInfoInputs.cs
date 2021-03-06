﻿using Xunit;
using static System.Reflection.BindingFlags;
using ZSpitz.Util;
using System.Reflection;
using System.Linq;
using System;
using System.Diagnostics;

namespace Tests {
    public class MemberInfoInputs {
        private static Lazy<TheoryData<MemberInfo>> members = new Lazy<TheoryData<MemberInfo>>(() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(x => x.FullName)
                .SelectMany(x => x.GetTypes()
                    .Where(x => !x.IsAnonymous())
                )
                .SelectMany(x => x.GetMembers(Public | NonPublic | Static | Instance))
                .Where(x =>
                    x.DeclaringType == x.ReflectedType &&
                    !(x is MethodInfo mi && mi.IsSpecialName) &&
                    !(x is Type)
                )
                .Take(1000)
                .ToTheoryData()
        );

        public static TheoryData<MemberInfo> TestData => members.Value;
        public static TheoryData<MethodInfo> HorrorTestData => typeof(Horror).GetMethods().ToTheoryData();
        public static TheoryData<MethodInfo> SpecialTestsData => new TheoryData<MethodInfo>() {
            typeof(string).GetMethod("Concat", new [] { typeof(string), typeof(string)})!
        };

        [Theory]
        //[MemberData(nameof(TestData))]
        [MemberData(nameof(HorrorTestData))]
        [MemberData(nameof(SpecialTestsData))]
        public void TestMemberInputs(MemberInfo mi) {
            var (methodOrName, args) = mi.GetInputs();
            if (methodOrName.IsT1 && args.OfType<int>().Any()) { return; } // .NET Framework, no GetMethod overload which takes generic parameter arity
            var invokeResult = methodOrName.AsT0.Invoke(mi.ReflectedType, args) as MemberInfo;
            if (Debugger.IsAttached && mi != invokeResult) {
                Debugger.Log(5, "", $"{mi.ReflectedType?.FullName}.{mi.Name}\n");
            }
            Assert.Equal(mi, invokeResult);
        }
    }
}
