// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Model.Projection;

public class ProjectionControl__Proxy : IProjectionControl
{
    private const string ConfirmerForRepresentation1 =
        "ConfirmerFor(Vlingo.Xoom.Lattice.Model.Projection.IProjectable, Vlingo.Xoom.Lattice.Model.Projection.IProjectionControl)";

    private const string ConfirmProjectedRepresentation2 = "ConfirmProjected(string)";

    private readonly Actor actor;
    private readonly IMailbox mailbox;

    public ProjectionControl__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
        this.mailbox = mailbox;
    }

    public Confirmer ConfirmerFor(IProjectable projectable, IProjectionControl control)
    {
        if (!actor.IsStopped)
        {
            Action<IProjectionControl> cons609358827 = __ => __.ConfirmerFor(projectable, control);
            if (mailbox.IsPreallocated)
                mailbox.Send(actor, cons609358827, null, ConfirmerForRepresentation1);
            else
                mailbox.Send(
                    new LocalMessage<IProjectionControl>(actor, cons609358827, ConfirmerForRepresentation1));
        }
        else
        {
            actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ConfirmerForRepresentation1));
        }

        return null!;
    }

    public void ConfirmProjected(string projectionId)
    {
        if (!actor.IsStopped)
        {
            Action<IProjectionControl> cons842506682 = __ => __.ConfirmProjected(projectionId);
            if (mailbox.IsPreallocated)
                mailbox.Send(actor, cons842506682, null, ConfirmProjectedRepresentation2);
            else
                mailbox.Send(new LocalMessage<IProjectionControl>(actor, cons842506682,
                    ConfirmProjectedRepresentation2));
        }
        else
        {
            actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ConfirmProjectedRepresentation2));
        }
    }
}