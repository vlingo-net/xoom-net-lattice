// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Symbio;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class DoCommand2Adapter : EntryAdapter<DoCommand2, TextEntry>
    {
        public override DoCommand2 FromEntry(TextEntry entry) => JsonSerialization.Deserialized<DoCommand2>(entry.EntryRawData);

        public override TextEntry ToEntry(DoCommand2 source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand2), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoCommand2 source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand2), 1, serialization, version, metadata);
        }

        public override TextEntry ToEntry(DoCommand2 source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoCommand2), 1, serialization, metadata);
        }
    }
}