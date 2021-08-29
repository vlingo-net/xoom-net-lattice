// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Query.Fixtures.Store
{
    public interface ITestQueries
    {
        ICompletes<TestState> TestStateById(string id);

        ICompletes<TestState> TestStateById(string id, TestState notFoundState);

        ICompletes<TestState> TestStateById(string id, int retryInterval, int retryCount);

        ICompletes<TestState> TestStateById(string id, TestState notFoundState, int retryInterval, int retryCount);

        ICompletes<ObjectState<TestState>> TestObjectStateById(string id);

        ICompletes<ObjectState<TestState>> TestObjectStateById(string id, ObjectState<TestState> notFoundState);

        ICompletes<ObjectState<TestState>> TestObjectStateById(string id, int retryInterval, int retryCount);

        ICompletes<ObjectState<TestState>> TestObjectStateById(string id, ObjectState<TestState> notFoundState, int retryInterval, int retryCount);

        ICompletes<IEnumerable<TestState>> All(List<TestState> all);
    }
}