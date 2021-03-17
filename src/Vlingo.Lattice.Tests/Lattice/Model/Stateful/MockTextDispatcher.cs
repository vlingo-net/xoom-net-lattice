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
    public class MockTextDispatcher<TEntry, TState> : IDispatcher<Dispatchable<TEntry, TState>> where TEntry : IEntry where TState : class, IState
    {
        private AccessSafely _access;

        private IDispatcherControl _control;
        private readonly Dictionary<string, TState> _dispatched = new Dictionary<string, TState>();
        private readonly ConcurrentBag<TEntry> _dispatchedEntries = new ConcurrentBag<TEntry>();
        private readonly AtomicBoolean _processDispatch = new AtomicBoolean(true);
        
        public MockTextDispatcher() => _access = AfterCompleting(0);

        public void ControlWith(IDispatcherControl control) => _control = control;
        public void Dispatch(Dispatchable<TEntry, TState> dispatchable)
        {
            if (_processDispatch.Get())
            {
                var dispatchId = dispatchable.Id;
                _access.WriteUsing("dispatched", dispatchId, new Dispatch<TState, TEntry>(dispatchable.TypedState<TState>(), dispatchable.Entries));
            }
        }

        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely.AfterCompleting(times)
                .WritingWith<string, Dispatch<TState, TEntry>>("dispatched", (id, dispatch) =>
                {
                    _dispatched[id] = dispatch.State;
                    foreach (var entry in dispatch.Entries)
                    {
                        _dispatchedEntries.Add(entry);
                    }
                })
                .ReadingWith("dispatchedIds", () => _dispatched.Keys)
                .ReadingWith<string, TState>("dispatchedState", id => _dispatched[id])
                .ReadingWith("dispatchedStateCount", () => _dispatched.Count)

                .WritingWith<bool>("processDispatch", flag => _processDispatch.Set(flag))
                .ReadingWith("processDispatch", () => _processDispatch.Get())

                .ReadingWith("dispatched", () => _dispatched);

            return _access;
        }

        public List<Dispatchable<TEntry, TState>> GetDispatched() => _access.ReadFrom<List<Dispatchable<TEntry, TState>>>("dispatched");
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