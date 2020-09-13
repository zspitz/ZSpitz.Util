using Xunit;
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

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMemberInputs(MemberInfo mi) {
            var (getMethod, args) = mi.GetInputs();
            var invokeResult = getMethod.Invoke(mi.DeclaringType, args) as MemberInfo;
            if (Debugger.IsAttached && mi != invokeResult) {
                Debugger.Log(5, "", $"{mi.ReflectedType?.FullName}.{mi.Name}\n");
            }
            Assert.Equal(mi, invokeResult);
        }
    }
}
