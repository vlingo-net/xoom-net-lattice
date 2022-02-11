// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Dispatch;

namespace Vlingo.Xoom.Lattice.Tests.Model.Stateful;

public class MockTextDispatcher : IDispatcher
{
    private AccessSafely _access;

    private IDispatcherControl _control;
    private readonly Dictionary<string, TextState> _dispatched = new Dictionary<string, TextState>();
    private readonly ConcurrentBag<IEntry> _dispatchedEntries = new ConcurrentBag<IEntry>();
    private readonly AtomicBoolean _processDispatch = new AtomicBoolean(true);
        
    public MockTextDispatcher() => _access = AfterCompleting(0);

    public void ControlWith(IDispatcherControl control) => _control = control;
    public void Dispatch(Dispatchable dispatchable)
    {
        if (_processDispatch.Get())
        {
            var dispatchId = dispatchable.Id;
            _access.WriteUsing("dispatched", dispatchId, new Dispatch(dispatchable.TypedState<TextState>(), dispatchable.Entries));
        }
    }

    public AccessSafely AfterCompleting(int times)
    {
        _access = AccessSafely.AfterCompleting(times)
            .WritingWith<string, Dispatch>("dispatched", (id, dispatch) =>
            {
                _dispatched[id] = (TextState) dispatch.State;
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

    public List<Dispatchable> GetDispatched() => _access.ReadFrom<List<Dispatchable>>("dispatched");
}
    
internal class Dispatch
{
    public List<IEntry> Entries { get; }
    public IState State { get; }

    public Dispatch(IState state, List<IEntry> entries)
    {
        State = state;
        Entries = entries;
    }
}