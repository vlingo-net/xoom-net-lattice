using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Model.Stateful
{
    public class Entity1StateAdapter : StateAdapter<Entity1State, TextState>
    {
        public override int TypeVersion => 1;

        public override Entity1State FromRawState(TextState raw) => JsonSerialization.Deserialized<Entity1State>(raw.Data);

        public override TState FromRawState<TState>(TextState raw) => JsonSerialization.Deserialized<TState>(raw.Data);

        public override TextState ToRawState(Entity1State state, int stateVersion) => ToRawState(state.Id, state, stateVersion, Metadata.NullMetadata());

        public override TextState ToRawState(string id, Entity1State state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(id, typeof(Entity1State), TypeVersion, serialization, stateVersion);
        }
    }
}