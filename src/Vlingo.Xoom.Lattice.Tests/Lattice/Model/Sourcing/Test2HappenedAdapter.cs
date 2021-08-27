// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Model.Sourcing
{
    public class Test2HappenedAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => JsonSerialization.Deserialized<Test2Happened>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test2Happened), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test2Happened), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(Test2Happened), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(Test2Happened);
    }
}