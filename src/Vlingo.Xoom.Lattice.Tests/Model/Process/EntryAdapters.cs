// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Model.Process
{
    public class DoStepOneAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => 
            JsonSerialization.Deserialized<DoStepOne>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepOne), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepOne), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepOne), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(DoStepOne);
    }
    
    public class DoStepTwoAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => 
            JsonSerialization.Deserialized<DoStepTwo>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepTwo), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepTwo), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepTwo), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(DoStepTwo);
    }
    
    public class DoStepThreeAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => 
            JsonSerialization.Deserialized<DoStepThree>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepThree), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepThree), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepThree), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(DoStepThree);
    }
    
    public class DoStepFourAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => 
            JsonSerialization.Deserialized<DoStepFour>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFour), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFour), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepFour), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(DoStepFour);
    }
    
    public class DoStepFiveAdapter : EntryAdapter
    {
        public override ISource FromEntry(IEntry entry) => 
            JsonSerialization.Deserialized<DoStepFive>(entry.EntryRawData);

        public override IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFive), 1, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(typeof(DoStepFive), version, serialization, metadata);
        }

        public override IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(source);
            return new TextEntry(id, typeof(DoStepFive), version, serialization, metadata);
        }

        public override Type SourceType { get; } = typeof(DoStepFive);
    }
}