using System;
using Vlingo.Symbio;

namespace Vlingo.Tests.Lattice.Model.Stateful
{
    public class Entity1State : BaseEntry<string>, IEquatable<Entity1State>
    {   
        public Entity1State(): this(UnknownId, EmptyTextData, -1)
        {
        }
        
        public Entity1State(string id, string name, int age) : base(id, typeof(string), 1, EmptyTextData)
        {
            Name = name;
            Age = age;
        }

        public Entity1State(string id) : this(id, null, 0)
        {
        }

        public string Name { get; }
        public int Age { get; }

        public bool HasState => Id != null && Name != null && Age > 0;

        public Entity1State Copy() => new Entity1State(Id, Name, Age);

        public override string ToString() => $"Entity1State[id={Id} name={Name} age={Age}]";

        public Entity1State WithName(string name) => new Entity1State(Id, name, Age);

        public Entity1State WithAge(int age) => new Entity1State(Id, Name, age);

        public override IEntry<string> WithId(string id) => new Entity1State(id);

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            var otherState = (Entity1State) obj;
            return Id.Equals(otherState.Id) && Name.Equals(otherState.Name) && Age == otherState.Age;
        }

        public bool Equals(Entity1State other) => Id == other.Id && Name == other.Name && Age == other.Age;

        public override int GetHashCode() => HashCode.Combine(Id, Name, Age);
    }
}