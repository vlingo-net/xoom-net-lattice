// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;

namespace Vlingo.Tests.Lattice.Model.Object
{
    public class MockDispatcher : IDispatcher<IDispatchable<IEntry, IState>>
    {
        public void ControlWith(IDispatcherControl control)
        {
        }

        public void Dispatch(IDispatchable<IEntry, IState> dispatchable)
        {
        }
    }
}