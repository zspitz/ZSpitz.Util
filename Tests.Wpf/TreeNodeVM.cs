using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZSpitz.Util.Wpf;

namespace Tests.Wpf {
    public class TreeNodeVMTests {
        [Fact]
        public void TestIsSelected() {
            var parent = new TreeNodeVM<int>(0);
            Assert.False(parent.IsSelected);

            var child1 = new TreeNodeVM<int>(1) {
                Parent = parent
            };
            Assert.Contains(child1, parent.Children);
            Assert.False(parent.IsSelected);

            child1.IsSelected = true;
            Assert.True(parent.IsSelected);

            var child2 = new TreeNodeVM<int>(2) {
                Parent = parent,
                IsSelected = false
            };
            Assert.Null(parent.IsSelected);

            parent.IsSelected = true;
            Assert.All(parent.Descendants(), child => Assert.True(child.IsSelected));

            parent.IsSelected = false;
            Assert.All(parent.Descendants(), child => Assert.False(child.IsSelected));
        }
    }
}
