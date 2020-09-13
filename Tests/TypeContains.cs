using System;
using System.Collections.Generic;
using Xunit;
using ZSpitz.Util;

namespace Tests {
    public class TypeContains {
        public static TheoryData<Type, Type> TestData = new TheoryData<Type, Type>() {
            {typeof(List<int>), typeof(List<int>) },
            {typeof(List<int>), typeof(int) },
            {typeof(List<int>), typeof(List<>) }
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public static void TestTypeContains(Type source, Type value) =>
            Assert.True(source.ContainsType(value));
    }
}
