// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Query;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.State;

namespace Vlingo.Xoom.Lattice.Tests.Query.Fixtures.Store
{
    public class TestQueriesActor : StateStoreQueryActor<TestState>, ITestQueries
    {
        public TestQueriesActor(IStateStore stateStore) : base(stateStore)
        {
        }

        public ICompletes<TestState> TestStateById(string id) => QueryStateFor(id);

        public ICompletes<TestState> TestStateById(string id, TestState notFoundState) => QueryStateFor(id, notFoundState);

        public ICompletes<TestState> TestStateById(string id, int retryInterval, int retryCount) => QueryStateFor(id, retryInterval, retryCount);

        public ICompletes<TestState> TestStateById(string id, TestState notFoundState, int retryInterval, int retryCount) => QueryStateFor(id, notFoundState, retryInterval, retryCount);

        public ICompletes<ObjectState<TestState>> TestObjectStateById(string id) => QueryObjectStateFor(id);

        public ICompletes<ObjectState<TestState>> TestObjectStateById(string id, ObjectState<TestState> notFoundState) => QueryObjectStateFor(id, notFoundState);

        public ICompletes<ObjectState<TestState>> TestObjectStateById(string id, int retryInterval, int retryCount) => QueryObjectStateFor(id, retryInterval, retryCount);

        public ICompletes<ObjectState<TestState>> TestObjectStateById(string id, ObjectState<TestState> notFoundState, int retryInterval, int retryCount) => QueryObjectStateFor(id, notFoundState, retryInterval, retryCount);
        
        public ICompletes<IEnumerable<TestState>> All(List<TestState> all) => AllOf(all);
    }
}