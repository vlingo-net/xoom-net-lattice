// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Common;
using Vlingo.Lattice.Model;
using Vlingo.Lattice.Model.Object;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Tests.Model.Process
{
    public abstract class ObjectProcess<T> : ObjectEntity<T>, IProcess<T> where T : StateObject
    {
        private Info<ObjectProcess<T>> _info;
        
        public Chronicle<T> Chronicle { get; }
        
        public string Id { get; }
        
        public void Process(Command command)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TResult> Process<TResult>(Command command, Func<TResult> andThen)
        {
            throw new NotImplementedException();
        }

        public void Process(DomainEvent @event)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TResult> Process<TResult>(DomainEvent @event, Func<TResult> andThen)
        {
            throw new NotImplementedException();
        }

        public void ProcessAll<TSource>(IEnumerable<Source<TSource>> sources)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TResult> ProcessAll<TResult, TSource>(IEnumerable<Source<TSource>> sources, Func<TResult> andThen)
        {
            throw new NotImplementedException();
        }

        public void Send(Command command)
        {
            throw new NotImplementedException();
        }

        public void Send(DomainEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}