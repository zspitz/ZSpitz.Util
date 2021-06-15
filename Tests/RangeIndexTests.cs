using System;
using Xunit;

namespace Tests {
    public class RangeIndexTests {
        static string[] words = new[] {
            "The",
            "quick",
            "brown",
            "fox",
            "jumped",
            "over",
            "the",
            "lazy",
            "dog"
        };

        public static TheoryData<Index, string> IndexData = new (Index, string)[] {
            (0, "The"),
            (5, "over"),
            (^1, "dog"),
            (^5, "jumped")
        }.ToTheoryData();

        [Theory]
        [MemberData(nameof(IndexData))]
        public void TestIndex(Index index, string result) => Assert.Equal(result, words[index]);

        // each position can have a positive index, negative index, or open
        public static TheoryData<Range, string[]> RangeData = new (Range, string[])[] {
            (0..1, new[] {"The" }),
            (^7..4, new[] {"brown", "fox" }),
            (..4, new[] {"The", "quick", "brown", "fox" }),
            (5..^2, new[] {"over", "the" }),
            (^5..^2, new[] {"jumped", "over", "the" }),
            (..^6, new[] {"The","quick","brown"}),
            (7.., new[] {"lazy", "dog" }),
            (^2.., new[] {"lazy", "dog"}),
            (.., words)
        }.ToTheoryData();

        [Theory]
        [MemberData(nameof(RangeData))]
        public void TestRange(Range rng, string[] result) => Assert.Equal(result, words[rng]);
    }
}
