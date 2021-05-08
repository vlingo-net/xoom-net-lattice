// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class DoCommand3Adapter : EntryAdapter<DoCommand3, TextEntry>
    {
        public override DoCommand3 FromEntry(TextEntry entry) => JsonSerialization.Deserialized<DoCommand3>(entry.EntryRawData);

        public override TextEntry ToEntry(DoCommand3 source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand3), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoCommand3 source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoCommand3), 1, serialization, version, metadata);
        }

        public override TextEntry ToEntry(DoCommand3 source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoCommand3), 1, serialization, metadata);
        }
    }
}