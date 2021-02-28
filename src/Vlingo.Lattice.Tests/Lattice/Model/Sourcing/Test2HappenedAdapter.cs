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
    public class Test2HappenedAdapter : EntryAdapter<Test2Happened, TextEntry>
    {
        public override Test2Happened FromEntry(TextEntry entry)
        {
            return JsonSerialization.Deserialized<Test2Happened>(entry.EntryRawData);
        }

        public override TextEntry ToEntry(Test2Happened source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test2Happened), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(Test2Happened source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test2Happened), version, serialization, metadata);
        }

        public override TextEntry ToEntry(Test2Happened source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(Test2Happened), version, serialization, metadata);
        }
    }
}