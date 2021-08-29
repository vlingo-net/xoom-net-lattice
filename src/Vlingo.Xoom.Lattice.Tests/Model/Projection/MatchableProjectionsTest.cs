// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using Vlingo.Xoom.Lattice.Model.Projection;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Model.Projection
{
    public class MatchableProjectionsTest
    {
        [Fact]
        public void TestThatEntireCauseMatches()
        {
            var matchable = new MatchableProjections();

            matchable.MayDispatchTo(new MockProjection(), new[] {"some-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"some-other-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"yet-another-matching-text"});

            Assert.Single(matchable.MatchProjections("some-matching-text"));
            Assert.Single(matchable.MatchProjections("some-other-matching-text"));
            Assert.Single(matchable.MatchProjections("yet-another-matching-text"));
        }

        [Fact]
        public void TestThatBeginsWithCauseMatches()
        {
            var matchable = new MatchableProjections();

            matchable.MayDispatchTo(new MockProjection(), new[] {"some-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"some-mat*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"some-other-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"some-other-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"some-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"yet-another-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"yet-another-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"yet-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"yet*"}); // note matches whole text "yet"

            Assert.Equal(3, matchable.MatchProjections("some-matching-text").Count());
            Assert.Equal(3, matchable.MatchProjections("some-other-matching-text").Count());
            Assert.Equal(4, matchable.MatchProjections("yet-another-matching-text").Count());
            Assert.Single(matchable.MatchProjections("yet")); // matched with "yet*"
        }

        [Fact]
        public void TestThatEndsWithCauseMatches()
        {
            var matchable = new MatchableProjections();

            matchable.MayDispatchTo(new MockProjection(), new[] {"*-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-other-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-another-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*text"}); // note matches whole text "text"

            Assert.Equal(7, matchable.MatchProjections("some-matching-text").Count());
            Assert.Equal(8, matchable.MatchProjections("some-other-matching-text").Count());
            Assert.Equal(8, matchable.MatchProjections("yet-another-matching-text").Count());
            Assert.Single(matchable.MatchProjections("text")); // matched with "text*"
        }

        [Fact]
        public void TestThatContainsCauseMatches()
        {
            var matchable = new MatchableProjections();

            matchable.MayDispatchTo(new MockProjection(), new[] {"*-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-other-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-another-matching-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*-*"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"*text*"});

            Assert.Equal(3, matchable.MatchProjections("some-matching-text").Count());
            Assert.Equal(4, matchable.MatchProjections("some-other-matching-text").Count());
            Assert.Equal(4, matchable.MatchProjections("yet-another-matching-text").Count());
            Assert.Single(matchable.MatchProjections("text")); // matched with "text*"
        }

        [Fact]
        public void TestThatNothingMatches()
        {
            var matchable = new MatchableProjections();

            matchable.MayDispatchTo(new MockProjection(), new[] {"other-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"another-matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"matching-text"});
            matchable.MayDispatchTo(new MockProjection(), new[] {"text"});

            Assert.Empty(matchable.MatchProjections("some-matching-text"));
            Assert.Empty(matchable.MatchProjections("some-other-matching-text"));
            Assert.Empty(matchable.MatchProjections("yet-another-matching-text"));
            Assert.Empty(matchable.MatchProjections("another"));
            Assert.Empty(matchable.MatchProjections("other"));
            Assert.Empty(matchable.MatchProjections("matching"));
        }
    }
}