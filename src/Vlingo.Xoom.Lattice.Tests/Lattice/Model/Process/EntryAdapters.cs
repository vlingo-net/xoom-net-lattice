// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class DoStepOneAdapter : EntryAdapter<DoStepOne, TextEntry>
    {
        public override DoStepOne FromEntry(TextEntry entry) => 
            JsonSerialization.Deserialized<DoStepOne>(entry.EntryRawData);

        public override TextEntry ToEntry(DoStepOne source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepOne), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepOne source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepOne), version, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepOne source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepOne), version, serialization, metadata);
        }
    }
    
    public class DoStepTwoAdapter : EntryAdapter<DoStepTwo, TextEntry>
    {
        public override DoStepTwo FromEntry(TextEntry entry) => 
            JsonSerialization.Deserialized<DoStepTwo>(entry.EntryRawData);

        public override TextEntry ToEntry(DoStepTwo source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepTwo), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepTwo source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepTwo), version, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepTwo source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepTwo), version, serialization, metadata);
        }
    }
    
    public class DoStepThreeAdapter : EntryAdapter<DoStepThree, TextEntry>
    {
        public override DoStepThree FromEntry(TextEntry entry) => 
            JsonSerialization.Deserialized<DoStepThree>(entry.EntryRawData);

        public override TextEntry ToEntry(DoStepThree source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepThree), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepThree source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepThree), version, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepThree source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepThree), version, serialization, metadata);
        }
    }
    
    public class DoStepFourAdapter : EntryAdapter<DoStepFour, TextEntry>
    {
        public override DoStepFour FromEntry(TextEntry entry) => 
            JsonSerialization.Deserialized<DoStepFour>(entry.EntryRawData);

        public override TextEntry ToEntry(DoStepFour source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFour), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepFour source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFour), version, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepFour source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepFour), version, serialization, metadata);
        }
    }
    
    public class DoStepFiveAdapter : EntryAdapter<DoStepFive, TextEntry>
    {
        public override DoStepFive FromEntry(TextEntry entry) => 
            JsonSerialization.Deserialized<DoStepFive>(entry.EntryRawData);

        public override TextEntry ToEntry(DoStepFive source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFive), 1, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepFive source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFive), version, serialization, metadata);
        }

        public override TextEntry ToEntry(DoStepFive source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepFive), version, serialization, metadata);
        }
    }
}