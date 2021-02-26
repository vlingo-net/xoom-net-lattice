// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common.Serialization;
using Vlingo.Symbio;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class DoCommand1Adapter : EntryAdapter<DoCommand1, TextEntry>
    {
        public override DoCommand1 FromEntry(TextEntry entry) => JsonSerialization.Deserialized<DoCommand1>(entry.EntryRawData);

        public override TextEntry ToEntry(DoCommand1 source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand1), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoCommand1 source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand1), 1, serialization, version, metadata);
        }

        public override TextEntry ToEntry(DoCommand1 source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoCommand1), 1, serialization, metadata);
        }
    }
}