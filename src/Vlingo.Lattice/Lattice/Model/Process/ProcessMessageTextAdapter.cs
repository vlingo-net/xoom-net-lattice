// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Serialization;
using Vlingo.Symbio;

namespace Vlingo.Lattice.Model.Process
{
    public class ProcessMessageTextAdapter : IEntryAdapter<ProcessMessage, TextEntry>
    {
        public ProcessMessage FromEntry(TextEntry entry)
        {
            try
            {
                var serializedMessage = JsonSerialization.Deserialized<SerializableProcessMessage>(entry.EntryData);
                var source = JsonSerialization.Deserialized<ISource>(serializedMessage.Source);
                return new ProcessMessage(source);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"ProcessMessageTextAdapter failed because: {e.Message}", e);
            }
        }

        public TextEntry ToEntry(ProcessMessage source) => ToEntry(source, Metadata.NullMetadata());

        public TextEntry ToEntry(ProcessMessage source, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage(source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(typeof(ProcessMessage), 1, serialization, metadata);
        }

        public TextEntry ToEntry(ProcessMessage source, int version, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage(source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(typeof(ProcessMessage), 1, serialization, version, metadata);
        }

        public TextEntry ToEntry(ProcessMessage source, string id) => ToEntry(source, id, Metadata.NullMetadata());

        public TextEntry ToEntry(ProcessMessage source, string id, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage(source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(id, typeof(ProcessMessage), 1, serialization, metadata);
        }

        public TextEntry ToEntry(ProcessMessage source, int version, string id, Metadata metadata)
        {
            var serializedMessage = new SerializableProcessMessage(source);
            var serialization = JsonSerialization.Serialized(serializedMessage);
            return new TextEntry(id, typeof(ProcessMessage), 1, serialization, version, metadata);
        }
        
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
    }
}