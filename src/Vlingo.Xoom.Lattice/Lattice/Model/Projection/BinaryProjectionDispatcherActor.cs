// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Symbio.Store.Dispatch;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    public class BinaryProjectionDispatcherActor : ProjectionDispatcherActor
    {
        public BinaryProjectionDispatcherActor() : this (Enumerable.Empty<ProjectToDescription>())
        {
        }
        
        public BinaryProjectionDispatcherActor(IEnumerable<ProjectToDescription> projectToDescriptions) : this(projectToDescriptions, MultiConfirming.DefaultExpirationLimit)
        {
        }
        
        public BinaryProjectionDispatcherActor(IEnumerable<ProjectToDescription> projectToDescriptions, long multiConfirmationsExpiration): base(projectToDescriptions, multiConfirmationsExpiration)
        {
        }

        public override void Dispatch(Dispatchable dispatchable)
        {
            dispatchable.State.IfPresent(state =>
            {
                if (HasProjectionsFor(state.Metadata.Operation))
                {
                    Dispatch(dispatchable.Id, new BinaryProjectable(state, dispatchable.Entries, dispatchable.Id));
                }
            });

            var entries = dispatchable.Entries.Where(entry => HasProjectionsFor(entry.TypeName)).ToList();

            if (entries.Any())
            {
                Dispatch(dispatchable.Id, new BinaryProjectable(dispatchable.State.OrElse(null!), entries, dispatchable.Id));
            }
        }
        
        protected override bool RequiresDispatchedConfirmation() => true;
    }
}