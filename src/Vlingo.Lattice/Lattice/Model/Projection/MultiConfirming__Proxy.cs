// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Lattice.Model.Projection
{
    public class MultiConfirming__Proxy : IMultiConfirming
    {
        private const string ManageConfirmationsForRepresentation1 =
            "ManageConfirmationsFor(Vlingo.Lattice.Model.Projection.IProjectable, int)";

        private const string ManagedConfirmationsRepresentation2 = "ManagedConfirmations()";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public MultiConfirming__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void ManageConfirmationsFor(IProjectable projectable, int count)
        {
            if (!actor.IsStopped)
            {
                Action<IMultiConfirming> cons1665207899 = __ => __.ManageConfirmationsFor(projectable, count);
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, cons1665207899, null, ManageConfirmationsForRepresentation1);
                else
                    mailbox.Send(new LocalMessage<IMultiConfirming>(actor, cons1665207899,
                        ManageConfirmationsForRepresentation1));
            }
            else
            {
                actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ManageConfirmationsForRepresentation1));
            }
        }

        public ICompletes<IEnumerable<IProjectable>> ManagedConfirmations()
        {
            if (!actor.IsStopped)
            {
                Action<IMultiConfirming> cons859132867 = __ => __.ManagedConfirmations();
                var completes = new BasicCompletes<IEnumerable<IProjectable>>(actor.Scheduler);
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, cons859132867, completes, ManagedConfirmationsRepresentation2);
                else
                    mailbox.Send(new LocalMessage<IMultiConfirming>(actor, cons859132867, completes,
                        ManagedConfirmationsRepresentation2));
                return completes;
            }

            actor.DeadLetters?.FailedDelivery(new DeadLetter(actor, ManagedConfirmationsRepresentation2));
            return null;
        }
    }
}