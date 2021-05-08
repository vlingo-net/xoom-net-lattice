// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Text;
using Vlingo.Lattice.Model.Projection;
using Vlingo.Xoom.Symbio;
using Xunit;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class BinaryProjectableTest
    {
        [Fact]
        public void TestProjectableness()
        {
            var textState = "test-state";
            var state = new BinaryState("123", typeof(string), 1, Encoding.UTF8.GetBytes(textState), 1, Metadata.With("value", "op"));
            var projectable = new BinaryProjectable(state, Enumerable.Empty<IEntry>(), "p123");

            Assert.Equal("op", projectable.BecauseOf()[0]);
            Assert.Equal(Encoding.UTF8.GetBytes(textState), projectable.DataAsBytes());
            Assert.Equal("123", projectable.DataId);
            Assert.Equal(1, projectable.DataVersion());
            Assert.Equal("value", projectable.Metadata);
            Assert.Equal("p123", projectable.ProjectionId);
            Assert.Equal(typeof(String).AssemblyQualifiedName, projectable.Type);
            Assert.Equal(1, projectable.TypeVersion);
        }
        
        [Fact]
        public void TestProjectableNotText()
        {
            var textState = "test-state";
            var state = new BinaryState("123", typeof(string), 1, Encoding.UTF8.GetBytes(textState), 1, Metadata.With("value", "op"));
            var projectable = new BinaryProjectable(state, Enumerable.Empty<IEntry>(), "p123");

            Assert.Throws<NotImplementedException>(() => projectable.DataAsText());
        }
    }
}