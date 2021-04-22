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
    public class Test1HappenedAdapter : EntryAdapter<Test1Happened, TextEntry>
    {
        public override Test1Happened FromEntry(TextEntry entry)
        {
            return JsonSerialization.Deserialized<Test1Happened>(entry.EntryRawData);
        }

        public override TextEntry ToEntry(Test1Happened source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test1Happened), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(Test1Happened source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test1Happened), version, serialization, metadata);
        }

        public override TextEntry ToEntry(Test1Happened source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(Test1Happened), version, serialization, metadata);
        }
    }
}