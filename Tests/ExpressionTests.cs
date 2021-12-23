using System;
using Xunit;
using static System.Linq.Expressions.Expression;
using ZSpitz.Util;
using System.Linq.Expressions;

namespace Tests {
    public class ExpressionTests {
        [Fact]
        public void ExtractValueFromLambda() {
            Expression<Func<bool>> expr = () => true;
            Assert.Equal(true, expr.ExtractValue());
        }

        [Fact]
        public void ExtractVariableFromLambda() {
            var b = new Random().NextDouble() >= .5;
            Expression<Func<bool>> expr = () => b;
            Assert.Equal(b, expr.ExtractValue());
        }

        [Fact]
        public void ExtractValueFromExpression() {
            var expr = Field(null, typeof(string).GetField("Empty")!);
            Assert.Equal("", expr.ExtractValue());
        }
    }
}
