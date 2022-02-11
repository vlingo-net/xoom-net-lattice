using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public class DistributedSpace__Proxy : Vlingo.Xoom.Actors.IProxy, Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace
{
    private const string LocalPutRepresentation1 =
        "LocalPut(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Item)";

    private const string LocalTakeRepresentation2 =
        "LocalTake(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)";

    private const string ItemForRepresentation3 = "ItemFor<T>(System.Type, System.Object[])";

    private const string PutRepresentation4 =
        "Put(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Item)";

    private const string GetRepresentation5 =
        "Get(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)";

    private const string TakeRepresentation6 =
        "Take(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)";

    private readonly Actor actor;
    private readonly IMailbox mailbox;

    public DistributedSpace__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
        this.mailbox = mailbox;
    }

    public Vlingo.Xoom.Common.ICompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem> LocalPut(
        Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Vlingo.Xoom.Lattice.Grid.Spaces.Item item)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons1210159281 = __ => __.LocalPut(key, item);
            var completes = new BasicCompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1210159281, completes, LocalPutRepresentation1);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons1210159281, completes, LocalPutRepresentation1));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, LocalPutRepresentation1));
        }

        return null!;
    }

    public Vlingo.Xoom.Common.ICompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem> LocalTake(
        Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Vlingo.Xoom.Lattice.Grid.Spaces.Period until)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons1092697423 =
                __ => __.LocalTake(key, until);
            var completes = new BasicCompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1092697423, completes, LocalTakeRepresentation2);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons1092697423, completes, LocalTakeRepresentation2));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, LocalTakeRepresentation2));
        }

        return null!;
    }

    public Vlingo.Xoom.Common.ICompletes<T> ItemFor<T>(System.Type actorType, System.Object[] parameters)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons398700987 = __ =>
                __.ItemFor<T>(actorType, parameters);
            var completes = new BasicCompletes<T>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons398700987, completes, ItemForRepresentation3);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons398700987, completes, ItemForRepresentation3));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, ItemForRepresentation3));
        }

        return null!;
    }

    public Vlingo.Xoom.Common.ICompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem> Put(
        Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Vlingo.Xoom.Lattice.Grid.Spaces.Item item)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons1545614305 = __ => __.Put(key, item);
            var completes = new BasicCompletes<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1545614305, completes, PutRepresentation4);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons1545614305, completes, PutRepresentation4));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, PutRepresentation4));
        }

        return null!;
    }

    public Vlingo.Xoom.Common.ICompletes<Vlingo.Xoom.Common.Optional<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>> Get(
        Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Vlingo.Xoom.Lattice.Grid.Spaces.Period until)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons1594774244 = __ => __.Get(key, until);
            var completes =
                new BasicCompletes<Vlingo.Xoom.Common.Optional<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>>(
                    this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1594774244, completes, GetRepresentation5);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons1594774244, completes, GetRepresentation5));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, GetRepresentation5));
        }

        return null!;
    }

    public Vlingo.Xoom.Common.ICompletes<Vlingo.Xoom.Common.Optional<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>> Take(
        Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Vlingo.Xoom.Lattice.Grid.Spaces.Period until)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace> cons2144414873 = __ => __.Take(key, until);
            var completes =
                new BasicCompletes<Vlingo.Xoom.Common.Optional<Vlingo.Xoom.Lattice.Grid.Spaces.KeyItem>>(
                    this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons2144414873, completes, TakeRepresentation6);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.IDistributedSpace>(this.actor,
                    cons2144414873, completes, TakeRepresentation6));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, TakeRepresentation6));
        }

        return null!;
    }


    public IAddress Address => this.actor.Address;

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
        return "IDistributedSpace[Address=" + this.actor.Address + "]";
    }
}