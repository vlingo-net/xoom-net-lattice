// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Vlingo.Lattice.Model.Projection;
using Vlingo.Symbio.Store.Dispatch;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class DescribedProjection : Actor, IProjection
    {
        public void ProjectWith(IProjectable projectable, IProjectionControl control) => 
            control.ConfirmProjected(projectable.ProjectionId);
    }

    public class Outcome : IDispatcherControl
    {
        public AtomicInteger Count { get; }
        public AccessSafely Access { get; private set; }
        
        public Outcome(int testUntilHappenings)
        {
            Count = new AtomicInteger(0);
            Access = AccessSafely.AfterCompleting(testUntilHappenings);
        }
        
        public void ConfirmDispatched(string dispatchId, IConfirmDispatchedResultInterest interest) => Access.WriteUsing("count", 1);

        public void DispatchUnconfirmed()
        {
        }

        public void Stop()
        {
        }
        
        public AccessSafely AfterCompleting(int times)
        {
            Access = AccessSafely.AfterCompleting(times);
            Access.WritingWith<int>("count", increment => Count.AddAndGet(increment))
                  .ReadingWith("count", () => Count.Get());

            return Access;
        }
    }
}