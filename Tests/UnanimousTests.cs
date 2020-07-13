using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZSpitz.Util;

namespace Tests {
    public class UnanimousTests {
        [Fact]
        public void TestTrue() {
            var src = Enumerable.Repeat(true,5);
            Assert.True(src.Unanimous());
        }

        [Fact]
        public void TestFalse() {
            var src = Enumerable.Repeat(false, 5);
            Assert.False(src.Unanimous());
        }

        [Fact]
        public void TestNull() {
            var src = Enumerable.Repeat((bool?)null, 5);
            Assert.Null(src.Unanimous());
        }

        [Fact]
        public void TestEmpty() {
            var src = Enumerable.Empty<bool?>();
            Assert.Null(src.Unanimous());
        }

        [Fact]
        public void TestNonUnanimous() {
            var src = new bool[] { true, true, false, false, true };
            Assert.False(src.Unanimous());
        }

        [Fact]
        public void TestOther() {
            var src = Enumerable.Empty<bool?>();
            Assert.False(src.Unanimous(false));
        }

    }
}
