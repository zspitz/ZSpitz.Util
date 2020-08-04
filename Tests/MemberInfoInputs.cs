using System;
using System.Linq;
using Xunit;
using static System.Reflection.BindingFlags;
using ZSpitz.Util;
using System.Reflection;
using System.Diagnostics;

namespace Tests {
    public class MemberInfoInputs {
        private static Lazy<TheoryData<MemberInfo>> members = new Lazy<TheoryData<MemberInfo>>(() => 
            AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(x => x.FullName)
                .SelectMany(x => x.GetTypes()
                    .Where(x => !(
                        x.IsAnonymous() ||
                        (x.FullName?.StartsWith("Microsoft.CSharp.RuntimeBinder") ?? false) ||
                        (x.FullName?.StartsWith("Microsoft.VisualStudio.TestPlatform") ?? false)
                    ))
                )
                .SelectMany(x => x.GetMembers(Public | NonPublic | Static | Instance))
                .Where(x => 
                    !(x is MethodInfo mi && mi.IsSpecialName) &&
                    !(x is Type)
                )
                .ToTheoryData()
        );

        public static TheoryData<MemberInfo> TestData => members.Value;

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMemberInputs(MemberInfo mi) {
            if (Debugger.IsAttached) {
                Debugger.Log(5, "", $"{mi.ReflectedType?.FullName}.{mi.Name}\n");
            }
            var (getMethod, args) = mi.GetInputs();
            var invokeResult = getMethod.Invoke(mi.ReflectedType, args);
            Assert.Equal(mi, invokeResult);
        }
    }
}
