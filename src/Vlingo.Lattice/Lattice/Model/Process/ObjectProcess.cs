// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;
using Vlingo.Lattice.Model.Object;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Abstract base definition for all concrete object process types.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ObjectEntity{T}"/></typeparam>
    public abstract class ObjectProcess<T> : ObjectEntity<T>, IProcess<T> where T : StateObject
    {
        private readonly Info<T> _info;
        private readonly List<Source> _applied;
        
        public abstract Chronicle<T> Chronicle { get; }
        
        public abstract string ProcessId { get; }

        /// <summary>
        /// Construct my default state using my <code>address</code> as my <code>id</code>.
        /// </summary>
        public ObjectProcess() : this(null)
        {
        }
        
        protected ObjectProcess(string? id) : base(id)
        {
            _info = Stage.World.ResolveDynamic<ProcessTypeRegistry<T>>(ProcessTypeRegistry<T>.InternalName).Info();
            _applied = new List<Source>(2);
        }
        
        public void Process(Command command)
        {
            _applied.Add(command);
            Apply(Chronicle.State, new ProcessMessage(command));
        }
        
        public ICompletes<TResult> Process<TResult>(Command command, Func<TResult> andThen)
        {
            _applied.Add(command);
            return Apply(Chronicle.State, new ProcessMessage(command), andThen);
        }
        
        public void Process(DomainEvent @event)
        {
            _applied.Add(@event);
            Apply(Chronicle.State, new ProcessMessage(@event));
        }
        
        public ICompletes<TResult> Process<TResult>(DomainEvent @event, Func<TResult> andThen)
        {
            _applied.Add(@event);
            return Apply(Chronicle.State, new ProcessMessage(@event), andThen);
        }
        
        public void ProcessAll<TSource>(IEnumerable<Source<TSource>> sources)
        {
            var listSources = sources.ToList();
            _applied.AddRange(listSources);
            Apply(Chronicle.State, ProcessMessage.Wrap(listSources));
        }
        
        public ICompletes<TResult> ProcessAll<TResult, TSource>(IEnumerable<Source<TSource>> sources, Func<TResult> andThen)
        {
            var listSources = sources.ToList();
            _applied.AddRange(listSources);
            return Apply(Chronicle.State, ProcessMessage.Wrap(listSources), andThen);
        }

        public void Send(Command command) => _info.Exchange.Send(command);

        public void Send(DomainEvent @event) => _info.Exchange.Send(@event);

        protected override void AfterApply()
        {
            foreach (var source in _applied)
            {
                _info.Exchange.Send(source);
            }
            
            _applied.Clear();
        }
    }
}