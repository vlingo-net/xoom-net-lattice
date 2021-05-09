// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Projection;
using Vlingo.Xoom.Actors.TestKit;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class MockProjection : IProjection
    {
        private readonly AtomicInteger _projections = new AtomicInteger(0);
        public List<string> ProjectedDataIds { get; } = new List<string>();
        public AccessSafely Access { get; private set; } = AccessSafely.AfterCompleting(0);
        
        public void ProjectWith(IProjectable projectable, IProjectionControl control) => 
            Access.WriteUsing("projections", 1, projectable.DataId);

        public AccessSafely AfterCompleting(int times)
        {
            Access = AccessSafely.AfterCompleting(times);

            Access
                .WritingWith<int, string>("projections", (val, id) => {
                    _projections.Set(_projections.Get() + val);
                    ProjectedDataIds.Add(id);
                })
                .ReadingWith("projections", () => _projections.Get())
                .ReadingWith<int, string>("projectionId", index => ProjectedDataIds[index]);

            return Access;
        }
    }
}