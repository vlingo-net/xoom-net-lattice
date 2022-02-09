// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Query.Fixtures.Store
{
    public class TestState
    {
        public static readonly string MISSING = "(missing)";

        public TestState()
        {
        }

        public string Id { get; set; }
        
        public string Name { get; set; }

        private TestState(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public static TestState NamedWithId(string name, string id) => new TestState(name, id);

        public static TestState Missing() => new TestState(MISSING, "-1");
    }
    
    public class TestStateAdapter : StateAdapter<TestState, TextState>
    {
        public override int TypeVersion => 1;
        
        public override TestState FromRawState(TextState raw) => JsonSerialization.Deserialized<TestState>(raw.Data);

        public override TOtherState FromRawState<TOtherState>(TextState raw) => JsonSerialization.Deserialized<TOtherState>(raw.Data);

        public override TextState ToRawState(string id, TestState state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(id, typeof(TestState), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(TestState state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(state.Id, typeof(TestState), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(TestState state, int stateVersion) => ToRawState(state, stateVersion, Metadata.With("value", "op"));
    }

    public class ObjectTestStateAdapter : StateAdapter<ObjectState<TestState>, TextState>
    {
        public override int TypeVersion => 1;

        public override ObjectState<TestState> FromRawState(TextState raw)
            => JsonSerialization.Deserialized<ObjectState<TestState>>(raw.Data);

        public override TOtherState FromRawState<TOtherState>(TextState raw)
            => JsonSerialization.Deserialized<TOtherState>(raw.Data);
        
        public override TextState ToRawState(string id, ObjectState<TestState> state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(id, typeof(ObjectState<TestState>), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(ObjectState<TestState> state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(state.Id, typeof(ObjectState<TestState>), TypeVersion, serialization, stateVersion, metadata);
        }

        public override TextState ToRawState(ObjectState<TestState> state, int stateVersion) => ToRawState(state, stateVersion, Metadata.With("value", "op"));

    }
}