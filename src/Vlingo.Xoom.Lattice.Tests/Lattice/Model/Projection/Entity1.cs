// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Model.Projection
{
    public class Entity1
    {
        public string Id { get; }
        
        public int Value { get; }

        public Entity1(string id, int value)
        {
            Id = id;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return Id.Equals(((Entity1) obj).Id);
        }

        public override int GetHashCode() => 31 * Id.GetHashCode();

        public override string ToString() => $"Entity1[id={Id} value={Value}]";
    }
    
    public class Entity1StateAdapter : StateAdapter<Entity1, TextState>
    {
        public override int TypeVersion => 1;
        
        public override Entity1 FromRawState(TextState raw) => JsonSerialization.Deserialized<Entity1>(raw.Data);

        public override TOtherState FromRawState<TOtherState>(TextState raw) => JsonSerialization.Deserialized<TOtherState>(raw.Data);

        public override TextState ToRawState(string id, Entity1 state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(id, typeof(Entity1), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(Entity1 state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(state.Id, typeof(Entity1), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(Entity1 state, int stateVersion) => ToRawState(state, stateVersion, Metadata.With("value", "op"));
    }
}