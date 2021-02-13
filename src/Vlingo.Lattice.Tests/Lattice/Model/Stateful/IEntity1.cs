using Vlingo.Common;

namespace Vlingo.Tests.Lattice.Model.Stateful
{
    public interface IEntity1
    {
        ICompletes<Entity1State> DefineWith(string name, int age);

        ICompletes<Entity1State> Current();

        void ChangeName(string name);

        void IncreaseAge();
    }
}