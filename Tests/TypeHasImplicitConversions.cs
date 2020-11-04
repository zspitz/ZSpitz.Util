using System;
using Xunit;
using ZSpitz.Util;

namespace Tests {
    public class TypeHasImplicitConversions
    {
        public static readonly TheoryData<Type, Type, bool> TestData = new[] {
            (typeof(long), typeof(int), false),
            (typeof(int), typeof(long), true),
            (typeof(sbyte), typeof(int), true),
            (typeof(float), typeof(double), true),
            (typeof(int), typeof(float), true)
        }.ToTheoryData();

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMethod(Type @base, Type target, bool expected) => 
            Assert.Equal(expected, @base.HasImplicitConversionTo(target));
    }
}
