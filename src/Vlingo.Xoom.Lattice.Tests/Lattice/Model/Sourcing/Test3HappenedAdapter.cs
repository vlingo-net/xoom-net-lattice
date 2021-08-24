// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Model.Sourcing
{
    public class Test3HappenedAdapter : EntryAdapter<Test3Happened, TextEntry>
    {
        public override Test3Happened FromEntry(TextEntry entry)
        {
            return JsonSerialization.Deserialized<Test3Happened>(entry.EntryRawData);
        }

        public override TextEntry ToEntry(Test3Happened source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test3Happened), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(Test3Happened source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(Test3Happened), version, serialization, metadata);
        }

        public override TextEntry ToEntry(Test3Happened source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(Test3Happened), version, serialization, metadata);
        }
    }
}