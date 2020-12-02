using System;
using System.Collections.Generic;
using Xunit;
using ZSpitz.Util;
using static ZSpitz.Util.ConversionStrategy;

namespace Tests {
    public class TypeHasImplicitConversions {
        public static readonly TheoryData<Type, Type, bool, ConversionStrategy> TestData = new[] {
            (typeof(long), typeof(int), false, None),
            (typeof(int), typeof(long), true, BuiltIn),
            (typeof(sbyte), typeof(int), true, BuiltIn),
            (typeof(float), typeof(double), true, BuiltIn),
            (typeof(int), typeof(float), true, BuiltIn),
            (typeof(double), typeof(float), false, None),
            (typeof(string), typeof(IEnumerable<char>), true, Assignable),
            (typeof(IEnumerable<char>), typeof(string), false, None)
        }.ToTheoryData();

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMethod(Type @base, Type target, bool expected, ConversionStrategy expectedStrategy) {
            Assert.Equal(expected, @base.HasImplicitConversionTo(target));
            var conversion = @base.GetImplicitConversionTo(target);
            Assert.Equal(expectedStrategy, conversion.strategy);
            Assert.Equal(conversion.strategy == Method, conversion.method is { });
        }
    }
}
