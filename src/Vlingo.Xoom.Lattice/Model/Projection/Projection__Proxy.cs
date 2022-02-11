// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Model.Projection;

public class Projection__Proxy : IProjection
{
    private const string ProjectWithRepresentation1 =
        "ProjectWith(Vlingo.Xoom.Lattice.Model.Projection.IProjectable, Vlingo.Xoom.Lattice.Model.Projection.IProjectionControl)";

    private readonly Actor actor;
    private readonly IMailbox mailbox;

    public Projection__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
        this.mailbox = mailbox;
    }

    public void ProjectWith(IProjectable projectable, IProjectionControl control)
    {
        if (!actor.IsStopped)
        {
            Action<IProjection> cons1613435108 = __ => __.ProjectWith(projectable, control);
            if (mailbox.IsPreallocated)
                mailbox.Send(actor, cons1613435108, null, ProjectWithRepresentation1);
            else
                mailbox.Send(new LocalMessage<IProjection>(actor, cons1613435108, ProjectWithRepresentation1));
        }
        else
        {
            actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ProjectWithRepresentation1));
        }
    }
}