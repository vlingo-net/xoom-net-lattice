// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;

namespace Vlingo.Tests.Lattice.Model.Stateful
{
    public class MockTextDispatcher : IDispatcher<Dispatchable<Entity1State, TextState>>
    {
        private AccessSafely _access;

        private IDispatcherControl _control;
        private readonly Dictionary<string, TextState> _dispatched = new Dictionary<string, TextState>();
        private readonly ConcurrentBag<Entity1State> _dispatchedEntries = new ConcurrentBag<Entity1State>();
        private readonly AtomicBoolean _processDispatch = new AtomicBoolean(true);
        
        public MockTextDispatcher() => _access = AfterCompleting(0);

        public void ControlWith(IDispatcherControl control) => _control = control;

        public void Dispatch(Dispatchable<Entity1State, TextState> dispatchable)
        {
            if (_processDispatch.Get())
            {
                var dispatchId = dispatchable.Id;
                _access.WriteUsing("dispatched", dispatchId, new Dispatch<TextState, Entity1State>(dispatchable.TypedState<TextState>(), dispatchable.Entries));
            }
        }
        
        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely.AfterCompleting(times)
                .WritingWith<string, Dispatch<TextState, Entity1State>>("dispatched", (id, dispatch) =>
                {
                    _dispatched[id] = dispatch.State;
                    foreach (var entry in dispatch.Entries)
                    {
                        _dispatchedEntries.Add(entry);
                    }
                })
                .ReadingWith("dispatchedIds", () => _dispatched.Keys)
                .ReadingWith<string, TextState>("dispatchedState", id => _dispatched[id])
                .ReadingWith("dispatchedStateCount", () => _dispatched.Count)

                .WritingWith<bool>("processDispatch", flag => _processDispatch.Set(flag))
                .ReadingWith("processDispatch", () => _processDispatch.Get())

                .ReadingWith("dispatched", () => _dispatched);

            return _access;
        }

        public List<Dispatchable<Entity1State, TextState>> GetDispatched() => _access.ReadFrom<List<Dispatchable<Entity1State, TextState>>>("dispatched");
    }
    
    internal class Dispatch<TState, TEntry>
    {
        public List<TEntry> Entries { get; }
        public TState State { get; }

        public Dispatch(TState state, List<TEntry> entries)
        {
            State = state;
            Entries = entries;
        }
    }
}