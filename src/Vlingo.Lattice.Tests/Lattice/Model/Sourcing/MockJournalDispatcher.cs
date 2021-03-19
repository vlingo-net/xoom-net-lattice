// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class MockJournalDispatcher : IDispatcher<Dispatchable<IEntry, IState>>
    {
        private AccessSafely _access;
        private readonly List<IEntry> _entries = new List<IEntry>();
        
        public MockJournalDispatcher() => _access = AfterCompleting(0);
        
        public void ControlWith(IDispatcherControl control)
        {
        }

        public void Dispatch(Dispatchable<IEntry, IState> dispatchable) =>
            _access.WriteUsing("appendedAll", dispatchable.Entries);

        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely
                .AfterCompleting(times)
  
                .WritingWith("appended", (IEntry appended) => _entries.Add(appended))
                .WritingWith("appendedAll", (List<IEntry> appended) => _entries.AddRange(appended))
                .ReadingWith<int, IEntry>("appendedAt", index => _entries[index])
                .ReadingWith("entries", () => _entries)
                .ReadingWith("entriesCount", () => _entries.Count);

            return _access;
        }
    }
}