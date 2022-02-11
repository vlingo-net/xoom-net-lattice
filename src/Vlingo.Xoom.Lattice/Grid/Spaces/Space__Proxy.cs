using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public class Space__Proxy : ActorProxyBase, Vlingo.Xoom.Actors.IProxy, Vlingo.Xoom.Lattice.Grid.Spaces.ISpace
{
    private const string ItemForRepresentation1 = "ItemFor<T>(System.Type, System.Object[])";
    private const string PutRepresentation2 = "Put<T>(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Item<T>)";

    private const string GetRepresentation3 =
        "Get<T>(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)";

    private const string TakeRepresentation4 =
        "Take<T>(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)";

    private readonly Actor actor;
    private readonly IMailbox mailbox;

    public Space__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
        this.mailbox = mailbox;
    }

    public ICompletes<T> ItemFor<T>(System.Type actorType, System.Object[] parameters)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace> cons193199966 = __ =>
                __.ItemFor<T>(actorType, parameters);
            var completes = new BasicCompletes<T>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons193199966, completes, ItemForRepresentation1);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace>(this.actor,
                    cons193199966, completes, ItemForRepresentation1));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, ItemForRepresentation1));
        }

        return null!;
    }

    public ICompletes<KeyItem> Put(Vlingo.Xoom.Lattice.Grid.Spaces.IKey key, Item item)
    {
        if (!this.actor.IsStopped)
        {
            ActorProxyBase self = this;
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace> cons262655915 = a => a.Put(Thunk(self, (Actor) a, key), Thunk(self, (Actor) a, item));
            var completes = Completes.Using<KeyItem>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons262655915, completes, PutRepresentation2);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace>(this.actor,
                    cons262655915, completes, PutRepresentation2));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, PutRepresentation2));
        }

        return null!;
    }

    public ICompletes<Optional<KeyItem>> Get(Vlingo.Xoom.Lattice.Grid.Spaces.IKey key,
        Vlingo.Xoom.Lattice.Grid.Spaces.Period until)
    {
        if (!this.actor.IsStopped)
        {
            ActorProxyBase self = this;
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace> cons1247307923 = a => a.Get(Thunk(self, (Actor) a, key), Thunk(self, (Actor) a, until));
            var completes = Completes.Using<Optional<KeyItem>>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1247307923, completes, GetRepresentation3);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace>(this.actor,
                    cons1247307923, completes, GetRepresentation3));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, GetRepresentation3));
        }

        return null!;
    }

    public ICompletes<Optional<KeyItem>> Take(Vlingo.Xoom.Lattice.Grid.Spaces.IKey key,
        Vlingo.Xoom.Lattice.Grid.Spaces.Period until)
    {
        if (!this.actor.IsStopped)
        {
            Action<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace> cons1640817233 = __ => __.Take(key, until);
            var completes = new BasicCompletes<Optional<KeyItem>>(this.actor.Scheduler);
            if (this.mailbox.IsPreallocated)
            {
                this.mailbox.Send(this.actor, cons1640817233, completes, TakeRepresentation4);
            }
            else
            {
                this.mailbox.Send(new LocalMessage<Vlingo.Xoom.Lattice.Grid.Spaces.ISpace>(this.actor,
                    cons1640817233, completes, TakeRepresentation4));
            }

            return completes;
        }
        else
        {
            this.actor.DeadLetters?.FailedDelivery(new DeadLetter(this.actor, TakeRepresentation4));
        }

        return null!;
    }


    public new IAddress Address => this.actor.Address;

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
        return "ISpace[Address=" + this.actor.Address + "]";
    }
}