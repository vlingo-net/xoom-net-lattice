// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    public class ProcessMessageTextAdapter : IEntryAdapter
    {
        public ISource FromEntry(IEntry entry)
        {
            try
            {
                var serializedMessage = JsonSerialization.Deserialized<SerializableProcessMessage>(entry.EntryRawData);
                var source = JsonSerialization.Deserialized<ISource>(serializedMessage.Source);
                return new ProcessMessage(source);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"ProcessMessageTextAdapter failed because: {e.Message}", e);
            }
        }

        public IEntry ToEntry(ISource source) => ToEntry(source, Metadata.NullMetadata());

        public IEntry ToEntry(ISource source, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage((ProcessMessage) source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(typeof(ProcessMessage), 1, serialization, metadata);
        }

        public IEntry ToEntry(ISource source, int version, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage((ProcessMessage) source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(typeof(ProcessMessage), 1, serialization, version, metadata);
        }

        public IEntry ToEntry(ISource source, string id) => ToEntry(source, id, Metadata.NullMetadata());

        public IEntry ToEntry(ISource source, string id, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage((ProcessMessage) source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(id, typeof(ProcessMessage), 1, serialization, metadata);
        }

        public IEntry ToEntry(ISource source, int version, string id, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage((ProcessMessage) source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(id, typeof(ProcessMessage), 1, serialization, version, metadata);
        }

        public Type SourceType { get; } = typeof(ProcessMessage);

        private sealed class SerializableProcessMessage
        {
            public string Source { get; }

            public SerializableProcessMessage(ProcessMessage message)
            {
                Source = SourceToText(message.Source);
            }

            private string SourceToText(ISource? source)
            {
                var sourceJson = JsonSerialization.Serialized(source);
                return sourceJson;
            }
        }

        public ISource AnyTypeFromEntry(IEntry entry) => FromEntry((TextEntry) entry);
    }
}