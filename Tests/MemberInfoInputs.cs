using Xunit;
using static System.Reflection.BindingFlags;
using ZSpitz.Util;
using System.Reflection;
using System.Linq;

namespace Tests {
    public class MemberInfoInputs {
        public static readonly TheoryData<MemberInfo> TestData = 
            typeof(Foo)
                .GetMembers(Public | NonPublic | Instance | Static)
                .Where(x => !(x is MethodInfo mi && mi.IsSpecialName))
                .ToTheoryData();

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMemberInputs(MemberInfo mi) {
            var (getMethod, args) = mi.GetInputs();
            var invokeResult = getMethod.Invoke(mi.ReflectedType, args);
            Assert.Equal(mi, invokeResult);
        }
    }
}
