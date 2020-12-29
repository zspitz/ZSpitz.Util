using OneOf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;
using ZSpitz.Util;
using static ZSpitz.Util.Functions;

namespace Tests {
    public class OneOfDerived1 : OneOfBase<string, int> {
        protected OneOfDerived1(OneOf<string, int> input) : base(input) { }
    }
    public class OneOfDerived2 : OneOfBase<
        string, int, double, float, Random,
        Exception, uint, StringBuilder, Regex, XElement,
        IEnumerable<string>, IEnumerable<int>
    > {
        protected OneOfDerived2(OneOf<
            string, int, double, float, Random,
            Exception, uint, StringBuilder, Regex, XElement,
            IEnumerable<string>, IEnumerable<int>
        > input) : base(input) { }
    }

    public class OneOfDerived3 : OneOfDerived2 {
        protected OneOfDerived3(OneOf<
            string, int, double, float, Random,
            Exception, uint, StringBuilder, Regex, XElement,
            IEnumerable<string>, IEnumerable<int>> input) : base(input) { }
    }

    public class OneOfType {
        public static TheoryData<Type, Type?, Type[]> TestData = IIFE(() => {
            var oneOfType = typeof(OneOf<string, int>);
            var oneofTypeExtended = typeof(OneOf<
                string, int, double, float, Random,
                Exception, uint, StringBuilder, Regex, XElement,
                IEnumerable<string>, IEnumerable<int>
            >);
            var oneofBaseExtended = typeof(OneOfBase<
                string, int, double, float, Random,
                Exception, uint, StringBuilder, Regex, XElement,
                IEnumerable<string>, IEnumerable<int>
            >);
            var subtypesExtended = new[] {
                typeof(string), typeof(int), typeof(double), typeof(float), typeof(Random),
                typeof(Exception), typeof(uint), typeof(StringBuilder), typeof(Regex), typeof(XElement),
                typeof(IEnumerable<string>), typeof(IEnumerable<int>)
            };


            return new TheoryData<Type, Type?, Type[]> {
                {typeof(int), null, new Type[0] },
                {oneOfType,  oneOfType, new[] { typeof(string), typeof(int) }},
                {oneofTypeExtended, oneofTypeExtended, subtypesExtended },
                {typeof(OneOfDerived1), typeof(OneOfBase<string, int>), new[] { typeof(string), typeof(int) } },
                {typeof(OneOfDerived2), oneofBaseExtended, subtypesExtended },
                {typeof(OneOfDerived3), oneofBaseExtended, subtypesExtended }
            };
        });

        [Theory]
        [MemberData(nameof(TestData))]
        public static void TestMethod(Type t, Type? oneOfType, Type[] subtypes) {
            var actual = t.OneOfType();
            Assert.Equal(oneOfType, actual);

            var actualSubtypes = t.OneOfSubtypes();
            Assert.Equal(subtypes, actualSubtypes);
        }
    }
}
