using Xunit;
using static System.Reflection.BindingFlags;
using ZSpitz.Util;
using System.Reflection;
using System.Diagnostics;

namespace Tests {
    public class MemberInfoInputs {
        public static readonly TheoryData<MemberInfo> TestData = 
            typeof(Foo).GetMembers(Public | NonPublic | Instance | Static).ToTheoryData();

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
