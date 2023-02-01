// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Model.Projection;

public class ProjectionDispatcher__Proxy : IProjectionDispatcher
{
    private const string ProjectToRepresentation1 =
        "ProjectTo(Vlingo.Xoom.Lattice.Model.Projection.IProjection, System.String[])";

    private readonly Actor actor;
    private readonly IMailbox mailbox;

    public ProjectionDispatcher__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
        this.mailbox = mailbox;
    }

    public void ProjectTo(IProjection projection, string[] becauseOf)
    {
        if (!actor.IsStopped)
        {
            Action<IProjectionDispatcher> cons2068952794 = __ => __.ProjectTo(projection, becauseOf);
            if (mailbox.IsPreallocated)
                mailbox.Send(actor, cons2068952794, null, ProjectToRepresentation1);
            else
                mailbox.Send(
                    new LocalMessage<IProjectionDispatcher>(actor, cons2068952794, ProjectToRepresentation1));
        }
        else
        {
            actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ProjectToRepresentation1));
        }
    }
}