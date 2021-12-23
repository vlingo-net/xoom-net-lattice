using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Router
{
    public class CommandRouter__Proxy : IProxy, ICommandRouter
    {
        private const string RouteRepresentation1 =
            "Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer>)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public CommandRouter__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public CommandRouterType CommandRouterType => CommandRouterType.LoadBalancing;

        public void Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer> command)
            where TCommand : Model.Command
        {
            if (!actor.IsStopped)
            {
                Action<ICommandRouter> cons1478567733 = __ => __.Route<TProtocol, TCommand, TAnswer>(command);
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, cons1478567733, null, RouteRepresentation1);
                else
                    mailbox.Send(new LocalMessage<ICommandRouter>(actor, cons1478567733, RouteRepresentation1));
            }
            else
            {
                actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, RouteRepresentation1));
            }
        }


        public IAddress Address => actor.Address;

        public override bool Equals(object? other)
        {
            if (this == other) return true;
            if (other == null) return false;
            if (other.GetType() != GetType()) return false;
            return Address.Equals(other.FromRaw().Address);
        }

        public override int GetHashCode()
        {
            return 31 + GetType().GetHashCode() + actor.Address.GetHashCode();
        }

        public override string ToString()
        {
            return "ICommandRouter[Address=" + actor.Address + "]";
        }
    }
}