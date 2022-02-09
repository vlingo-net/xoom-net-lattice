// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Lattice.Model.Projection;
using Vlingo.Xoom.Symbio;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Model.Projection
{
    public class ProjectableTest
    {
        [Fact]
        public void TestThatNoStateFailsSafely()
        {
            var projectable = new TextProjectable(null, new List<IEntry>(), "123");

            Assert.Equal(-1, projectable.DataVersion());
            Assert.Equal("", projectable.DataId);
            Assert.Equal("", projectable.Metadata);
            Assert.False(projectable.HasObject);
            Assert.Null(projectable.Object<object>());
            Assert.False(projectable.OptionalObject<object>().IsPresent);
            Assert.False(projectable.HasState);
            Assert.Equal(-1, projectable.TypeVersion);
        }
        
        [Fact]
        public void TestThatStateDoesNotFail()
        {
            var @object = new object();
            var projectable =
                new TextProjectable(
                    new TextState("ABC", typeof(string), 1, "state", 1, Metadata.With(@object, "value", "op1")),
            new List<IEntry>(), "123");

            Assert.Equal(1, projectable.DataVersion());
            Assert.Equal("ABC", projectable.DataId);
            Assert.Equal("value", projectable.Metadata);
            Assert.True(projectable.HasObject);
            Assert.NotNull(projectable.Object<object>());
            Assert.Equal(@object, projectable.Object<object>());
            Assert.True(projectable.OptionalObject<object>().IsPresent);
            Assert.True(projectable.HasState);
            Assert.Equal(1, projectable.TypeVersion);
        }
    }
}